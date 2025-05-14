#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(Animator))]

    public class ActionStateRecorder : MonoBehaviour
    {
        public Transform weapons;

        public Transform actions;

        private Animator animator;

        private int actionState;
        private int actionType;

        private int stateIndex;
        private int attackIndex;

        private Vector3 startPosition;

        private float lastPosition;

        private List<ActionState> actionStates = new List<ActionState>();
        private List<ActionState> meleeStates = new List<ActionState>();
        private List<ActionState> jumpStates = new List<ActionState>();

        private List<Weapon> weaponList = new List<Weapon>();

        private int weaponCount;

        private void Awake()
        {
            GetTransforms();

            animator = GetComponent<Animator>();

            animator.applyRootMotion = true;

            InitializeWeapons();

            InitializeActions();
        }

        private void Reset()
        {
            gameObject.name = "Action State Recorder";

            GetTransforms();
        }

        private void InitializeWeapons()
        {
            int childCount = weapons.childCount;

            for (int i = 0; i < childCount; i ++)
            {
                Weapon weapon = weapons.GetChild(i).GetComponent<Weapon>();

                weapon.InitializeAttackPoints();

                weaponList.Add(weapon);
            }

            weaponCount = weaponList.Count;
        }

        private void InitializeActions()
        {
            GetActionStates(actions.GetChild(0), out actionStates);

            GetActionStates(actions.GetChild(1), out meleeStates);

            GetActionStates(actions.GetChild(2), out jumpStates);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;

            if (keyboard.rKey.wasPressedThisFrame)
            {
                animator.SetTrigger("Record");
            }

            RecordRootMotion();

            RecordAttackPointPositions();
        }

        private void RecordRootMotion()
        {
            if (actionState > 0)
            {
                ActionState currentState = null;

                // =========================================================

                if (actionType == 0)
                    currentState = actionStates[stateIndex];

                else if (actionType == 1)
                    currentState = meleeStates[stateIndex];

                else if (actionType == 2)
                    currentState = jumpStates[stateIndex];

                // =========================================================

                if (currentState != null)
                {
                    transform.eulerAngles = Vector3.down * currentState.moveDirection;

                    currentState.listCount ++;

                    currentState.moveSpeed.Add(transform.position.z - lastPosition);
                    currentState.distanceFromOrigin.Add(transform.position.z - startPosition.z);
                }

                lastPosition = transform.position.z;
            }
        }

        private void RecordAttackPointPositions()
        {
            if (actionState > 0)
            {
                Transform actionStateTransform = actions.GetChild(actionType).GetChild(stateIndex);

                AttackProperties attackProperties = actionStateTransform.GetComponent<AttackProperties>();

                for (int i = 0; i < weaponCount; i ++)
                {
                    Weapon currentWeapon = weaponList[i];

                    if (currentWeapon.GetIsRecording())
                    {
                        currentWeapon.RecordAttackPointPositions(transform.position, attackProperties);
                    }
                }
            }
        }

        private void GetActionStates(Transform listTransform, out List<ActionState> stateList)
        {
            stateList = new List<ActionState>();

            int childCount = listTransform.childCount;

            for (int i = 0; i < childCount; i ++)
            {
                ActionState newState = listTransform.GetChild(i).GetComponent<ActionState>();

                if (newState != null)
                    stateList.Add(newState);
            }
        }

        public void SetStartPosition() // called by ActionStateProperties.cs
        {
            startPosition = transform.position;

            lastPosition = startPosition.z;
        }

        public void SetActionState(int state, int type) // called by ActionStateProperties.cs
        {
            actionState = state;

            actionType = type;

            attackIndex = 0;

            if (actionState > 0)
            {
                ActionState currentState = null;

                stateIndex = actionState - 1;

                // =========================================================

                if (actionType == 0)
                    currentState = actionStates[stateIndex];

                else if (actionType == 1)
                    currentState = meleeStates[stateIndex];

                else if (actionType == 2)
                    currentState = jumpStates[stateIndex];

                // =========================================================

                if (currentState != null)
                {
                    currentState.listCount = 0;

                    currentState.moveSpeed.Clear();
                    currentState.distanceFromOrigin.Clear();

                    currentState.timeInterval = Time.fixedDeltaTime;

                    transform.eulerAngles = Vector3.down * currentState.moveDirection;
                }
            }
        }

        private void AttackStart(AnimationEvent currentEvent) // called by animation events
        {
            int weaponIndex = (int) currentEvent.floatParameter;

            Weapon weapon = weaponList[weaponIndex];

            // =========================================================

            Transform actionStateTransform = actions.GetChild(actionType).GetChild(stateIndex);

            AttackProperties attackProperties = actionStateTransform.GetComponent<AttackProperties>();

            attackProperties.timeInterval = Time.fixedDeltaTime;

            // =========================================================

            int listCount = attackProperties.attacks.Count;

            for (int i = listCount; i < attackIndex + 1; i ++)
            {
                attackProperties.attacks.Add(new Attack());
            }

            Attack currentAttack = attackProperties.attacks[attackIndex];

            currentAttack.attackPointPositions.Clear();

            attackIndex ++;

            // =========================================================

            weapon.SetIsRecording(true, currentAttack);

            weapon.RecordAttackPointPositions(transform.position, attackProperties);
        }

        private void AttackEnd(AnimationEvent currentEvent) // called by animation events
        {
            int weaponIndex = (int) currentEvent.floatParameter;

            Weapon weapon = weaponList[weaponIndex];

            weapon.SetIsRecording(false, null);
        }

        private void NextAction() // called by animation events
        {
            // do nothing
        }

        private void ActionEvent() // called by animation events
        {
            // do nothing
        }

        private void GetTransforms()
        {
            int childCount = transform.childCount;

            for (int i = 0; i < childCount; i ++)
            {
                Transform currentChild = transform.GetChild(i);

                string name = currentChild.gameObject.name;

                if (name.Contains("Weapons"))
                {
                    weapons = currentChild;
                }

                else if (name.Contains("Actions"))
                {
                    actions = currentChild;
                }
            }
        }
    }
}

#endif







