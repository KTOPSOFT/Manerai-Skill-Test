using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class Footsteps : MonoBehaviour
    {
        public Transform leftFoot;
        public Transform rightFoot;

        public AudioPlayer audioPlayer;

        [Range(0f, 1.0f)]
        public float volume = 0.75f;

        private float controllerRadius;

        private static bool playOneShot = true;

        // =========================================================

        private Animator animator;

        private PlayerController playerController;

        private SceneHandler sceneHandler;

        // =========================================================

        [Space(10)]

        public WaterLayer waterLayer;

        [System.Serializable]
        public struct WaterLayer
        {
            [Range(0f, 1.0f)]
            public float waterVolume;

            [Range(0f, 1.0f)]
            public float groundVolume;
        }

        // =========================================================

        [Space(10)]

        public Transform armature;

        // =========================================================
        //    Animation Parameters
        // =========================================================

        readonly int hashStateType = Animator.StringToHash("StateType");

        // =========================================================
        //    Standard Methods
        // =========================================================

        public void Initialize(PlayerController player) // called by PlayerController.cs
        {
            animator = player.GetAnimator();

            playerController = player;

            sceneHandler = player.GetSceneHandler();
        }

        private void LeftFootstep() // called by animation event
        {
            int transitionHash = animator.GetAnimatorTransitionInfo(0).fullPathHash;

            if (transitionHash == 0)
            {
                Footstep(leftFoot);
            }
        }

        private void RightFootstep() // called by animation event
        {
            int transitionHash = animator.GetAnimatorTransitionInfo(0).fullPathHash;

            if (transitionHash == 0)
            {
                Footstep(rightFoot);
            }
        }

        private void Footstep(Transform origin)
        {
            if (audioPlayer != null && origin != null)
            {   
                float x = origin.position.x;
                float y = transform.position.y;
                float z = origin.position.z;

                Vector3 position = new Vector3(x, y, z);

                // ==================================================

                ColliderInfo colliderInfo = GetColliderInfo(position, false);

                if (colliderInfo != null)
                {
                    FootstepEvent(colliderInfo, position);
                }

                else
                {
                    controllerRadius = playerController.GetCharacterController().radius;

                    colliderInfo = GetColliderInfo(transform.position, true);

                    if (colliderInfo != null)
                    {
                        FootstepEvent(colliderInfo, position);
                    }
                }
            }
        }

        public void Land() // called by PlayerController.cs
        {
            controllerRadius = playerController.GetCharacterController().radius;

            // ==================================================

            ColliderInfo colliderInfo = GetColliderInfo(transform.position, true);

            if (colliderInfo != null)
            {
                LandEvent(colliderInfo);
            }
        }

        private void FootstepEvent(ColliderInfo colliderInfo, Vector3 position)
        {
            List<AudioListGroup> footsteps = sceneHandler.audioLists.footsteps;

            int listCount = footsteps.Count;

            // ==================================================

            float volumeScale = 1.0f;

            float volume = GetFootstepVolume() * this.volume;

            int waterListIndex = sceneHandler.audioLists.waterListIndex;

            bool inWater = GetInWater(position, 0.1f, 1.0f, waterListIndex);

            if (inWater && waterListIndex < listCount)
            {   
                AudioListGroup audioListGroup = footsteps[waterListIndex];

                audioPlayer.PlayFromAudioListGroup(audioListGroup, playerController, volume * waterLayer.waterVolume, playOneShot);

                volumeScale *= waterLayer.groundVolume;
            }

            // ==================================================

            if (colliderInfo.isTerrain)
            {   
                PlayTerrainFootstep(colliderInfo, position, volume * volumeScale);
            }

            else
            {
                int index = colliderInfo.materialTypes[0];

                if (index < listCount)
                {
                    AudioListGroup audioListGroup = footsteps[index];

                    audioPlayer.PlayFromAudioListGroup(audioListGroup, playerController, volume * volumeScale, playOneShot);
                }
            }
        }

        private void LandEvent(ColliderInfo colliderInfo)
        {
            AudioSource audioSource = audioPlayer.audioSource;

            List<AudioSequence> landingSounds = sceneHandler.audioLists.landingSounds;

            int listCount = landingSounds.Count;

            // ==================================================

            float volumeScale = 1.0f;

            int waterListIndex = sceneHandler.audioLists.waterListIndex;

            bool inWater = GetInWater(transform.position, controllerRadius / 2.0f, 1.0f, waterListIndex);

            if (inWater && waterListIndex < listCount)
            {
                AudioSequence audioSequence = landingSounds[waterListIndex];

                if (audioSequence != null)
                {
                    audioSequence.PlayTracks(audioSource, playerController, volume * waterLayer.waterVolume);

                    volumeScale *= waterLayer.groundVolume;
                }
            }

            // ==================================================

            int index = colliderInfo.materialTypes[0];

            if (colliderInfo.isTerrain)
            {
                int terrainLayer = 0;

                float[] textureValues = TerrainTexture.CheckTexture(colliderInfo, transform.position, out terrainLayer);

                index = colliderInfo.materialTypes[terrainLayer];
            }

            // ==================================================

            if (index < listCount)
            {    
                AudioSequence audioSequence = landingSounds[index];

                if (audioSequence != null)
                {
                    audioSequence.PlayTracks(audioSource, playerController, volume * volumeScale);
                }
            }
        }

        private void PlayTerrainFootstep(ColliderInfo colliderInfo, Vector3 position, float volume)
        {
            int[] materialTypes = colliderInfo.materialTypes;

            int dominantLayer = 0;

            float[] textureValues = TerrainTexture.CheckTexture(colliderInfo, position, out dominantLayer);

            int arrayLength = textureValues.Length;

            if (arrayLength > 0)
            {
                List<AudioListGroup> footsteps = sceneHandler.audioLists.footsteps;

                int listCount = footsteps.Count;

                // ==================================================

                for (int i = 0; i < arrayLength; i ++)
                {
                    int index = materialTypes[i];

                    if (index < listCount)
                    {
                        AudioListGroup audioListGroup = footsteps[index];

                        audioPlayer.PlayFromAudioListGroup(audioListGroup, playerController, volume * textureValues[i], playOneShot);
                    }
                }
            }
        }

        private ColliderInfo GetColliderInfo(Vector3 position, bool useSphereCast)
        {
            ColliderInfo colliderInfo = null;

            // ==================================================

            RaycastHit hit;

            Ray ray = new Ray(position + Vector3.up * 1.0f, Vector3.down);

            LayerMask groundLayerMask = sceneHandler.groundLayerMask;

            // ==================================================

            bool groundCheck;

            if (useSphereCast)
            {
                groundCheck = Physics.SphereCast(ray, controllerRadius, out hit, 1.2f, groundLayerMask);
            }

            else
            {
                groundCheck = Physics.Raycast(ray, out hit, 1.2f, groundLayerMask);
            }

            // ==================================================

            if (groundCheck)
            {
                colliderInfo = hit.collider.GetComponent<ColliderInfo>();
            }

            // ==================================================

            return colliderInfo;
        }

        private bool GetInWater(Vector3 position, float radius, float height, int audioListIndex)
        {
            bool inWater = false;

            if (audioListIndex >= 0)
            {
                LayerMask waterLayerMask = 1 << 4; // layer 4 = "Water"

                Vector3 startPosition = position + Vector3.up * radius;
                Vector3 endPosition = position + Vector3.up * (height - radius);

                inWater = Physics.CheckCapsule(startPosition, endPosition, radius, waterLayerMask, QueryTriggerInteraction.Ignore);
            }

            return inWater;
        }

        private float GetFootstepVolume()
        {
            float volume = 1.0f;

            if (playerController.GetIsCrouching())
                volume = 0.5f;

            else if (playerController.GetTargetInput() <= 0.5f)
                volume = 0.75f;

            return volume;
        }

        private void Reset()
        {
            waterLayer.waterVolume = 0.5f;

            waterLayer.groundVolume = 0.75f;
        }
    }
}









