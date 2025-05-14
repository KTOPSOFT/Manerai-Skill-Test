using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class TargetCollider : MonoBehaviour
    {
        public int hitSound;

        public float damageModifier = 1.0f;

        // =========================================================a

        public Collider visualBounds;

        public Transform targetTransform;

        public Transform parentTransform;

        // =========================================================

        private Target target;

        private Collider m_collider;

        // =========================================================

        public void Initialize(Target target) // called by Target.cs
        {
            this.target = target;

            m_collider = GetComponent<Collider>();

            m_collider.isTrigger = false;

            // =========================================================

            if (visualBounds != null)
            {
                visualBounds.isTrigger = true;

                visualBounds.gameObject.layer = 0;
            }

            /// =========================================================

            if (parentTransform != null)
            {
                transform.parent = parentTransform;

                transform.localPosition = Vector3.zero;

                transform.localEulerAngles = Vector3.zero;
            }
        }

        public Target GetTarget()
        {
            return target;
        }
    }
}





