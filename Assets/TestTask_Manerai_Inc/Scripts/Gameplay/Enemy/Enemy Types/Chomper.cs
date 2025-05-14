using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using UnityEngine.InputSystem; // temp

namespace YukiOno.SkillTest
{
    public class Chomper : Enemy
    {   
        public AudioPlayer frontStepAudio;
        public AudioPlayer backStepAudio;
        public AudioPlayer attackAudio;

        [Space(10)]

        public float footstepVolume = 0.5f;
        public float gruntVolume = 0.75f;
        public float attackVolume = 0.75f;

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Update() // temp
        {
            Keyboard keyboard = Keyboard.current;

            bool tKeyDown = keyboard.tKey.wasPressedThisFrame;

            if (tKeyDown)
            {
                animator.SetTrigger("MeleeAttackA");
            }
        }

        private void PlayStep(int value) // called by animation event
        {
            bool frontFoot = (value == 1);

            if (frontFoot)
            {
                AudioList audioList = audioListGroup.audioLists[0];

                frontStepAudio.PlayFromAudioList(audioList, playerController, footstepVolume, false);
            }

            else
            {
                AudioList audioList = audioListGroup.audioLists[1];

                backStepAudio.PlayFromAudioList(audioList, playerController, footstepVolume, false);
            }
        }

        private void Grunt() // called by animation event
        {
            AudioList audioList = audioListGroup.audioLists[2];

            voiceAudioPlayer.PlayFromAudioList(audioList, playerController, gruntVolume, false);
        }

        private void AttackStart() // called by animation event
        {
            AudioList audioList = audioListGroup.audioLists[3];

            attackAudio.PlayFromAudioList(audioList, playerController, attackVolume, false);
        }
    }
}




