using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class QuitConfirmation : MonoBehaviour
    {
        public static bool quitConfirmation = false;

        public static GameObject confirmationWindow;

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            #if UNITY_EDITOR
            {
                quitConfirmation = true;
            }
            #endif

                // ====================

                Application.wantsToQuit += WantsToQuit;
        }

        static bool WantsToQuit()
        {
            if (quitConfirmation) 
            {
                return true;
            }

            return false;
        }
    }
}




