using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class ResetPositions : MonoBehaviour
    {
        public Transform playerSpawn;

        public EnemySpawner enemySpawner;

        [Space(10)]

        public List<Transform> resetChildObjects = new List<Transform>();

        // =============================================================

        private SceneHandler sceneHandler;
        private InputManager inputManager;

        private PlayerManager playerManager;

        private GameEvent screenFade = new GameEvent();

        // =============================================================

        private void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            sceneHandler = elements.GetComponent<SceneHandler>();
            inputManager = elements.GetComponent<InputManager>();

            playerManager = elements.GetComponent<PlayerManager>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard.digit9Key.wasPressedThisFrame && !inputManager.GetUsingMenu())
            {
                ScreenFadeEvent();
            }
        }

        public void ScreenFadeEvent()
        {
            PlayerController activePlayer = playerManager.GetActivePlayer();

            if (!activePlayer.GetLockMovement())
            {
                screenFade.RemoveAllListeners();

                screenFade.AddListener(delegate{ResetPlayers(activePlayer);});

                // ==========================================================

                screenFade.AddListener(delegate{ResetObjects();});

                if (enemySpawner != null)
                {
                    screenFade.AddListener(delegate{enemySpawner.ResetValues();});
                }

                // ==========================================================

                sceneHandler.screenFade.FadeTo(screenFade);
            }
        }

        private void Reset()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            gameObject.name = "Reset Positions";
        }

        private void ResetObjects()
        {
            int listCount = resetChildObjects.Count;

            for (int i = 0; i < listCount; i ++)
            {
                Transform current = resetChildObjects[i];

                int childCount = current.childCount;

                for (int j = 0; j < childCount; j ++)
                {
                    current.GetChild(j).gameObject.SetActive(false);

                    current.GetChild(j).gameObject.SetActive(true);
                }
            }
        }

        private void ResetPlayers(PlayerController activePlayer)
        {
            bool hideMeter = true;

            List<PlayerController> players = playerManager.GetPlayers();

            int playerCount = playerManager.playerCount;

            for (int i = 0; i < playerCount; i ++)
            {
                Attributes attributes = players[i].GetAttributes();

                attributes.ResetHealth(hideMeter);
                
                attributes.ResetEnergy();
            }

            // ==========================================================

            var tpc = sceneHandler.thirdPersonCamera;

            if (playerSpawn != null)
            {
                float xRotation = playerSpawn.eulerAngles.y;

                bool resetToIdle = true;

                activePlayer.Teleport(playerSpawn, resetToIdle);

                tpc.SetCameraRotation(xRotation, 0f);
            }

            else
            {
                float xRotation = activePlayer.GetSpawnRotation().y;

                activePlayer.ResetValues();

                tpc.SetCameraRotation(xRotation, 0f);
            }
        }
    }
}




