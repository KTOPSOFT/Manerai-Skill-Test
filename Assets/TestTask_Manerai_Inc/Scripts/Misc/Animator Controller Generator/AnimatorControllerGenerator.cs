#if UNITY_EDITOR

    using UnityEngine;

using UnityEditor;
using UnityEditor.Animations;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class AnimatorControllerGenerator : MonoBehaviour
    {
        [Header("Movement States")]

        public AnimationClip m_idle;

        public AnimationClip m_walk;
        public AnimationClip m_run;
        public AnimationClip m_sprint;

        [Header("Jumping States")]

        public AnimationStateData m_jump = new AnimationStateData();
        public AnimationStateData m_fall = new AnimationStateData();

        [Header("Landing States")]

        public AnimationClip m_land;

        [Header("Melee States")]

        public List<AnimationStateData> normalAttacks = new List<AnimationStateData>();

        public AnimationStateData m_chargedAttack = new AnimationStateData();

        public List<SpecialAttack> specialAttacks = new List<SpecialAttack>();

        private DodgeAttack m_dodgeAttack = new DodgeAttack();

        // [Header("Dodge States")]

        private DodgeState m_dodge = new DodgeState();

        [Header("Animator Controller")]

        public string newFileName = "";

        // =========================================================

        private List<AnimatorStateMachine> stateMachines;

        private AnimatorState idle;
        private AnimatorState movement;

        private static AnimatorConditionMode greater = AnimatorConditionMode.Greater;
        private static AnimatorConditionMode less = AnimatorConditionMode.Less;

        private static AnimatorConditionMode equals = AnimatorConditionMode.Equals;

        // =========================================================
        //    Methods
        // =========================================================

        public void CreateAnimatorController()
        {
            if (newFileName.Length > 0)
            {
                string fileName = AssetDatabase.GenerateUniqueAssetPath("Assets/" + newFileName + ".controller");

                AnimatorController controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(fileName);

                AddParameters(controller);
                AddStateMachines(controller, out stateMachines);

                AppendMovementSM(stateMachines[1]);
                AppendLandSM(stateMachines[3]); // Append LandSM before JumpSM so that landing transition is prioritized over falling transition
                AppendJumpSM(stateMachines[2]);
                AppendMeleeSM(stateMachines[4]);
                // AppendDodgeSM(stateMachines[5]);

                AddEventLayer(controller);

                AssetDatabase.SaveAssets();

                // =========================================================

                string name = fileName.Remove(0, 7); // remove the prefix "Assets/"

                Debug.Log("<color=#80E7FF>" + name + "</color> has been saved under <color=#80E7FF>Assets</color>.");
            }

            else
            {
                Debug.Log("Could not create new animator controller as the file name was empty.");
            }
        }

        private void AddParameters(AnimatorController controller)
        {
            controller.AddParameter("StateTime", AnimatorControllerParameterType.Float);
            controller.AddParameter("InputMagnitude", AnimatorControllerParameterType.Float);
            controller.AddParameter("FallSpeed", AnimatorControllerParameterType.Float);

            controller.AddParameter("StateType", AnimatorControllerParameterType.Int);
            controller.AddParameter("ActionState", AnimatorControllerParameterType.Int);
            controller.AddParameter("NetworkState", AnimatorControllerParameterType.Int);
            controller.AddParameter("InputDirection", AnimatorControllerParameterType.Int);
            controller.AddParameter("AttackType", AnimatorControllerParameterType.Int);
            controller.AddParameter("Energy", AnimatorControllerParameterType.Int);

            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("InputDetected", AnimatorControllerParameterType.Bool);
            controller.AddParameter("AutoRotate", AnimatorControllerParameterType.Bool);
            controller.AddParameter("LastAttack", AnimatorControllerParameterType.Bool);
            // controller.AddParameter("DodgeState", AnimatorControllerParameterType.Bool);

            controller.AddParameter("MeleeAttackA", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("MeleeAttackB", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("ChargedAttack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("NextAction", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Land", AnimatorControllerParameterType.Trigger);
            // controller.AddParameter("Dodge", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Launch", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("NetworkTrigger", AnimatorControllerParameterType.Trigger);

        }

        private void AddStateMachines(AnimatorController controller, out List<AnimatorStateMachine> stateMachines)
        {
            var rootStateMachine = controller.layers[0].stateMachine;

            var movementSM = rootStateMachine.AddStateMachine("Movement SM", Vector3.right * 260.0f + Vector3.up * 70.0f);

            var jumpSM = rootStateMachine.AddStateMachine("Jump SM", Vector3.zero);
            var landSM = rootStateMachine.AddStateMachine("Land SM", Vector3.up * 70.0f);
            var meleeSM = rootStateMachine.AddStateMachine("Melee SM", Vector3.up * 140.0f);

            // var jumpSM = rootStateMachine.AddStateMachine("Jump SM", Vector3.down * 70.0f);
            // var landSM = rootStateMachine.AddStateMachine("Land SM", Vector3.zero);
            // var meleeSM = rootStateMachine.AddStateMachine("Melee SM", Vector3.up * 70.0f);
            // var dodgeSM = rootStateMachine.AddStateMachine("Dodge SM", Vector3.up * 140.0f);

            // =========================================================

            stateMachines = new List<AnimatorStateMachine>();

            stateMachines.Add(rootStateMachine);

            stateMachines.Add(movementSM);
            stateMachines.Add(jumpSM);
            stateMachines.Add(landSM);
            stateMachines.Add(meleeSM);

            // stateMachines.Add(dodgeSM);

            // =========================================================

            rootStateMachine.entryPosition = Vector3.right * 280.0f + Vector3.down * 60.0f;
            rootStateMachine.anyStatePosition = Vector3.right * 280.0f + Vector3.down * 110.0f;
            rootStateMachine.exitPosition = Vector3.right * 280.0f + Vector3.down * 160.0f;
        }

        private void AppendMovementSM(AnimatorStateMachine movementSM)
        {
            idle = movementSM.AddState("Idle", Vector3.zero);

            movement = movementSM.AddState("Movement", Vector3.up * 90.0f);

            // =========================================================

            AddMotion(idle, m_idle);

            idle.AddStateMachineBehaviour<StateHandler>();
            idle.AddStateMachineBehaviour<ActionStateHandler>();

            StateHandler stateHandlerA = (StateHandler) idle.behaviours[0];

            stateHandlerA.movementEnabled = true;
            stateHandlerA.rotationEnabled = true;

            var transitionToMovement = AddTransition(idle, movement, 0.2f);

            AddCondition(transitionToMovement, "InputDetected", true);

            // =========================================================

            AddBlendTree(movement);

            movement.AddStateMachineBehaviour<StateHandler>();
            movement.AddStateMachineBehaviour<ActionStateHandler>();

            StateHandler stateHandlerB = (StateHandler) movement.behaviours[0];

            stateHandlerB.movementEnabled = true;
            stateHandlerB.rotationEnabled = true;

            stateHandlerB.movementState = true;

            var transitionToIdle = AddTransition(movement, idle, 0.2f);

            AddCondition(transitionToIdle, "InputDetected", false);

            // =========================================================

            movementSM.entryPosition = Vector3.down * 100.0f + Vector3.right * 20.0f;
            movementSM.anyStatePosition = Vector3.down * 150.0f + Vector3.right * 20.0f;
            movementSM.exitPosition = Vector3.down * 200.0f + Vector3.right * 20.0f;

            movementSM.parentStateMachinePosition = Vector3.right * 300.0f;
        }

        private void AppendLandSM(AnimatorStateMachine landSM)
        {
            landSM.AddStateMachineBehaviour<StateHandler>();
            landSM.AddStateMachineBehaviour<ActionStateHandler>();

            StateHandler stateHandler = (StateHandler) landSM.behaviours[0];

            stateHandler.movementEnabled = true;
            stateHandler.rotationEnabled = true;

            stateHandler.landingState = true;

            // =========================================================

            AnimatorState land = landSM.AddState("Land - Idle", Vector3.zero);

            AddMotion(land, m_land);

            var anyStateTransitionA = AddAnyStateTransition(land, 0f);

            AddCondition(anyStateTransitionA, "Land");

            /*if (m_landWalking != null || m_landRunning != null)
            {
                AddCondition(anyStateTransitionA, "InputDetected", false);
            }*/

            var transitionToMovementA = AddTransition(land, movement, 0.2f);

            AddCondition(transitionToMovementA, "InputDetected", true);

            AddDefaultExitTransition(land, idle);

            // =========================================================

            /*int statePosition = 1;

            if (m_landWalking != null)
            {
                AnimatorState landWalking = landSM.AddState("Land - Walking", Vector3.up * 70.0f * statePosition);

                AddMotion(landWalking, m_landWalking);

                var anyStateTransitionB = AddAnyStateTransition(landWalking, 0f);

                AddCondition(anyStateTransitionB, "Land");
                AddCondition(anyStateTransitionB, "InputDetected", true);
                AddCondition(anyStateTransitionB, "InputMagnitude", less, 0.501f);

                var transitionToMovementB = AddTransition(landWalking, movement, 0.2f);

                AddCondition(transitionToMovementB, "InputDetected", true);
                AddCondition(transitionToMovementB, "StateTime", greater, 0.75f);

                var transitionToIdleB = AddTransition(landWalking, idle, 0.2f);

                AddCondition(transitionToIdleB, "InputDetected", false);

                statePosition ++;
            }

            // =========================================================

            if (m_landRunning != null)
            {
                AnimatorState landRunning = landSM.AddState("Land - Running", Vector3.up * 70.0f * statePosition);

                AddMotion(landRunning, m_landRunning);

                var anyStateTransitionC = AddAnyStateTransition(landRunning, 0f);

                AddCondition(anyStateTransitionC, "Land");
                AddCondition(anyStateTransitionC, "InputDetected");
                AddCondition(anyStateTransitionC, "InputMagnitude", greater, 0.501f);

                var transitionToMovementC = AddTransition(landRunning, movement, 0.2f);

                AddCondition(transitionToMovementC, "InputDetected", true);
                AddCondition(transitionToMovementC, "StateTime", greater, 0.75f);

                var transitionToIdleC = AddTransition(landRunning, idle, 0.2f);

                AddCondition(transitionToIdleC, "InputDetected", false);
            }*/

            // =========================================================

            landSM.entryPosition = Vector3.down * 100.0f + Vector3.right * 20.0f;
            landSM.anyStatePosition = Vector3.left * 280.0f;
            landSM.exitPosition = Vector3.down * 150.0f + Vector3.right * 20.0f;

            landSM.parentStateMachinePosition = Vector3.right * 300.0f;
        }

        private void AppendJumpSM(AnimatorStateMachine dodgeSM)
        {
            AnimatorState jump = dodgeSM.AddState("Jump", Vector3.zero);

            AddMotion(jump, m_jump.animationClip);

            jump.AddStateMachineBehaviour<StateHandler>();
            jump.AddStateMachineBehaviour<ActionStateHandler>();

            StateHandler stateHandlerA = (StateHandler) jump.behaviours[0];

            stateHandlerA.stateType = 3;

            stateHandlerA.rotationEnabled = true;

            // =========================================================

            float durationA = m_jump.transitionDuration;

            float offsetA = m_jump.transitionOffset;

            var anyStateTransitionA = AddAnyStateTransition(jump, durationA, offsetA);

            AddCondition(anyStateTransitionA, "IsGrounded", false);
            AddCondition(anyStateTransitionA, "FallSpeed", greater, 0f);

            // =========================================================

            AnimatorState fall = dodgeSM.AddState("Fall", Vector3.up * 70.0f);

            AddMotion(fall, m_fall.animationClip);

            fall.AddStateMachineBehaviour<StateHandler>();
            fall.AddStateMachineBehaviour<ActionStateHandler>();

            StateHandler stateHandlerB = (StateHandler) fall.behaviours[0];

            stateHandlerB.stateType = 3;

            // =========================================================

            float durationB = m_fall.transitionDuration;

            float offsetB = m_fall.transitionOffset;

            var anyStateTransitionB = AddAnyStateTransition(fall, durationB, offsetB);

            AddCondition(anyStateTransitionB, "IsGrounded", false);
            AddCondition(anyStateTransitionB, "FallSpeed", less, 0f);

            // =========================================================

            dodgeSM.entryPosition = Vector3.down * 100.0f + Vector3.right * 20.0f;
            dodgeSM.anyStatePosition = Vector3.left * 240.0f;
            dodgeSM.exitPosition = Vector3.down * 150.0f + Vector3.right * 20.0f;

            dodgeSM.parentStateMachinePosition = Vector3.right * 260.0f;
        }

        private void AppendMeleeSM(AnimatorStateMachine meleeSM)
        {
            meleeSM.AddStateMachineBehaviour<StateHandler>();

            StateHandler stateHandler = (StateHandler) meleeSM.behaviours[0];

            stateHandler.stateType = 1;

            stateHandler.rotationEnabled = true;
            stateHandler.autoRotate = true;

            // =========================================================

            int listCount = normalAttacks.Count;

            if (listCount > 10) // limit to ten attacks to prevent animator clutter
                listCount = 10;

            for (int i = 0; i < listCount; i ++)
            {
                int stateIndex = i + 1;

                AnimatorState attack = meleeSM.AddState(normalAttacks[i].animationClip.name, Vector3.up * 80.0f * i);

                AddMotion(attack, normalAttacks[i].animationClip);

                attack.AddStateMachineBehaviour<ActionStateHandler>();

                ActionStateHandler actionStateHandler = (ActionStateHandler) attack.behaviours[0];

                actionStateHandler.actionState = stateIndex;
                actionStateHandler.actionSpeed = normalAttacks[i].actionSpeed;

                actionStateHandler.lastAttack = stateIndex == listCount;

                if (stateIndex == 1)
                    actionStateHandler.turnSmoothTime = 0f;

                AddAttackTransitions(attack, normalAttacks[i], stateIndex);

                // =========================================================

                if (stateIndex == m_dodgeAttack.nextAttack)
                {
                    m_dodgeAttack.SetNextAttackState(attack);
                }
            }

            // =========================================================

            /*if (m_dodgeAttack.animationClip != null)
            {
                int actionState = listCount + 1;

                // =========================================================

                AnimatorState dodgeAttack = meleeSM.AddState(m_dodgeAttack.animationClip.name, Vector3.up * 80.0f * listCount);

                AddMotion(dodgeAttack, m_dodgeAttack.animationClip);

                dodgeAttack.AddStateMachineBehaviour<ActionStateHandler>();

                ActionStateHandler actionStateHandler = (ActionStateHandler) dodgeAttack.behaviours[0];

                actionStateHandler.actionState = actionState;
                actionStateHandler.actionSpeed = m_dodgeAttack.actionSpeed;

                AddDodgeAttackTransitions(dodgeAttack, m_dodgeAttack);

                listCount ++;

                // =========================================================

                int index = m_dodgeAttack.nextAttack;

                if (index > 0)
                {   
                    AnimatorState nextAttack = m_dodgeAttack.GetNextAttackState();

                    if (nextAttack != null)
                    {
                        AddAttackTransitions(nextAttack, normalAttacks[index - 1], actionState);
                    }
                }
            }*/

            // =========================================================

            if (m_chargedAttack.animationClip != null)
            {
                int stateIndex = listCount + 1;

                AnimatorState chargedAttack = meleeSM.AddState(m_chargedAttack.animationClip.name, Vector3.up * 80.0f * listCount);

                AddMotion(chargedAttack, m_chargedAttack.animationClip);

                chargedAttack.AddStateMachineBehaviour<ActionStateHandler>();

                ActionStateHandler actionStateHandler = (ActionStateHandler) chargedAttack.behaviours[0];

                actionStateHandler.actionState = stateIndex;
                actionStateHandler.actionSpeed = m_chargedAttack.actionSpeed;

                actionStateHandler.chargedAttack = true;

                AddChargedAttackTransitions(chargedAttack, m_chargedAttack, stateIndex);

                listCount ++;
            }

            // =========================================================

            int specialCount = specialAttacks.Count;

            if (specialCount > 10) // limit to ten attacks to prevent animator clutter
                specialCount = 10;

            for (int i = 0; i < specialCount; i ++)
            {
                int stateIndex = listCount + 1;

                AnimatorState attack = meleeSM.AddState(specialAttacks[i].animationClip.name, Vector3.up * 80.0f * listCount);

                AddMotion(attack, specialAttacks[i].animationClip);

                attack.AddStateMachineBehaviour<ActionStateHandler>();

                ActionStateHandler actionStateHandler = (ActionStateHandler) attack.behaviours[0];

                actionStateHandler.actionState = stateIndex;
                actionStateHandler.actionSpeed = specialAttacks[i].actionSpeed;

                actionStateHandler.animationLock = true;
                actionStateHandler.lastAttack = true;
                
                actionStateHandler.energyCost = specialAttacks[i].energyCost;

                AddSpecialAttackTransitions(attack, specialAttacks[i], stateIndex, i);

                listCount ++;
            }

            // =========================================================

            float offset = 0f;

            if (listCount > 0)
                offset = 40.0f * (listCount - 1);

            meleeSM.entryPosition = Vector3.down * 100.0f + Vector3.right * 20.0f;
            meleeSM.anyStatePosition = Vector3.left * 280.0f + Vector3.up * offset;
            meleeSM.exitPosition = Vector3.down * 150.0f + Vector3.right * 20.0f;

            meleeSM.parentStateMachinePosition = Vector3.right * 300.0f + Vector3.up * offset;
        }

        private void AddAttackTransitions(AnimatorState animatorState, AnimationStateData stateData, int stateIndex)
        {
            float duration = (stateIndex == 1) ? 0f : stateData.transitionDuration;

            float offset = stateData.transitionOffset;

            // =========================================================

            var networkTransition = AddAnyStateTransition(animatorState, duration, offset);
            
            networkTransition.canTransitionToSelf = true;

            AddCondition(networkTransition, "NetworkTrigger");
            AddCondition(networkTransition, "NetworkState", equals, stateIndex);

            // =========================================================

            if (stateIndex == 1)
            {
                var anyStateTransitionA = AddAnyStateTransition(animatorState, 0f, offset);

                AddCondition(anyStateTransitionA, "MeleeAttackA");
                AddCondition(anyStateTransitionA, "ActionState", equals, 0);
                AddCondition(anyStateTransitionA, "StateType", equals, 0);

                if (m_chargedAttack != null)
                {
                    int chargedAttackIndex = normalAttacks.Count + 1;

                    var anyStateTransitionB = AddAnyStateTransition(animatorState, 0f, offset);

                    AddCondition(anyStateTransitionB, "MeleeAttackA");
                    AddCondition(anyStateTransitionB, "NextAction");
                    AddCondition(anyStateTransitionB, "ActionState", equals, chargedAttackIndex);
                    AddCondition(anyStateTransitionB, "StateType", equals, 1);
                }

                var anyStateTransitionC = AddAnyStateTransition(animatorState, 0f, offset);

                AddCondition(anyStateTransitionC, "MeleeAttackA");
                AddCondition(anyStateTransitionC, "NextAction");
                AddCondition(anyStateTransitionC, "LastAttack", true);
                AddCondition(anyStateTransitionC, "StateType", equals, 1);
            }

            else
            {
                var anyStateTransition = AddAnyStateTransition(animatorState, duration, offset);

                AddCondition(anyStateTransition, "MeleeAttackA");
                AddCondition(anyStateTransition, "NextAction");
                AddCondition(anyStateTransition, "ActionState", equals, stateIndex - 1);
                AddCondition(anyStateTransition, "StateType", equals, 1);
            }

            // =========================================================

            AddDefaultExitTransition(animatorState, movement);
        }

        private void AddSpecialAttackTransitions(AnimatorState animatorState, SpecialAttack stateData, int stateIndex, int attackType)
        {   
            float duration = stateData.transitionDuration;

            float offset = stateData.transitionOffset;
            
            int energyCost = stateData.energyCost;

            // =========================================================

            var networkTransition = AddAnyStateTransition(animatorState, duration, offset);
            
            networkTransition.canTransitionToSelf = true;

            AddCondition(networkTransition, "NetworkTrigger");
            AddCondition(networkTransition, "NetworkState", equals, stateIndex);

            // =========================================================

            var anyStateTransitionA = AddAnyStateTransition(animatorState, duration, offset);

            AddCondition(anyStateTransitionA, "MeleeAttackB");
            AddCondition(anyStateTransitionA, "ActionState", equals, 0);
            AddCondition(anyStateTransitionA, "StateType", equals, 0);
            AddCondition(anyStateTransitionA, "AttackType", equals, attackType);
            AddCondition(anyStateTransitionA, "Energy", greater, energyCost - 1);

            var anyStateTransitionB = AddAnyStateTransition(animatorState, duration, offset);

            anyStateTransitionB.canTransitionToSelf = true;

            AddCondition(anyStateTransitionB, "MeleeAttackB");
            AddCondition(anyStateTransitionB, "NextAction");
            AddCondition(anyStateTransitionB, "StateType", equals, 1);
            AddCondition(anyStateTransitionB, "AttackType", equals, attackType);
            AddCondition(anyStateTransitionB, "Energy", greater, energyCost - 1);

            // =========================================================

            AddDefaultExitTransition(animatorState, movement);
        }

        private void AddDodgeAttackTransitions(AnimatorState animatorState, AnimationStateData stateData)
        {
            float duration = stateData.transitionDuration;

            float offset = stateData.transitionOffset;

            var anyStateTransition = AddAnyStateTransition(animatorState, duration, offset);

            AddCondition(anyStateTransition, "MeleeAttackA");
            AddCondition(anyStateTransition, "NextAction");
            AddCondition(anyStateTransition, "DodgeState", true);

            AddDefaultExitTransition(animatorState, movement);
        }

        private void AddChargedAttackTransitions(AnimatorState animatorState, AnimationStateData stateData, int stateIndex)
        {
            float duration = stateData.transitionDuration;

            float offset = stateData.transitionOffset;

            // =========================================================

            var networkTransition = AddAnyStateTransition(animatorState, duration, offset);
            
            networkTransition.canTransitionToSelf = true;

            AddCondition(networkTransition, "NetworkTrigger");
            AddCondition(networkTransition, "NetworkState", equals, stateIndex);

            // =========================================================

            var anyStateTransition = AddAnyStateTransition(animatorState, duration, offset);

            AddCondition(anyStateTransition, "ChargedAttack");
            AddCondition(anyStateTransition, "NextAction");
            AddCondition(anyStateTransition, "StateType", equals, 1);

            // =========================================================

            AddDefaultExitTransition(animatorState, movement);
        }

        /*private void AppendDodgeSM(AnimatorStateMachine dodgeSM)
        {
            AddDodgeState(dodgeSM, m_dodge, "Dodge", 1, 0);

            // AddDodgeState(dodgeSM, m_dodgeLeft, "Dodge Left", 2, 1);

            // AddDodgeState(dodgeSM, m_dodgeRight, "Dodge Right", 3, 3);

            // =========================================================

            dodgeSM.entryPosition = Vector3.down * 100.0f + Vector3.right * 20.0f;
            dodgeSM.anyStatePosition = Vector3.left * 280.0f;
            dodgeSM.exitPosition = Vector3.down * 150.0f + Vector3.right * 20.0f;

            dodgeSM.parentStateMachinePosition = Vector3.right * 300.0f;
        }

        /*private void AddDodgeState(AnimatorStateMachine dodgeSM, DodgeState dodgeState, string stateName, int actionState, int inputDirection)
        {
            AnimatorState dodge = dodgeSM.AddState(stateName, Vector3.up * 70.0f * (actionState - 1));

            AddMotion(dodge, dodgeState.animationClip);

            // =========================================================

            StateHandler stateHandler = dodge.AddStateMachineBehaviour<StateHandler>();

            stateHandler.rotationEnabled = actionState == 1;

            stateHandler.dodgeState = true;

            // =========================================================

            ActionStateHandler actionStateHandler = dodge.AddStateMachineBehaviour<ActionStateHandler>();

            actionStateHandler.actionState = actionState;
            actionStateHandler.actionSpeed = dodgeState.actionSpeed;

            // =========================================================

            if (inputDirection == 0)
            {
                var anyStateTransitionA = AddAnyStateTransition(dodge, 0.2f);

                AddCondition(anyStateTransitionA, "Dodge");
                AddCondition(anyStateTransitionA, "ActionState", equals, 0);
                AddCondition(anyStateTransitionA, "StateType", equals, 0);
            }

            // =========================================================

            var anyStateTransitionB = AddAnyStateTransition(dodge, 0.2f);

            AddCondition(anyStateTransitionB, "Dodge");
            AddCondition(anyStateTransitionB, "NextAction");

            if (inputDirection > 0)
                AddCondition(anyStateTransitionB, "InputDirection", equals, inputDirection);

            AddCondition(anyStateTransitionB, "StateType", equals, 1);

            // =========================================================

            AddDefaultExitTransition(dodge, movement);

            // =========================================================

            var transitionToMovement = AddTransition(dodge, movement, 0.25f);

            AddCondition(transitionToMovement, "InputDetected", true);
            AddCondition(transitionToMovement, "StateTime", greater, 0.55f);
        }*/

        private void AddMotion(AnimatorState animatorState, AnimationClip animationClip)
        {
            if (animationClip != null)
            {
                animatorState.motion = animationClip;
            }
        }

        private void AddBlendTree(AnimatorState animatorState)
        {
            BlendTree blendTree = new BlendTree();

            blendTree.name = "Movement Blend Tree";

            blendTree.blendParameter = "InputMagnitude";

            blendTree.useAutomaticThresholds = false;

            blendTree.AddChild(m_idle, 0f);
            blendTree.AddChild(m_walk, 0.5f);
            blendTree.AddChild(m_run, 1.0f);
            blendTree.AddChild(m_sprint, 1.5f);

            animatorState.motion = blendTree;

            // =========================================================

            if (AssetDatabase.GetAssetPath(animatorState) != string.Empty) // saves blend tree to asset as blend trees do not save by default
            {
                AssetDatabase.AddObjectToAsset(blendTree, AssetDatabase.GetAssetPath(animatorState));
            }
        }

        private AnimatorStateTransition AddAnyStateTransition(AnimatorState destination, float duration)
        {
            return AddAnyStateTransition(destination, duration, 0f);
        }

        private AnimatorStateTransition AddAnyStateTransition(AnimatorState destination, float duration, float offset)
        {
            var transition = stateMachines[0].AddAnyStateTransition(destination);

            transition.hasFixedDuration = true;

            transition.duration = duration;
            transition.offset = offset;

            transition.interruptionSource = TransitionInterruptionSource.Destination;

            transition.canTransitionToSelf = false;

            return transition;
        }

        private AnimatorStateTransition AddTransition(AnimatorState source, AnimatorState destination, float duration)
        {
            var transition = source.AddTransition(destination);

            transition.hasFixedDuration = true;
            transition.duration = duration;

            transition.interruptionSource = TransitionInterruptionSource.Destination;

            return transition;
        }

        private void AddDefaultExitTransition(AnimatorState source, AnimatorState destination)
        {
            bool defaultExitTime = true; // use default exit time of 0.25 seconds

            var transition = source.AddTransition(destination, defaultExitTime);

            transition.interruptionSource = TransitionInterruptionSource.Destination;
        }

        private void AddExitTransition(AnimatorState source, AnimatorState destination, float exitTime, float duration)
        {
            var transition = source.AddTransition(destination);

            transition.hasExitTime = true;
            transition.exitTime = exitTime;

            transition.hasFixedDuration = true;
            transition.duration = duration;

            transition.interruptionSource = TransitionInterruptionSource.Destination;
        }

        private void AddCondition(AnimatorStateTransition transition, string parameter)
        {            
            var mode = AnimatorConditionMode.If;

            transition.AddCondition(mode, 0f, parameter);
        }

        private void AddCondition(AnimatorStateTransition transition, string parameter, bool condition)
        {
            var mode = AnimatorConditionMode.IfNot;

            if (condition)
                mode = AnimatorConditionMode.If;

            transition.AddCondition(mode, 0f, parameter);
        }

        private void AddCondition(AnimatorStateTransition transition, string parameter, AnimatorConditionMode mode, float threshold)
        {
            transition.AddCondition(mode, threshold, parameter);
        }

        // =========================================================
        //    Action State Recorder
        // =========================================================

        public void CreateActionStateRecorder()
        {
            string fileName = AssetDatabase.GenerateUniqueAssetPath("Assets/Action State Recorder.controller");

            AnimatorController controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(fileName);

            controller.AddParameter("Record", AnimatorControllerParameterType.Trigger);

            var rootStateMachine = controller.layers[0].stateMachine;

            rootStateMachine.entryPosition = Vector3.right * 20.0f + Vector3.down * 130.0f;
            rootStateMachine.anyStatePosition = Vector3.right * 20.0f + Vector3.down * 180.0f;
            rootStateMachine.exitPosition = Vector3.right * 20.0f + Vector3.down * 230.0f;

            // =========================================================

            idle = rootStateMachine.AddState("Idle", Vector3.zero);

            AddMotion(idle, m_idle);

            idle.AddStateMachineBehaviour<ActionStateProperties>();

            // =========================================================

            var previousState = idle;

            int listCount = normalAttacks.Count;

            for (int i = 0; i < listCount; i ++)
            {
                int attackIndex = i + 1;

                string stateName = normalAttacks[i].animationClip.name;

                int[] stateInfo = new int[3];

                stateInfo[0] = attackIndex;
                stateInfo[1] = attackIndex;
                stateInfo[2] = 1;

                AddState(rootStateMachine, stateInfo, stateName, normalAttacks[i].animationClip, ref previousState);
            }

            int currentIndex = normalAttacks.Count + 1;

            // =========================================================

            AddState(rootStateMachine, m_dodgeAttack.animationClip, currentIndex, 1, ref currentIndex, ref previousState);

            AddState(rootStateMachine, m_chargedAttack.animationClip, currentIndex, 1, ref currentIndex, ref previousState);

            // =========================================================

            listCount = specialAttacks.Count;

            for (int i = 0; i < listCount; i ++)
            {
                AddState(rootStateMachine, specialAttacks[i].animationClip, currentIndex, 1, ref currentIndex, ref previousState);
            }

            // =========================================================

            // AddState(rootStateMachine, m_dodge.animationClip, 1, 0, ref currentIndex, ref previousState);

            // AddState(rootStateMachine, m_dodgeLeft.animationClip, 2, 0, ref currentIndex, ref previousState);

            // AddState(rootStateMachine, m_dodgeRight.animationClip, 3, 0, ref currentIndex, ref previousState);

            // =========================================================

            if (previousState != idle)
            {
                AddExitTransition(previousState, idle, 1.0f, 0f);
            }

            AssetDatabase.SaveAssets();

            // =========================================================

            string name = fileName.Remove(0, 7);

            Debug.Log("<color=#80E7FF>" + name + "</color> has been saved under <color=#80E7FF>Assets</color>.");
        }

        private void AddState(AnimatorStateMachine stateMachine, AnimationClip animationClip, int stateIndex, int stateType, ref int currentIndex, ref AnimatorState previousState)
        {
            if (animationClip != null)
            {
                string stateName = animationClip.name;

                int[] stateInfo = new int[3];

                stateInfo[0] = currentIndex;
                stateInfo[1] = stateIndex;
                stateInfo[2] = stateType;

                AddState(stateMachine, stateInfo, stateName, animationClip, ref previousState);

                currentIndex ++;
            }
        }

        private void AddState(AnimatorStateMachine rootStateMachine, int[] stateInfo, string stateName, AnimationClip clip, ref AnimatorState previousState)
        {
            int statePosition = stateInfo[0];
            int stateIndex = stateInfo[1];
            int stateType = stateInfo[2];

            // =========================================================

            Vector3 m_statePosition = Vector3.right * 260.0f + Vector3.up * 80.0f * (statePosition - 1);

            AnimatorState attack = rootStateMachine.AddState(stateName, m_statePosition);

            AddMotion(attack, clip);

            attack.AddStateMachineBehaviour<ActionStateProperties>();

            ActionStateProperties properties = (ActionStateProperties) attack.behaviours[0];

            properties.actionState = stateIndex;
            properties.actionType = stateType;

            if (stateIndex == 1 && stateType == 1)
            {
                var transition = AddTransition(previousState, attack, 0f);

                AddCondition(transition, "Record");
            }

            else
            {
                AddExitTransition(previousState, attack, 1.0f, 0f);
            }

            previousState = attack;
        }

        private void AddEventLayer(AnimatorController controller)
        {
            controller.AddLayer("Event Layer");

            AnimatorControllerLayer eventLayer = controller.layers[1];

            // =========================================================

            eventLayer.avatarMask = GetEventMask();

            eventLayer.defaultWeight = 1.0f;

            eventLayer.syncedLayerIndex = 0;

            // =========================================================

            Motion footstepEvents = GetFootstepEvents();

            eventLayer.SetOverrideMotion(movement, footstepEvents);

            // =========================================================

            AnimatorControllerLayer[] layers = new AnimatorControllerLayer[2];

            layers[0] = controller.layers[0];
            layers[1] = eventLayer;

            controller.layers = layers;
        }

        private static AvatarMask GetEventMask()
        {
            AvatarMask eventMask = null;

            string[] maskAssets = AssetDatabase.FindAssets("Event Mask");

            if (maskAssets.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(maskAssets[0]);

                eventMask = AssetDatabase.LoadAssetAtPath(assetPath, typeof(AvatarMask)) as AvatarMask;
            }

            return eventMask;
        }

        private static Motion GetFootstepEvents()
        {
            Motion footstepEvents = null;

            string[] motionAssets = AssetDatabase.FindAssets("Footstep Events");

            if (motionAssets.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(motionAssets[0]);

                footstepEvents = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Motion)) as Motion;
            }

            return footstepEvents;
        }

        private void Reset()
        {
            normalAttacks.Clear();

            AnimationStateData notmalAttack = new AnimationStateData();

            normalAttacks.Add(notmalAttack);

            // =========================================================

            specialAttacks.Clear();

            SpecialAttack specialAttack = new SpecialAttack();

            specialAttacks.Add(specialAttack);

            // =========================================================

            gameObject.name = "Animator Controller Generator";

            transform.position = Vector3.zero;
            transform.eulerAngles = Vector3.zero;

            transform.localScale = Vector3.one;
        }
        
        // =========================================================
        //    Classes
        // =========================================================
        
        [System.Serializable]
        public class AnimationStateData
        {
            public AnimationClip animationClip;

            public float actionSpeed;

            public float transitionDuration;
            public float transitionOffset;

            public AnimationStateData()
            {
                actionSpeed = 1.0f;

                transitionDuration = 0.2f;
            }
        }
        
        [System.Serializable]
        public class SpecialAttack : AnimationStateData
        {
            public int energyCost;
        }

        [System.Serializable]
        public class DodgeAttack : AnimationStateData
        {
            public int nextAttack;

            private AnimatorState nextAttackState;

            public DodgeAttack()
            {
                actionSpeed = 1.0f;

                transitionDuration = 0.2f;

                nextAttack = -1;
            }

            public AnimatorState GetNextAttackState()
            {
                return nextAttackState;
            }

            public void SetNextAttackState(AnimatorState state)
            {
                nextAttackState = state;
            }
        }

        [System.Serializable]
        public class DodgeState
        {
            public AnimationClip animationClip;

            public float actionSpeed;

            public DodgeState()
            {
                actionSpeed = 1.0f;
            }
        }
    }
}

#endif




