using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [System.Serializable]
    public struct WeaponSound
    {
        public int index;

        [Tooltip("Use audio sequence over audio clip. Set the audio sequence as a child of the weapon's audio player using the sibling index specified above.")]
        public bool useAudioSequence;
    }
}




