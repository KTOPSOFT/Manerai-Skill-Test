using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(CapsuleCollider))]

    [RequireComponent(typeof(Rigidbody))]

    public class CollisionTrigger : MonoBehaviour
    {
        private PlayerController playerController;

        private float boundaryTime;

        private int collisionTriggerLayer;

        [HideInInspector] public Teleporter destinationTeleporter;

        private void OnDisable()
        {
            boundaryTime = 0f;
            
            destinationTeleporter = null;
        }

        public void Initialize(PlayerController player, CharacterController controller) // called by PlayerController.cs
        {
            CapsuleCollider m_collider = GetComponent<CapsuleCollider>();

            float skinWidth = controller.skinWidth;

            m_collider.isTrigger = true;

            m_collider.center = controller.center;

            m_collider.radius = controller.radius + skinWidth + 0.01f;
            m_collider.height = controller.height + skinWidth * 2.0f + 0.01f;

            // =========================================================

            Rigidbody m_rigidbody = GetComponent<Rigidbody>();

            m_rigidbody.useGravity = false;

            m_rigidbody.isKinematic = true;

            // =========================================================

            transform.localPosition = Vector3.zero;

            transform.localRotation = Quaternion.identity;

            // =========================================================

            playerController = player;

            collisionTriggerLayer = player.GetSceneHandler().GetCollisionTriggerLayer();

            gameObject.layer = collisionTriggerLayer;

            // =========================================================

            destinationTeleporter = null;
        }

        public void SetNewArea(AreaCollider areaCollider) // called by AreaCollider.cs
        {
            Transform newArea = areaCollider.transform.parent;

            if (newArea != null && playerController != null)
            {
                PlayerManager playerManager = playerController.GetPlayerManager();

                Transform currentArea = playerManager.GetCurrentArea();

                if (newArea != currentArea)
                {
                    SceneHandler sceneHandler = playerController.GetSceneHandler();
                    
                    MessageList areaName = sceneHandler.messageLists.areaName;

                    if (areaName != null)
                    {
                        float temp = areaName.exitTime;

                        bool lowLevel = false;

                        if (lowLevel)
                        {
                            MessageList notification = playerController.GetSceneHandler().messageLists.notification;

                            if (notification != null)
                            {
                                areaName.exitTime = notification.exitTime;

                                StartCoroutine(ShowDangerMessage(notification));
                            }
                        }

                        areaName.AddMessage(newArea.gameObject.name);

                        areaName.exitTime = temp;
                    }

                    playerManager.SetCurrentArea(newArea);

                    // =========================================================

                    Area area = areaCollider.transform.parent.GetComponent<Area>();

                    if (area != null)
                    {
                        AudioFade musicPlayer = sceneHandler.musicPlayer;

                        if (musicPlayer != null)
                        {
                            AudioClip nextTrack = area.soundtrack;

                            musicPlayer.FadeClip(nextTrack);
                        }

                        // =====================================================

                        area.gameEvent.Invoke();
                    }
                }
            }
        }

        private IEnumerator ShowDangerMessage(MessageList notification)
        {
            yield return new WaitForSeconds(0.2f);

            string message = "Entered dangerous area.";

            notification.AddMessage(message);
        }

        public void SetBoundaryTime(float newTime) // called by WorldBoundary.cs
        {
            if (newTime >= boundaryTime + 3.0f)
            {
                MessageList notification = playerController.GetSceneHandler().messageLists.notification;

                if (notification != null)
                {
                    string message = "Cannot travel any further.";

                    notification.AddMessage(message);
                }

                boundaryTime = newTime;

                // =========================================================

                AudioManager audioManager = playerController.GetSceneHandler().audioManager;

                AudioSource buzzError = audioManager.systemSounds.error;

                SceneHandler.PlayAudioSource(buzzError);
            }
        }

        public PlayerController GetPlayerController()
        {
            return playerController;
        }
    }
}









