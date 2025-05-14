using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class MessageList : MonoBehaviour
    {
        public int displayCount = 5;

        // ======================================================

        [Space(10)]

        public float messageDuration = 3.0f;

        public Vector3 messageOffset = Vector3.down * 45.0f;

        // ======================================================

        [Header("Entry Animation")]

        public float entryTime = 0.33f;

        public Vector3 entryPosition = Vector3.left * 50.0f;

        // ======================================================

        [Header("Exit Animation")]

        public float exitTime = 0.33f;

        public Vector3 exitPosition;

        // ======================================================

        private List<Message> messages = new List<Message>();

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {
            messages.Clear();
        }

        public void AddMessage(string text)
        {
            Transform messageTransform = ObjectPoolManager.GetFromPool(transform, Vector3.zero);

            if (messageTransform != null)
            {
                Message message = messageTransform.GetComponent<Message>();

                message.gameObject.SetActive(true);

                message.SetMessageList(this);

                // ======================================================

                SetValues(message);

                message.SetText(text);

                message.StartTimer();

                messages.Add(message);

                // ======================================================

                int listCount = messages.Count;

                if (listCount > 1)
                {
                    for (int i = 0; i < listCount - 1; i ++)
                    {                
                        float xPosition = (listCount * messageOffset.x) - ((i + 1) * messageOffset.x);
                        float yPosition = (listCount * messageOffset.y) - ((i + 1) * messageOffset.y);
                        float zPosition = (listCount * messageOffset.z) - ((i + 1) * messageOffset.z);

                        Vector3 newPosition = new Vector3(xPosition, yPosition, zPosition);

                        messages[i].TranslateTowards(newPosition, 0.15f);
                    }
                }

                // ======================================================

                if (listCount > displayCount)
                {
                    messages[0].Exit();

                    messages.RemoveAt(0);
                }
            }
        }

        public void RemoveFromList(Message message) // called by Message.cs
        {
            messages.Remove(message);
        }

        private void SetValues(Message message)
        {
            float[] values = new float[3];

            values[0] = messageDuration;
            values[1] = entryTime;
            values[2] = exitTime;

            message.SetValues(values, entryPosition, exitPosition);
        }
    }
}









