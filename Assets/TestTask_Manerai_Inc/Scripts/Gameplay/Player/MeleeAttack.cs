using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class MeleeAttack : MonoBehaviour
    {   
        public Transform weapons;

        public bool forceAutoTarget;

        public float autoTargetRange = 3.0f;

        public float chargedAttackStaminaCost = 20.0f;

        private bool disableAllInputs;
        private bool disableMeleeInput;

        private bool isAttacking;

        [Range(0f, 1.0f)]
        public float voiceChance = 0.33f;

        [Space(10)]

        public LaunchValue[] launchValues = new LaunchValue[] { new LaunchValue(7.0f, 1.2f) };

        [Space(10)]

        public int[] attackTypes = new int[] { 0, 1 };

        private IEnumerator hitstopCoroutine;
        private IEnumerator skillReadyCoroutine;

        // =========================================================
        //    Timer Variables
        // =========================================================

        private float chargedAttackTimer;
        private float bufferTimer;

        private static float chargeTime = 0.33f;
        private static float bufferTime = 0.33f;

        private bool chargedAttackInput;

        private bool chargeAvailable;
        private bool buffering;

        private bool animationLock;

        private bool nextAction;
        private bool skillReady;

        private int attackIndex;

        private int trigger;

        private float resetDelay;

        // =========================================================
        //    Component Dependencies
        // =========================================================

        private Animator animator;

        private PlayerController playerController;
        private RootMotionSimulator rms;

        private SceneHandler sceneHandler;

        // =========================================================
        //    Weapon Parameters
        // =========================================================

        private TargetCollider currentTarget;

        private List<Target> targetsHit = new List<Target>();

        private List<Weapon> weaponList = new List<Weapon>();

        private WeaponSound[] weaponSoundOverrides = new WeaponSound[0];

        // =========================================================
        //    Animation Parameters
        // =========================================================

        readonly int hashStateType = Animator.StringToHash("StateType");
        readonly int hashActionState = Animator.StringToHash("ActionState");

        readonly int hashAttackType = Animator.StringToHash("AttackType");

        readonly int hashInputDetected = Animator.StringToHash("InputDetected");

        readonly int hashLastAttack = Animator.StringToHash("LastAttack");

        readonly int hashMeleeAttackA = Animator.StringToHash("MeleeAttackA");
        readonly int hashMeleeAttackB = Animator.StringToHash("MeleeAttackB");

        readonly int hashChargedAttack = Animator.StringToHash("ChargedAttack");

        readonly int hashNextAction = Animator.StringToHash("NextAction");

        // =========================================================
        //    Standard Methods
        // =========================================================

        public void Initialize() // called by PlayerController;
        {
            GetComponents();

            InitializeWeaponList();
        }

        private void GetComponents()
        {
            // Player Components
            animator = GetComponent<Animator>();

            playerController = GetComponent<PlayerController>();
            rms = GetComponent<RootMotionSimulator>();

            // Scene Components
            sceneHandler = playerController.GetSceneHandler();
        }

        private void InitializeWeaponList()
        {
            if (weapons != null)
            {
                int childCount = weapons.childCount;

                for (int i = 0; i < childCount; i ++)
                {
                    weaponList.Add(weapons.GetChild(i).GetComponent<Weapon>());

                    weaponList[i].Initialize(playerController, sceneHandler);
                }
            }
        }

        private void Update()
        {
            if (playerController.GetActivePlayer() && !disableAllInputs)
            {
                MeleeInput();
            }
        }

        private void MeleeInput()
        {
            if (!disableMeleeInput)
            {
                var mouse = Mouse.current;
                var keyboard = Keyboard.current;

                bool inputA = mouse.leftButton.wasPressedThisFrame || GamepadButtonDown.North();
                bool inputB = mouse.rightButton.wasPressedThisFrame || GamepadButtonDown.East();
                bool inputC = mouse.forwardButton.wasPressedThisFrame || GamepadButtonDown.RightTrigger();

                // =========================================================

                int inputValue = -10;

                if (inputA)
                    inputValue = -1;

                else if (inputB)
                    inputValue = attackTypes[0];

                else if (inputC)
                    inputValue = attackTypes[1];

                // =========================================================

                if (inputValue >= -1)
                {
                    trigger = inputValue < 0 ? hashMeleeAttackA : hashMeleeAttackB;

                    // =========================================================

                    bool validInput = ValidInput();

                    if (validInput)
                    {
                        ResetTriggersA();

                        animator.SetInteger(hashAttackType, inputValue);

                        animator.SetInteger(hashActionState, rms.actionState);
                        animator.SetTrigger(trigger);

                        RunBufferTimer(true);
                    }

                    // =========================================================

                    if (inputValue < 0)
                    {
                        chargedAttackTimer = 0f;

                        chargeAvailable = true;
                    }
                }

                else if (chargedAttackInput)
                {
                    chargedAttackInput = false;

                    bool meleeInputHeld = mouse.leftButton.isPressed || GamepadButtonHeld.North();

                    if (meleeInputHeld)
                    {
                        TriggerChargedAttack();
                    }
                }

                // =========================================================

                UpdateTimers(mouse);
            }
        }

        public bool ValidInput() // called internally and by PlayerController.cs
        {
            bool lastAttack = animator.GetBool(hashLastAttack);

            bool attackInput = trigger == hashMeleeAttackA;

            // =========================================================

            bool validInputA = !lastAttack || (lastAttack && (!attackInput || nextAction));

            bool validInputB = (!animationLock && validInputA) || (animationLock && nextAction);

            return validInputB;
        }

        private void UpdateTimers(Mouse mouse)
        {   
            float deltaTime = Time.unscaledDeltaTime;

            if (buffering)
            {
                bufferTimer += deltaTime;

                if (bufferTimer >= bufferTime)
                {
                    ResetTriggersB();

                    RunBufferTimer(false);
                }

                else if (skillReady && trigger != hashMeleeAttackA)
                {
                    animator.SetTrigger(hashNextAction);
                }
            }

            // ===========================================

            if (chargeAvailable)
            {
                bool meleeInputHeld = mouse.leftButton.isPressed || GamepadButtonHeld.North();

                if (meleeInputHeld)
                {
                    if (deltaTime <= 0.1f)
                    {
                        chargedAttackTimer += deltaTime;
                    }

                    // ===========================================

                    if (chargedAttackTimer >= chargeTime && nextAction && !playerController.GetIsTired())
                    {   
                        // Debug.Log("timer = " + chargedAttackTimer + " / deltaTime = " + deltaTime);

                        chargedAttackInput = true;

                        ResetChargedAttack();
                    }
                }

                else
                {
                    ResetChargedAttack();
                }
            }
        }

        private void ResetChargedAttack()
        {
            animator.ResetTrigger(hashChargedAttack);

            chargedAttackTimer = 0f;

            chargeAvailable = false;
        }

        private void RunBufferTimer(bool value)
        {
            bufferTimer = 0f;

            buffering = value;
        }

        public void ResetAllTriggers() // called by PlayerController.cs
        {
            ResetTriggersA();
            ResetTriggersB();

            animator.ResetTrigger(hashChargedAttack);

            animator.ResetTrigger(hashNextAction);
        }

        private void ResetTriggersA() // reset any trigger that is not the melee attack triggers (theoretical examples provided in comments)
        {
            // animator.ResetTrigger(hashDrawAttack);

            // animator.ResetTrigger(hashDodge);

            // animator.ResetTrigger(hashDrawWeapon);
            // animator.ResetTrigger(hashSheatheWeapon);
        }

        private void ResetTriggersB()
        {
            animator.ResetTrigger(hashMeleeAttackA);

            animator.ResetTrigger(hashMeleeAttackB);
        }

        private void TriggerChargedAttack()
        {
            ResetTriggersA();

            bool lastAttack = animator.GetBool(hashLastAttack);

            if (!lastAttack)
            {
                animator.SetInteger(hashActionState, rms.actionState);

                animator.SetTrigger(hashChargedAttack);
            }
        }

        public void ChargedAttackEvent() // called by ActionStateHandler.cs
        {
            playerController.UseStamina(chargedAttackStaminaCost);
        }

        private void AttackStart(AnimationEvent currentEvent) // called by animation event
        {
            int weaponIndex = (int) currentEvent.floatParameter;

            Weapon weapon = weaponList[weaponIndex];

            // =========================================================

            if (animator.GetInteger(hashStateType) == 1)
            {
                if (weaponIndex < weaponList.Count && weaponList[weaponIndex] != null)
                {
                    isAttacking = true;

                    int actionState = rms.actionState;

                    weapon.BeginAttack(actionState, attackIndex);

                    ClearTargets();

                    // =========================================================

                    if (attackIndex == 0)
                    {
                        AudioList voiceAudioList = playerController.GetVoiceAudioList();

                        if (voiceAudioList != null)
                        {
                            float randomChance = Random.Range(0, 1.0f);

                            if (randomChance <= voiceChance)
                            {
                                bool playOneShot = false;

                                AudioPlayer voiceAudioPlayer = playerController.voiceAudioPlayer;

                                voiceAudioPlayer.PlayFromAudioList(voiceAudioList, playerController, playOneShot);
                            }
                        }
                    }
                }

                attackIndex ++;
            }

            // =========================================================

            bool playSound = true;

            string stringParameter = currentEvent.stringParameter;

            if (stringParameter.Length > 0)
            {
                char c = stringParameter[0];

                if (c == 'e' || c == 'E')
                    playerController.EventState();

                else if (c == 'm' || c == 'M')
                    playSound = false;
            }

            // =========================================================

            if (playSound)
            {
                PlayWeaponSound(weapon, attackIndex - 1);
            }
        }

        private void PlayWeaponSound(Weapon weapon, int attackIndex)
        {
            if (weaponSoundOverrides.Length > attackIndex)
            {   
                WeaponSound weaponSound = weaponSoundOverrides[attackIndex];

                int index = weaponSound.index;

                if (index >= 0)
                {
                    if (weaponSound.useAudioSequence)
                    {
                        PlayTracks(weapon, index);
                    }

                    else
                    {
                        weapon.PlayFromAudioList(index);
                    }
                }
            }

            else
            {
                weapon.PlayFromAudioList();
            }
        }

        private void WeaponSound(AnimationEvent currentEvent) // called by animation event
        {
            int weaponIndex = (int) currentEvent.floatParameter;

            Weapon weapon = weaponList[weaponIndex];

            PlayWeaponSound(weapon, attackIndex);
        }

        private void AttackEnd(AnimationEvent currentEvent) // called by animation event
        {
            int weaponIndex = (int) currentEvent.floatParameter;

            AttackEndEvent(weaponIndex, true);

            // =========================================================

            string stringParameter = currentEvent.stringParameter;

            if (stringParameter.Length > 0)
            {
                char c = stringParameter[0];

                if (c == 'n' || c == 'N')
                    NextAction();

                else if (c == 's' || c == 'S')
                    SkillReady();
            }
        }

        private void NextAction() // called by animation event
        {
            if (playerController.localEvent)
            {
                nextAction = true;

                animator.SetTrigger(hashNextAction);
            }
        }

        private void AttackEndEvent(int weaponIndex, bool animationEvent)
        {
            isAttacking = false;

            if (weaponIndex < weaponList.Count && weaponList[weaponIndex] != null)
            {
                weaponList[weaponIndex].EndAttack(animationEvent);

                ResetTriggersA();
            }
        }

        public void EndAllAttacks() // called by ActionStateHandler.cs
        {   
            attackIndex = 0;

            if (playerController.localEvent)
            {
                int listCount = weaponList.Count;

                for (int i = 0; i < listCount; i ++)
                {
                    AttackEndEvent(i, false);
                }
            }
        }

        private void SkillReady() // called by MeleeAttack.AttackEnd()
        {
            if (playerController.localEvent)
            {
                skillReady = true;

                if (resetDelay == 0f)
                {
                    NextAction();
                }

                else
                {
                    skillReadyCoroutine = IE_SkillReady();

                    StartCoroutine(skillReadyCoroutine);
                }
            }
        }

        private IEnumerator IE_SkillReady()
        {
            int attackState = rms.actionState;

            yield return new WaitForSeconds(resetDelay);

            if (rms.actionState == attackState)
            {
                NextAction();
            }
        }

        public void ClearTargets() // called internally and by StateHandler.cs
        {
            if (playerController.localEvent)
            {
                bool targetDisabled = currentTarget != null && !currentTarget.gameObject.activeSelf;

                if (targetsHit.Count == 0 || targetDisabled)
                    currentTarget = null;

                targetsHit.Clear();
            }
        }

        public void AddTarget(Target target, TargetCollider targetCollider) // called by PlayerController.cs
        {
            currentTarget = targetCollider;

            targetsHit.Add(target);
        }

        public void DisableAllInputs(bool value)
        {
            disableAllInputs = value;
        }

        private void PlayAudioSequence(AnimationEvent currentEvent) // called by animation event
        {
            int index = currentEvent.intParameter;

            int weaponIndex = (int) currentEvent.floatParameter;

            Weapon weapon = weaponList[weaponIndex];

            PlayTracks(weapon, index);
        }

        private void PlayTracks(Weapon weapon, int index)
        {
            Transform audioPlayer = weapon.audioPlayer.transform;

            if (index < audioPlayer.childCount)
            {
                AudioSequence audioSequence = audioPlayer.GetChild(index).GetComponent<AudioSequence>();

                AudioSource audioSource = weapon.audioPlayer.audioSource;

                if (audioSequence != null && audioSource != null)
                {
                    audioSequence.PlayTracks(audioSource, playerController, weapon.volume);
                }
            }
        }

        public void ResetChargedAttackTimer() // called by ActionStateHandler.cs
        {
            chargedAttackTimer = 0f;
        }

        // =============================================================
        //    Hitstop Methods
        // =============================================================

        public void StartHitstop(float duration)
        {
            if (duration > 0f)
            {
                resetDelay -= duration;

                if (resetDelay < 0f)
                {
                    resetDelay = 0f;
                }

                // =============================================

                CancelHitstop();

                hitstopCoroutine = Hitstop(duration);

                StartCoroutine(hitstopCoroutine);
            }
        }

        public void CancelHitstop()
        {
            if (hitstopCoroutine != null)
            {
                StopCoroutine(hitstopCoroutine);

                hitstopCoroutine = null;
            }

            rms.PauseSimulation(false);
        }

        private IEnumerator Hitstop(float duration)
        {
            rms.PauseSimulation(true);

            float originalSpeed = animator.speed;

            animator.speed = 0f;

            // =============================================

            yield return new WaitForSeconds(duration);

            animator.speed = originalSpeed;

            rms.PauseSimulation(false);
        }

        // =============================================================
        //    Get Methods - Variables
        // =============================================================

        public bool GetIsAttacking() // called by PlayerInteraction.cs
        {
            return isAttacking;
        }

        public bool GetAnimationLock() // called by PlayerController.cs
        {
            return animationLock;
        }

        public TargetCollider GetCurrentTarget() // called by PlayerController.cs
        {
            return currentTarget;
        }

        public List<Target> GetTargetsHit() // called by PlayerController.cs
        {
            return targetsHit;
        }

        // =============================================================
        //    Set Methods - Variables
        // =============================================================

        public void SetAnimationLock(bool value) // called by ActionStateHandler.cs
        {
            animationLock = value;
        }

        public void SetNextAction(bool value) // called by ActionStateHandler.cs
        {
            nextAction = value;
        }

        public void SetSkillReady(bool value) // called by ActionStateHandler.cs
        {
            skillReady = value;

            if (false && skillReadyCoroutine != null)
            {
                StopCoroutine(skillReadyCoroutine);

                skillReadyCoroutine = null;
            }
        }

        public void SetTrigger(int value) // called by PlayerController.cs
        {
            trigger = value;
        }

        public void SetResetDelay(float value) // called by ActionStateHandler.cs
        {
            resetDelay = (value >= 0f) ? value : 0.15f;
        }

        public void SetWeaponSoundOverrides(WeaponSound[] newOverrides) // called by ActionStateHandler.cs
        {
            weaponSoundOverrides = newOverrides;
        }

        // =============================================================
        //    Launch Value
        // =============================================================

        [System.Serializable]
        public struct LaunchValue
        {
            public float launchSpeed;
            public float launchHeight;

            public LaunchValue(float speed, float height)
            {
                launchSpeed = speed;
                launchHeight = height;
            }
        }
    }
}









