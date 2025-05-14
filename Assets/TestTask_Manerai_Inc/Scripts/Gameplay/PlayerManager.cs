using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(SceneHandler))]

    public class PlayerManager : MonoBehaviour
    {
        public PlayerController startingPlayer;

        public Transform playerContainer;

        public PlayerInteraction playerInteraction;

        private SceneHandler sceneHandler;

        private static bool[] defaultConditions = new bool[] {true, true};

        private bool allowSwitch;

        // =============================================================

        [Range(1.0f, 50.0f)]

        [Tooltip("The maximum distance in which enemy health meters are visible.")]

        public float enemyScanRange = 20.0f;

        // =============================================================

        private Transform enemyScanner;

        private SphereCollider scannerCollider;

        // =============================================================

        private List<PlayerController> players = new List<PlayerController>();

        private PlayerController activePlayer;
        private PlayerController previousPlayer;

        private Transform currentArea;

        public int playerCount { get { return players.Count; } }

        // =============================================================

        private void Awake()
        {
            sceneHandler = GetComponent<SceneHandler>();

            CheckStartingPlayer();

            CreateEnemyScanner();
        }

        public void SetActivePlayer(PlayerController player)
        {
            SetActivePlayer(player, defaultConditions);
        }

        public void SetActivePlayer(PlayerController player, bool[] conditions)
        {
            if (player != null)
            {
                HideText();

                if (activePlayer != null)
                {
                    previousPlayer = activePlayer;

                    DeactivatePlayer(activePlayer);
                }

                playerInteraction.SetActivePlayer(player.transform); // must be called before activating interaction triggers

                // =============================================================

                activePlayer = player;

                EnablePlayerObjects(player, true);

                player.SetActivePlayer(true);

                player.ToggleRigidbody(false);

                player.Land(true);

                // =============================================================

                bool resetCameraRotation = conditions[0];

                if (resetCameraRotation)
                {
                    var tpc = sceneHandler.GetThirdPersonCamera();

                    float xRotation = player.transform.eulerAngles.y;

                    tpc.SetCameraRotation(xRotation, 0f);
                }

                // =============================================================

                bool resetCameraPosition = conditions[1];

                CameraTarget cameraTarget = sceneHandler.cameraTarget;

                cameraTarget.SetPlayer(player, resetCameraPosition);

                // =============================================================

                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(TransferEnemyScanner(player.transform));
                }
            }
        }

        public void AddPlayer(PlayerController player)
        {
            Vector3 spawnPosition = player.transform.position;
            Vector3 spawnRotation = player.transform.eulerAngles;

            player.SetSpawnPoint(spawnPosition, spawnRotation);

            // =============================================================

            if (playerContainer != null)
            {
                player.SetDefaultParent(playerContainer);

                player.RevertPlatform();
            }

            // =============================================================

            if (!players.Contains(player))
            {
                if (playerCount == 0)
                {
                    ThirdPersonCamera tpc = sceneHandler.thirdPersonCamera;

                    if (tpc != null)
                    {   
                        tpc.HideCursor(true);
                    }
                }

                // =============================================================

                player.SetPlayerIndex(playerCount);

                players.Add(player);

                // =============================================================

                DeactivatePlayer(player);

                bool validSwitch = player.soloInstance || player.isLocalPlayer;

                if ((playerCount == 1 || allowSwitch) && validSwitch)
                {
                    SetActivePlayer(player);
                }
            }
        }

        public void RemovePlayer(PlayerController player)
        {
            if (players.Contains(player))
            {
                bool activePlayer = player.GetActivePlayer();

                // =============================================================

                players.Remove(player);

                DeactivatePlayer(player);

                // =============================================================

                if (players.Count == 0)
                {
                    ThirdPersonCamera tpc = sceneHandler.thirdPersonCamera;

                    if (tpc != null)
                    {   
                        tpc.HideCursor(false);
                    }
                }

                // =============================================================

                int playerCount = players.Count;

                if (activePlayer && playerCount >= 1)
                {
                    SetActivePlayer(players[0]);
                }

                // =============================================================

                if (playerCount == 0)
                {
                    playerInteraction.ClearLists();
                }
            }
        }

        private void DeactivatePlayer(PlayerController player)
        {
            EnablePlayerObjects(player, false);

            ResetCameraValues();

            // =============================================================

            player.ResetInput();

            player.ClearEventColliders();

            player.SetActivePlayer(false);
        }

        private void EnablePlayerObjects(PlayerController player, bool value)
        {
            GameObject targetColliders = player.GetTarget().targetColliders.gameObject;

            // =============================================================

            SetActive(targetColliders, value);

            SetActive(player.collisionTrigger, value);

            SetActive(player.interactionTriggers, value);

            // =============================================================

            player.playerHUD.alpha = value ? 1.0f : 0f;
        }

        private void ResetCameraValues()
        {
            var thirdPersonCamera = sceneHandler.thirdPersonCamera;

            thirdPersonCamera.UseFixedUpdate(false);

            // =============================================================

            var cameraTarget = sceneHandler.cameraTarget;

            cameraTarget.UseFixedUpdate(false);
            cameraTarget.UseFreeTransform(false);
        }

        private void HideText()
        {
            HelpText helpText = sceneHandler.helpText;

            if (helpText != null)
            {
                helpText.HideText();
            }
        }

        private void CheckStartingPlayer()
        {
            allowSwitch = false;

            if (startingPlayer != null)
            {
                StartCoroutine(SetStartingPlayer());
            }

            else
            {
                allowSwitch = true;
            }
        }

        private IEnumerator SetStartingPlayer()
        {
            yield return null;

            SetActivePlayer(startingPlayer);

            allowSwitch = true;
        }

        public void SetGamePaused(bool value) // called by PauseMenu.cs
        {
            int listCount = players.Count;

            for (int i = 0; i < listCount; i ++)
            {
                players[i].SetGamePaused(value);
            }
        }

        public void ShowActivePlayer(bool value)
        {
            GameObject playerMesh = activePlayer.transform.GetChild(0).gameObject;

            if (playerMesh != null)
            {
                playerMesh.SetActive(value);
            }
        }

        public List<PlayerController> GetPlayers()
        {
            return players;
        }

        public PlayerController GetActivePlayer()
        {
            return activePlayer;
        }

        public PlayerController GetPreviousPlayer()
        {
            return previousPlayer;
        }

        public Transform GetCurrentArea() // called by CollisionTrigger.cs
        {
            return currentArea;
        }

        public void SetCurrentArea(Transform target) // called by CollisionTrigger.cs
        {
            currentArea = target;
        }

        private void CreateEnemyScanner()
        {
            GameObject newObject = new GameObject("Enemy Scanner");

            newObject.layer = sceneHandler.GetInteractionLayer();

            newObject.AddComponent<EnemyScanner>();

            enemyScanner = newObject.transform;

            // =============================================================

            scannerCollider = enemyScanner.GetComponent<SphereCollider>();

            SetScanRange();
        }

        private IEnumerator TransferEnemyScanner(Transform newParent)
        {   
            yield return null;;

            if (enemyScanner == null) // create new scanner if player object was destroyed
            {
                CreateEnemyScanner();
            }

            enemyScanner.SetParent(newParent);

            enemyScanner.localPosition = Vector3.zero;
            enemyScanner.localRotation = Quaternion.identity;
        }

        private void SetScanRange()
        {
            if (scannerCollider != null)
            {
                float scanRange = enemyScanRange;

                if (scanRange < 1.0f)
                {
                    scanRange = 1.0f;
                }

                scannerCollider.radius = scanRange;
            }
        }

        private void OnValidate()
        {
            SetScanRange();
        }

        // =============================================================
        //    Static Methods
        // =============================================================

        private static void SetActive(GameObject targetObject, bool value)
        {
            if (targetObject != null)
            {
                targetObject.SetActive(value);
            }
        }

        private static void SetActive(MonoBehaviour target, bool value)
        {
            if (target != null)
            {
                GameObject targetObject = target.gameObject;

                if (targetObject != null)
                {
                    targetObject.SetActive(value);
                }
            }
        }
    }
}




