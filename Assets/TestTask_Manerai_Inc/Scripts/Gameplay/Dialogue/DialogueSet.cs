using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class DialogueSet : MonoBehaviour
    {
        [System.Serializable]
        public class DialogueEvent
        {  
            public int dialogueIndex;

            public NPC speaker;
            
            public Transform cameraAngle;
            
            public bool hideDialogue;
            public bool hidePlayer;
            
            [Space(10)]
            
            public GameEvent m_event;
        }

        public List<string> dialogue = new List<string>();

        public List<string> dialogueOptions;

        public List<DialogueEvent> events;

        public GameEvent onDialogueFinish;
        public GameEvent onDialogueClose;
    }
}




