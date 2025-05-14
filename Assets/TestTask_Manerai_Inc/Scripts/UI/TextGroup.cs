using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TMPro;

namespace YukiOno.SkillTest
{
    public class TextGroup : MonoBehaviour
    {
        private int listCount = -1;

        public List<TextMeshProUGUI> textLayers = new List<TextMeshProUGUI>();

        public void SetText(string text)
        {
            if (listCount < 0)
            {
                listCount = textLayers.Count;
            }

            // =========================================================

            for (int i = 0; i < listCount; i ++)
            {
                textLayers[i].SetText(text);
            }
        }

        private void OnValidate()
        {
            listCount = (listCount >= 0) ? textLayers.Count : -1;
        }
    }
}









