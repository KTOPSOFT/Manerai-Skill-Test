#if UNITY_EDITOR

using UnityEngine;

using UnityEditor;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class ActionStateProperties : StateMachineBehaviour
    {
        public int actionState = 0;

        public int actionType = 0;

        private ActionStateRecorder recorder;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (recorder == null)
            {
                recorder = animator.GetComponent<ActionStateRecorder>();
            }

            // =====================================

            if (actionState >= 0)
            {
                if (actionState > 0)
                    Debug.Break();

                // =====================================

                recorder.SetStartPosition();

                recorder.SetActionState(actionState, actionType);
            }
        }
    }
}

#endif





















