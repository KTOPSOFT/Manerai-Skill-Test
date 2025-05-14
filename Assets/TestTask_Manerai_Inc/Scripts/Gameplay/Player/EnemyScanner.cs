using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(SphereCollider))]

    [RequireComponent(typeof(Rigidbody))]

    public class EnemyScanner : MonoBehaviour
    {
        private PlayerManager playerManager;

        private List<InteractionCollider> enemyColliders = new List<InteractionCollider>();

        private void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            playerManager = elements.GetComponent<PlayerManager>();

            // =============================================================

            SphereCollider collider = GetComponent<SphereCollider>();

            collider.isTrigger = true;

            collider.center = Vector3.zero;

            // =============================================================

            Rigidbody m_rigidbody = GetComponent<Rigidbody>();

            m_rigidbody.useGravity = false;

            m_rigidbody.isKinematic = true;

            // =============================================================

            gameObject.hideFlags = HideFlags.HideInHierarchy;

            enemyColliders.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            InteractionCollider enemyCollider = other.GetComponent<InteractionCollider>();

            if (enemyCollider != null)
            {
                enemyCollider.ShowHealthMeter();

                if (!enemyColliders.Contains(enemyCollider))
                {
                    enemyColliders.Add(enemyCollider);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            InteractionCollider enemyCollider = other.GetComponent<InteractionCollider>();

            if (enemyCollider != null)
            {
                Vector3 difference = other.transform.position - transform.position;

                float distance = playerManager.enemyScanRange - 1.0f;

                if (difference.sqrMagnitude >= distance * distance)
                {
                    enemyCollider.HideHealthMeter();
                }

                enemyColliders.Remove(enemyCollider);
            }
        }

        private void OnDisable()
        {
            int listCount = enemyColliders.Count;

            for (int i = 0; i < listCount; i ++)
            {
                enemyColliders[i].HideHealthMeter();
            }

            enemyColliders.Clear();
        }
    }
}




