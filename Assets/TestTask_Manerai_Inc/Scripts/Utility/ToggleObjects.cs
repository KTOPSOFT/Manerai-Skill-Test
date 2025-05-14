using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class ToggleObjects : MonoBehaviour
    {
        public List<GameObject> objectsToToggle = new List<GameObject>();

        private void Update()
        {
            if (Keyboard.current.digit0Key.wasPressedThisFrame)
            {       
                ToggleObjectList();
            }
        }

        public void ToggleObjectList()
        {
            int listCount = objectsToToggle.Count;

            if (listCount > 0 && objectsToToggle[0] != null)
            {
                bool value = !objectsToToggle[0].activeSelf;

                for (int i = 0; i < listCount; i ++)
                {   
                    GameObject currentObject = objectsToToggle[i];

                    if (currentObject != null)
                    {
                        currentObject.SetActive(value);
                    }
                }
            }
        }
    }
}




