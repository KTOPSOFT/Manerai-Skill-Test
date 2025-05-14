using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class Platform : MonoBehaviour
    {
        private SceneHandler sceneHandler;

        public float moveSpeed = 1.0f;
        public float waitTime = -1;

        public List<Vector3> offsets = new List<Vector3>();

        private int listIndex;

        private float waitTimer;

        private bool moving;

        // =========================================================

        private float elapsedTime; // [SyncVar]
        private float travelDistance; // [SyncVar]

        private Vector3 pointA; // [SyncVar]
        private Vector3 pointB; // [SyncVar]

        private Vector3 startPosition; // [SyncVar]
        private Vector3 moveDirection; // [SyncVar]

        private bool targetReached; // [SyncVar]

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            SceneHandler sceneHandler = elements.GetComponent<SceneHandler>();

            // =========================================================

            networkManager = sceneHandler.networkManager;

            soloInstance = (networkManager != null) ? (networkManager.maxConnections == 1) : true;

            // =========================================================

            offsets.Insert(0, Vector3.zero);

            moving = offsets.Count > 1 && waitTime >= 0f;

            // =========================================================

            startPosition = transform.position;

            pointA = startPosition;
            pointB = startPosition;

            // =========================================================

            targetReached = true;
        }

        private void FixedUpdate()
        {
            if (moving)
            {
                if (targetReached)
                { 
                    UpdateWaitTimer();
                }

                else
                {
                    MovePlatform();
                }
            }
        }

        private void UpdateWaitTimer()
        {
            if (soloInstance || isOwned)
            {
                moveDirection = Vector3.zero;

                if (waitTime >= 0f)
                {
                    waitTimer += Time.fixedDeltaTime;

                    if (waitTimer >= waitTime)
                    {
                        NextTarget();
                    }
                }

                else
                {
                    moving = false;
                }
            }

            else
            {
                moving = false;
            }
        }

        private void MovePlatform()
        {
            elapsedTime += Time.fixedDeltaTime;

            float distance = elapsedTime * moveSpeed;

            float ratio = distance / travelDistance;

            if (ratio > 1.0f)
            {
                ratio = 1.0f;

                targetReached = true;
            }
            
            // float progress = AnimationCurves.Sinusoidal.InOut(ratio);

            Vector3 currentPosition = Vector3.Lerp(pointA, pointB, ratio);

            transform.position = currentPosition;
        }

        private void NextTarget()
        {
            listIndex ++;

            if (listIndex == offsets.Count)
                listIndex = 0;

            SetPlatformTarget(listIndex);

            // =========================================================

            if (isOwned)
                CmdSetPlatformTarget(listIndex);
        }

        private void SetPlatformTarget(int index)
        {
            listIndex  = index;

            int previousIndex = index - 1;

            if (previousIndex < 0)
                previousIndex = offsets.Count - 1;

            // =========================================================

            pointA = startPosition + offsets[previousIndex];
            pointB = startPosition + offsets[listIndex];

            travelDistance = Vector3.Distance(pointA, pointB);

            waitTimer = 0f;

            elapsedTime = 0f;

            // =========================================================

            Vector3 difference = pointB - pointA;

            moveDirection = difference.normalized;

            // =========================================================

            moving = true;

            targetReached = false;

            transform.position = pointA;
        }

        private static float GetAngle(Vector3 current, Vector3 target, Vector3 axis)
        {
            float angle = Vector3.SignedAngle(current, target, axis);

            if (angle < 0f)
            {
                angle = 360.0f + angle;
            }

            return angle;
        }

        public void SetTarget(int index)
        {
            if (index < offsets.Count)
            {
                pointA = pointB;
                pointB = startPosition + offsets[index];

                travelDistance = Vector3.Distance(pointA, pointB);

                elapsedTime = 0f;

                targetReached = false;
            }
        }
        
        // =========================================================
        //    Network Methods and Variables
        // =========================================================

        private CustomNetworkManager networkManager;

        private bool soloInstance;

        private bool isOwned;

        // [Command]
        private void CmdSetPlatformTarget(int index)
        {
            networkManager.SetPlatformTarget(this, index);
        }

        // [ClientRpc]
        public void RpcSetPlatformTarget(int index)
        {
            if (!isOwned)
            {
                SetPlatformTarget(index);
            }
        }
    }
}









