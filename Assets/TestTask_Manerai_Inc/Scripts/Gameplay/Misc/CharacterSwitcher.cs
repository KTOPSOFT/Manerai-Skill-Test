using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class CharacterSwitcher : MonoBehaviour
    {
        private SceneHandler sceneHandler;
        private InputManager inputManager;

        private PlayerManager playerManager;

        // =========================================================

        public PlayerController reserve;

        public Transform reserveLocation;

        public CanvasFadeIn switchPrompt;

        private bool m_allowSwitch;

        private static bool[] inputConditions = new bool[] {false, false};

        // =========================================================

        private void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            sceneHandler = elements.GetComponent<SceneHandler>();
            inputManager = elements.GetComponent<InputManager>();

            playerManager = elements.GetComponent<PlayerManager>();

            // =========================================================

            if (switchPrompt != null)
            {
                GameObject prompt = switchPrompt.gameObject;

                prompt.SetActive(true);

                prompt.SetActive(false);
            }
        }

        private void Update()
        {
            bool fKeyDown = Keyboard.current.fKey.wasPressedThisFrame;

            if (fKeyDown && m_allowSwitch && !inputManager.GetUsingMenu())
            {
                SwitchPlayer();
            }
        }

        private void SwitchPlayer()
        {
            Transform cameraTarget = sceneHandler.cameraTarget.transform;

            Vector3 cameraPosition = cameraTarget.position;

            PlayerController activePlayer = playerManager.GetActivePlayer();

            // =========================================================

            Vector3 position = activePlayer.transform.position;
            Vector3 rotation = activePlayer.transform.eulerAngles;
            
            bool resetToIdle = true;

            activePlayer.Teleport(reserveLocation, resetToIdle);

            reserve.SetPlayerTransform(position, rotation);

            // =========================================================

            playerManager.SetActivePlayer(reserve, inputConditions);

            reserve = playerManager.GetPreviousPlayer();

            cameraTarget.position = cameraPosition;
        }

        public void AllowSwitch(bool value)
        {
            if (switchPrompt != null)
            {
                ShowPrompt(value);
            }

            m_allowSwitch = value;
        }

        private void ShowPrompt(bool value)
        {
            if (value)
            {
                GameObject prompt = switchPrompt.gameObject;

                prompt.SetActive(false);

                prompt.SetActive(true);
            }

            else
            {
                switchPrompt.FadeIn(false);
            }
        }
    }
}




