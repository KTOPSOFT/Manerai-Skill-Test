using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class Item : Interactable
    {
        [Header("Item Obtain Message")]

        public string message = "Obtained *.";

        public bool lowercaseName = true;

        protected override void Awake()
        {
            interactionType = 1;
            
            interactionRange = 2.0f;

            viewDependent = true;

            base.Awake();
        }

        protected override void OnValidate()
        {
            interactionType = 1;
            
            viewDependent = true;

            base.OnValidate();
        }

        public override void StartInteraction()
        {
            base.StartInteraction();

            StartCoroutine(TakeItem());
        }

        private IEnumerator TakeItem()
        {
            SceneHandler sceneHandler = interaction.GetSceneHandler();

            MessageList messageList = sceneHandler.messageLists.messageList;

            if (messageList != null)
            {
                string message = GetMessage();

                messageList.AddMessage(message);
            }
            
            interaction.ConfirmAction();

            // =========================================================

            AudioSource takeItem = sceneHandler.audioManager.interactionSounds.takeItem;

            SceneHandler.PlayAudioSource(takeItem);

            // =========================================================

            yield return null;

            gameObject.SetActive(false);
        }

        private string GetMessage()
        {   
            string name = m_name;

            if (lowercaseName)
                name = m_name.ToLower();

            string message = this.message.Replace("*", name);

            // message = message.Replace("{", "<color=#" + sceneHandler.GetHexValue(0) + ">");
            // message = message.Replace("}", "</color>");

            return message;
        }
    }
}









