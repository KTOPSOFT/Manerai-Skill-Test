using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class ActionStateHandler: StateMachineBehaviour
    {   
        public int actionState = 0;

        public float actionSpeed = 1.0f;

        public bool animationLock;

        public bool autoTarget;

        public bool chargedAttack;

        public bool lastAttack;

        public int staminaCost;

        public int energyCost;

        // =========================================================

        [Header("Overrides")]

        public float turnSmoothTime = -1.0f;

        public float cameraSmoothTime = -1.0f;

        public float resetDelay = -1.0f;

        public WeaponSound[] weaponSounds;

        // =========================================================
        //    Component Dependencies
        // =========================================================

        protected PlayerController playerController;

        // =========================================================
        //    Animator Parameters
        // =========================================================

        readonly int hashStateType = Animator.StringToHash("StateType");

        readonly int hashIsGrounded = Animator.StringToHash("IsGrounded");

        readonly int hashAutoRotate = Animator.StringToHash("AutoRotate");

        readonly int hashLastAttack = Animator.StringToHash("LastAttack");

        // =========================================================
        //    State Machine Methods
        // =========================================================

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (layerIndex == 0) // use only on base layer states
            {
                if (playerController == null)
                {
                    playerController = animator.GetComponent<PlayerController>();
                }

                playerController.SetTurnSmoothTime(turnSmoothTime);

                playerController.ResetEventState();

                // =========================================================

                animator.speed = actionSpeed;

                animator.SetBool(hashLastAttack, lastAttack);

                // =========================================================

                MeleeAttack meleeAttack = playerController.GetMeleeAttack();

                meleeAttack.EndAllAttacks();

                meleeAttack.SetWeaponSoundOverrides(weaponSounds);

                // ==============================================================

                if (playerController.localEvent)
                {
                    meleeAttack.SetAnimationLock(animationLock);

                    meleeAttack.SetNextAction(false);
                    meleeAttack.SetSkillReady(false);

                    meleeAttack.SetResetDelay(resetDelay);

                    // =========================================================

                    if (chargedAttack)
                    {
                        meleeAttack.ChargedAttackEvent();
                    }

                    if (staminaCost > 0)
                    {
                        playerController.UseStamina(staminaCost);
                    }

                    if (energyCost > 0)
                    {
                        Attributes attributes = playerController.GetAttributes();

                        attributes.UpdateEnergy(-energyCost);
                    }

                    // =========================================================

                    bool autoRotate = animator.GetBool(hashAutoRotate);

                    if (autoRotate)
                    {
                        playerController.AutoRotate(autoTarget);
                    }

                    // =========================================================

                    int playerCount = playerController.GetPlayerCount();

                    bool validEvent = !playerController.GetMovementState() && !playerController.soloInstance && playerCount > 1;

                    if (validEvent && animator.GetBool(hashIsGrounded))
                    {
                        float targetAngle = playerController.GetTargetAngle();

                        playerController.SyncTransform();

                        playerController.SendNetworkTrigger(actionState, targetAngle);
                    }
                }

                // =========================================================

                int stateType = animator.GetInteger(hashStateType);

                bool meleeAction = (stateType == 1);

                bool validState = meleeAction || (!meleeAction && actionState > 0);

                playerController.SetSimulationState(validState);

                if (meleeAction && actionState == 1)
                {
                    meleeAttack.ResetChargedAttackTimer();
                }

                // =========================================================

                RootMotionSimulator rms = playerController.GetRootMotionSimulator();

                rms.StopTimer();

                rms.actionState = actionState;

                if (actionState > 0)
                {  
                    rms.StartTimer();

                    float smoothTime = cameraSmoothTime;

                    if (smoothTime < 0f)
                        smoothTime = 0.1f;

                    SetSmoothTime(smoothTime);
                }

                else
                {
                    rms.ResetScanDistance();

                    SetSmoothTime(0.05f);
                }
            }
        }

        private void SetSmoothTime(float value)
        {
            if (playerController.GetActivePlayer())
            {
                SceneHandler sceneHandler = playerController.GetSceneHandler();

                sceneHandler.cameraTarget.SetSmoothTime(value);
            }
        }
    }
}









