using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class Teleporter : MonoBehaviour
    {
        public Transform destination;

        [Tooltip("If teleporting inside another teleporter, ignore the destination teleporter's OnTriggerEnter() to prevent teleport loops.")]
        public Teleporter destinationTeleporter;

        private void OnTriggerEnter(Collider other)
        {
            CollisionTrigger playerTrigger = other.GetComponent<CollisionTrigger>();

            if (playerTrigger != null)
            {
                PlayerController player = playerTrigger.GetPlayerController();

                bool useTeleporter = playerTrigger.destinationTeleporter != this;

                if (useTeleporter && player != null && destination != null)
                {
                    if (destinationTeleporter != null)
                    {
                        playerTrigger.destinationTeleporter = destinationTeleporter;
                    }

                    bool resetToIdle = false;

                    player.Teleport(destination, resetToIdle);

                    // =========================================================

                    var thirdPersonCamera = player.GetSceneHandler().thirdPersonCamera;

                    float xRotation = destination.eulerAngles.y;

                    thirdPersonCamera.SetCameraRotation(xRotation, 0f);
                }

                else
                {
                    playerTrigger.destinationTeleporter = null;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            CollisionTrigger playerTrigger = other.GetComponent<CollisionTrigger>();

            if (playerTrigger != null)
            {
                if (playerTrigger.destinationTeleporter == this)
                {
                    playerTrigger.destinationTeleporter = null;
                }
            }
        }

        private void Reset()
        {
            gameObject.layer = LayerMask.NameToLayer("Collision Trigger");
        }
    }
}




