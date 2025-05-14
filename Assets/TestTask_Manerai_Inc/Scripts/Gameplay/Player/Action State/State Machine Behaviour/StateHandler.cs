using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class StateHandler: StateMachineBehaviour
    {
        public int stateType;

        public bool movementEnabled;
        public bool rotationEnabled;

        public bool autoRotate;

        public bool movementState;
        public bool landingState;

        private bool[] values = new bool[4]; // avoid creating new arrays at runtime to prevent garbage generation

        // =========================================================
        //    Component Dependencies
        // =========================================================

        private PlayerController playerController;

        // =========================================================
        //    Animation Parameters
        // =========================================================

        readonly int hashStateTime = Animator.StringToHash("StateTime");

        readonly int hashStateType = Animator.StringToHash("StateType");

        readonly int hashAutoRotate = Animator.StringToHash("AutoRotate");

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

                // =========================================================

                playerController.ResetAllTriggers();

                animator.SetInteger(hashStateType, stateType);

                animator.SetFloat(hashStateTime, 0f);

                animator.SetBool(hashAutoRotate, autoRotate);

                // =========================================================

                MeleeAttack meleeAttack = playerController.GetMeleeAttack();

                if (!autoRotate)
                {
                    meleeAttack.ClearTargets();
                }

                meleeAttack.CancelHitstop(); // fail-safe for when player flinches or gets launched in the air mid-animation

                // =========================================================

                values[0] = movementEnabled;
                values[1] = rotationEnabled;
                values[2] = autoRotate;
                values[3] = movementState;

                playerController.SetStateValues(values);

                // =========================================================

                if (landingState)
                {
                    playerController.PlayLandingAudio();
                }
            }
        }
    }
}









