using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class InteractionTrigger : MonoBehaviour
    {
        public LayerMask layerMask;

        private SceneHandler sceneHandler;

        private PlayerInteraction playerInteraction;

        private void Awake()
        {   
            Rigidbody m_rigidbody = GetComponent<Rigidbody>();

            m_rigidbody.useGravity = false;

            m_rigidbody.isKinematic = true;
        }

        public void Initialize(PlayerController player) // called by PlayerController.cs
        {
            sceneHandler = player.GetSceneHandler();

            playerInteraction = sceneHandler.GetComponent<PlayerManager>().playerInteraction;

            // =============================================================

            SphereCollider collider = GetComponent<SphereCollider>();

            collider.enabled = false;

            // =============================================================

            gameObject.layer = sceneHandler.GetInteractionLayer();

            collider.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (sceneHandler != null)
            {
                Interactable interactable = other.GetComponent<Interactable>();

                if (interactable != null)
                {
                    int layer = interactable.GetStartingLayer();

                    if (InLayerMask(layer, layerMask))
                    {   
                        AddTarget(other.transform);
                    }
                }
            }
        }

        private void AddTarget(Transform other)
        {
            Interactable target = other.GetComponent<Interactable>();

            if (target != null)
            {
                playerInteraction.AddTarget(target);
            }
        }

        private static bool InLayerMask(int layer, LayerMask layermask)
        {
            return layermask == (layermask | (1 << layer));
        }
    }
}


