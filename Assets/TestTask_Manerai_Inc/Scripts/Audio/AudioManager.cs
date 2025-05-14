using UnityEngine;

using System;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class AudioManager : MonoBehaviour
    {
        public SystemSounds systemSounds = new SystemSounds();

        public InteractionSounds interactionSounds = new InteractionSounds();

        // =========================================================
        //    Sound Groups
        // =========================================================

        [Serializable]
        public class SystemSounds
        {
            public AudioSource select;
            public AudioSource confirm;
            public AudioSource cancel;

            public AudioSource error;

            public AudioSource openMenu;
        }

        [Serializable]
        public struct InteractionSounds
        {
            public AudioSource target;
            public AudioSource interact;
            public AudioSource takeItem;

            public AudioSource startDialogue;
            public AudioSource resumeDialogue;

            public AudioSource selectOption;
        }
    }
}




