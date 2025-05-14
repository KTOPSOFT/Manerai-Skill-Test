using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class HelpText : MonoBehaviour
    {
        public List<CanvasGroup> canvasGroups = new List<CanvasGroup>();

        private GameObject currentText;

        private void Awake()
        {
            int childCount = transform.childCount;

            for (int i = 0; i < childCount; i ++)
            {
                GameObject textObject = transform.GetChild(i).gameObject;

                textObject.SetActive(false);
            }
        }
        
        public void ShowText(GameObject textObject)
        {
            if (textObject != null && textObject != gameObject)
            {
                ShowCanvasGroups(false);

                if (currentText != null)
                {
                    currentText.SetActive(false);
                }

                currentText = textObject;

                currentText.SetActive(true);
            }
        }

        public void HideText()
        {
            if (currentText != null)
            {
                currentText.SetActive(false);

                currentText = null;
            }

            // =========================================================

            ShowCanvasGroups(true);
        }

        private void ShowCanvasGroups(bool value)
        {
            float alpha = value ? 1.0f : 0f;

            // =========================================================

            int listCount = canvasGroups.Count;

            for (int i = 0; i < listCount; i ++)
            {
                canvasGroups[i].alpha = alpha;
            }
        }
    }
}




