using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class CycleThroughObjects : MonoBehaviour
    {
        private List<GameObject> objects = new List<GameObject>();

        private int index;

        private int objectCount;

        public bool resetOnEnable;

        private void Awake()
        {
            objectCount = transform.childCount;

            for (int i = 0; i < objectCount; i ++)
            {
                GameObject newObject = transform.GetChild(i).gameObject;

                objects.Add(newObject);
            }

            // =========================================================

            if (objects.Count > 0)
            {
                objects[0].SetActive(true);
            }
        }

        private void OnEnable()
        {
            if (resetOnEnable)
            {
                ToggleObjects(false);

                if (objects.Count > 0)
                {
                    objects[0].SetActive(true);
                }
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard.qKey.wasPressedThisFrame)
            {
                PreviousObject();
            }

            else if (keyboard.eKey.wasPressedThisFrame)
            {
                NextObject();
            }
        }

        private void ToggleObjects(bool value)
        {
            int listCount = objects.Count;

            for (int i = 0; i < listCount; i ++)
            {
                objects[i].SetActive(value);
            }
        }

        private void PreviousObject()
        {
            index --;

            if (index < 0)
                index = objects.Count - 1;

            ToggleObjects(false);

            objects[index].SetActive(true);
        }

        private void NextObject()
        {
            index ++;

            if (index >= objects.Count)
                index = 0;

            ToggleObjects(false);

            objects[index].SetActive(true);
        }
    }
}




