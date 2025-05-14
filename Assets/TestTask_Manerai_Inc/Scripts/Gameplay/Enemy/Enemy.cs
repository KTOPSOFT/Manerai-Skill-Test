using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class Enemy : MonoBehaviour
    {
        public CapsuleCollider targetCollider;

        protected Animator animator;

        protected PlayerController playerController;

        protected AudioListGroup audioListGroup;

        protected AudioPlayer voiceAudioPlayer;

        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();

            playerController = GetComponent<PlayerController>();

            voiceAudioPlayer = playerController.voiceAudioPlayer;

            RepositionTargetCollider();
        }

        public virtual void Initialize(AudioListGroup audioListGroup, Transform spawnPoint) // called by EnemySpawner.cs
        {
            this.audioListGroup = audioListGroup;

            playerController.SetSpawnPoint(spawnPoint.position, spawnPoint.eulerAngles);
        }

        public virtual void ResetValues()
        {
            if (playerController != null)
            {
                playerController.ResetValues();
            }
            
            else
            {
                Attributes attributes = GetComponent<Attributes>();
                
                if (attributes != null)
                {
                    attributes.ResetHealth(true);
                }
            }
        }

        protected void RepositionTargetCollider()
        {
            if (targetCollider != null)
            {
                // slightly shift collider down so that players can hit airborne enemies more easily

                targetCollider.center = Vector3.up * (targetCollider.center.y - 0.2f);

                targetCollider.height = targetCollider.height + 0.4f;
            }
        }
    }
}




