using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class ButtonPrompts : MonoBehaviour
    {
        public List<GameObject> controlTypes = new List<GameObject>();

        private GameObject currentObject;

        private InputManager inputManager;

        private void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            inputManager = elements.GetComponent<InputManager>();

            InitializePrompts();
        }

        private void OnEnable()
        {
            if (inputManager != null)
            {
                List<ButtonPrompts> buttonPrompts = inputManager.buttonPrompts;

                if (!buttonPrompts.Contains(this))
                {
                    buttonPrompts.Add(this);
                }

                // =============================================================

                int controlType = inputManager.GetControlType();

                SwitchType(controlType);
            }
        }

        private void OnDisable()
        {
            if (inputManager != null)
            {
                List<ButtonPrompts> buttonPrompts = inputManager.buttonPrompts;

                buttonPrompts.Remove(this);
            }
        }

        public void SwitchType(int index) // called by InputManager.cs
        {
            if (index < controlTypes.Count)
            {
                if (currentObject != null)
                {
                    currentObject.SetActive(false);
                }

                currentObject = controlTypes[index];

                currentObject.SetActive(true);
            }
        }

        private void InitializePrompts()
        {
            int listCount = controlTypes.Count;

            for (int i = 0; i < listCount; i ++)
            {
                controlTypes[i].SetActive(false);
            }
        }
    }
}




