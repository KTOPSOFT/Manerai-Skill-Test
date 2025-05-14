using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class Projectile : AttackSource
    {
        [Space(10)]

        public int attackIndex;

        public float attackSpeed = 1.0f;

        public float delayTime;

        [Header("Attack Overrides")]

        [Tooltip("If true, the collision's target will not be cached for auto targeting. Applicable to additional hits and projectiles that are not part of the main attack sequence.")]
        public bool ignoreHitCache;

        private void Start() // temporary
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            PlayerController playerController = transform.parent.GetComponent<PlayerController>();

            SceneHandler sceneHandler = elements.GetComponent<SceneHandler>();

            Initialize(playerController, sceneHandler);
        }

        private void Update()
        {
            if (inAttack)
            {
                UpdateAttackSource(transform, attackSpeed);
            }
        }

        public void BeginAttack(AttackProperties attackProperties)
        {
            currentAttack = null;

            if (attackProperties != null)
            {
                BeginAttack(attackProperties, attackIndex);

                // ===========================================================

                float ignoreHitCache = this.ignoreHitCache ? 1.0f : 0f;

                ReplaceInputValue(1, 0, ignoreHitCache);
            }
        }

        public void StartProjectile(AttackProperties attackProperties)
        {
            attackSpeed = animator.speed;

            StartCoroutine(IE_StartProjectile(attackProperties));
        }

        private IEnumerator IE_StartProjectile(AttackProperties attackProperties)
        {
            yield return new WaitForSeconds(delayTime);

            BeginAttack(attackProperties);
        }

        public void SetAttackIndex(int value)
        {
            attackIndex = value;
        }

        private void ReplaceInputValue(int index, int axis, float value)
        {
            Vector3 inputValue = inputValues[index];

            float x = (axis == 0) ? value : inputValue.x;
            float y = (axis == 1) ? value : inputValue.y;
            float z = (axis == 2) ? value : inputValue.z;

            inputValue = new Vector3(x, y, z);

            inputValues[index] = inputValue;
        }
    }
}









