using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class CustomNetworkManager : MonoBehaviour
    {
        private PlayerManager playerManager;

        // =========================================================

        [Space(10)]

        public Transform platformContainer;

        public List<GameObject> platforms = new List<GameObject>();
        
        public int maxConnections;

        // =========================================================
        //    Network Manager Methods
        // =========================================================

        /*public override void Awake()
        {
            base.Awake();

            GetComponents();
        }

        private void GetComponents()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            playerManager = elements.GetComponent<PlayerManager>();
        }

        public override void OnStartClient()
        {
            RegisterPrefabs();
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            if (playerManager != null)
            {
                int playerCount = playerManager.playerCount;

                if (playerCount == 0)
                {
                    SpawnPlatforms(conn);
                }

                // =========================================================

                GameObject playerPrefab = this.playerPrefab;

                if (playerManager.playerCount > 0)
                {
                    playerPrefab = spawnPrefabs[0];
                }

                GameObject playerObject = Instantiate(playerPrefab);

                // =========================================================

                PlayerController player = playerObject.GetComponent<PlayerController>();

                // =========================================================

                player.name = $"{player.m_name} [connId={conn.connectionId}]";

                NetworkServer.AddPlayerForConnection(conn, playerObject);
            }
        }

        private void SpawnPlatforms(NetworkConnectionToClient conn)
        {
            int listCount = platforms.Count;

            for (int i = 0; i < listCount; i ++)
            {
                if (platforms[i] != null)
                {
                    GameObject platform = Instantiate(platforms[0]);

                    if (platformContainer != null)
                    {
                        platform.transform.SetParent(platformContainer);
                    }

                    NetworkServer.Spawn(platform, conn);
                }
            }
        }

        private void RegisterPrefabs()
        {
            foreach (GameObject platform in platforms)
            {
                if (platform != null)
                {   
                    NetworkClient.RegisterPrefab(platform);
                }
            }
        }

        public override void OnApplicationQuit()
        {
            if (QuitConfirmation.quitConfirmation)
            {
                base.OnApplicationQuit();
            }
        }*/

        // =========================================================
        //    Player Controller Methods
        // =========================================================

        public void SetTargetAngle(PlayerController player, float angle)
        {
            player.RpcSetTargetAngle(angle);
        }

        public void SyncedJump(PlayerController player, Vector3 position, Vector3 rotation, float[] values, bool useTrigger)
        {
            player.RpcSyncedJump(position, rotation, values, useTrigger);
        }

        public void SyncedFall(PlayerController player, Vector3 position, Vector3 rotation)
        {
            player.RpcSyncedFall(position, rotation);
        }

        public void SyncedLand(PlayerController player, Vector3 newPosition, Vector3 newRotation, float moveSpeed, float moveInput, bool isAirborne)
        {
            player.RpcSyncedLand(newPosition, newRotation, moveSpeed, moveInput, isAirborne);
        }

        public void SyncedLand(PlayerController player, Vector3 newPosition, Vector3 newRotation, float moveSpeed, float moveInput, bool isAirborne, Platform platform, int siblingIndex)
        {
            player.RpcSyncedPlatformLand(newPosition, newRotation, moveSpeed, moveInput, isAirborne, platform, siblingIndex);
        }

        public void SetPlayerTransform(PlayerController player, Vector3 newPosition, Vector3 newRotation, float targetRotation, float moveSpeed, Vector3 moveDirection, float fallSpeed, bool setLocalPosition)
        {
            player.RpcSetPlayerTransform(newPosition, newRotation, targetRotation, moveSpeed, moveDirection, fallSpeed, setLocalPosition);
        }
        
        public void SendCollisionFeedback(PlayerController player, Target target, Vector3[] values)
        {
            player.RpcSendCollisionFeedback(target, values);
        }

        public void SendNetworkTrigger(PlayerController player, int actionState, float angle)
        {
            player.RpcSendNetworkTrigger(actionState, angle);
        }

        public void SetInteractionState(PlayerController player, bool value)
        {
            player.RpcSetInteractionState(value);
        }

        public void SetInputDetected(PlayerController player, bool value)
        {
            player.RpcSetInputDetected(value);
        }

        public void SetTargetVelocity(PlayerController player, float value)
        {
            player.RpcSetTargetVelocity(value);
        }

        public void SetTargetInput(PlayerController player, float value)
        {
            player.RpcSetTargetInput(value);
        }

        // =========================================================
        //    Platform Methods
        // =========================================================

        public void SetPlatformTarget(Platform platform, int index)
        {
            platform.RpcSetPlatformTarget(index);
        }
    }
}




