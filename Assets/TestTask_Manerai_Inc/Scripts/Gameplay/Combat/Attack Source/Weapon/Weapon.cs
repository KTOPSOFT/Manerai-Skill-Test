using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class Weapon : AttackSource
    {   
        private List<Transform> trailEffects = new List<Transform>();

        private bool isRecording;

        // =========================================================
        //    Animation Parameters
        // =========================================================

        readonly int hashStateType = Animator.StringToHash("StateType");

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Update()
        {
            if (inAttack)
            {
                UpdateAttackSource(playerController.transform, animator.speed);
            }
        }

        public void BeginAttack(int actionState, int attackIndex) // called by MeleeAttack.cs
        {
            currentAttack = null;
            
            if (playerController.GetSimulationState())
            {
                int stateType = playerController.GetAnimator().GetInteger(hashStateType);

                Transform actionList = playerController.GetRootMotionSimulator().actionList;

                Transform stateTypeTransform = actionList.GetChild(stateType);

                int index = actionState - 1;

                if (index < stateTypeTransform.childCount)
                {   
                    Transform actionStateTransform = stateTypeTransform.GetChild(index);

                    AttackProperties attackProperties = actionStateTransform.GetComponent<AttackProperties>();

                    BeginAttack(attackProperties, attackIndex);
                }
            }
        }
        
        // =========================================================
        //    Editor Methods
        // =========================================================

        #if UNITY_EDITOR

            public bool GetIsRecording() // called by ActionStateRecorder.cs
        {
            return isRecording;
        }

        public void SetIsRecording(bool value, Attack attack) // called by ActionStateRecorder.cs
        {
            isRecording = value;

            currentAttack = attack;
        }

        public void RecordAttackPointPositions(Vector3 playerPosition, AttackProperties attackProperties) // called by ActionStateRecorder.cs
        {
            for (int i = 0; i < pointCount; i ++)
            {
                AttackPoint attackPoint = validAttackPoints[i];

                Vector3 difference = attackPoint.transform.position - playerPosition;

                currentAttack.attackPointPositions.Add(difference);
            }
        }

        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i < pointCount; ++i)
            {
                AttackPoint attackPoint = validAttackPoints[i];

                Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.4f);

                // =========================================================

                float radius = attackPoint.radius * currentAttack.radiusMultiplier;

                if (radius < 0f)
                    radius = attackPoint.radius;

                // =========================================================

                Gizmos.DrawSphere(attackPoint.transform.position, radius);

                if (attackPoint.previousPositions.Count > 1)
                {
                    UnityEditor.Handles.DrawAAPolyLine(10, attackPoint.previousPositions.ToArray());
                }
            }
        }

        #endif
    }
}









