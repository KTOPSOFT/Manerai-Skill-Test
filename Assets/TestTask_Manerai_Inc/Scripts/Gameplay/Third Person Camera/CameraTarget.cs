using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class CameraTarget : MonoBehaviour
    {
        private Transform target;

        private PlayerController player;

        private float smoothTime;

        private bool lockPosition;

        private bool lockSmoothTime;
        private bool useTargetPosition;

        private Vector3 smoothVelocity;

        private Vector3 targetPosition;

        // =========================================================

        private bool useFreeTransform;

        private Transform freeTransform;

        // =========================================================

        private float deltaY; // the difference in height between this and the player at the precise moment player becomes airborne
        private float smoothRef;

        private bool isGrounded = true;
        private bool airborne;

        private bool heightCentered;

        // =========================================================

        private bool useFixedUpdate;

        private float deltaTime;

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {
            lockPosition = false;

            targetPosition = transform.position;

            gameObject.hideFlags = HideFlags.HideInHierarchy;
        }

        private void FixedUpdate()
        {
            if (useFixedUpdate && !lockPosition)
            {
                deltaTime = Time.fixedDeltaTime;

                UpdatePosition();
            }
        }

        private void LateUpdate()
        {
            if (!useFixedUpdate && !lockPosition)
            {
                deltaTime = Time.deltaTime;

                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            if (useTargetPosition)
            {
                transform.position = FollowTarget(transform.position, targetPosition, false);
            }

            else if (target != null)
            {
                transform.localPosition = FollowTarget(transform.localPosition, RelativePosition(target), false);
            }

            else if (player != null)
            {
                transform.localPosition = FollowTarget(transform.localPosition, RelativePosition(player.transform), true);
            }
        }

        private Vector3 RelativePosition(Transform target)
        {
            Vector3 relativePosition = target.localPosition;

            if (useFreeTransform)
            {
                freeTransform.position = target.position;

                relativePosition = freeTransform.localPosition;
            }

            return relativePosition;
        }

        public void LockSmoothTime(bool value)
        {
            lockSmoothTime = value;
        }

        public void SetSmoothTime(float value)
        {
            if (!lockSmoothTime)
            {
                smoothTime = value;
            }
        }

        public void SetParent(Transform newParent, bool resetVelocity)
        {   
            transform.SetParent(newParent);

            freeTransform.SetParent(newParent);

            smoothVelocity = resetVelocity ? Vector3.zero : smoothVelocity;
        }

        public void SetTarget(Transform newTarget, bool instant)
        {
            ResetValues();

            target = newTarget;

            smoothVelocity = Vector3.zero;

            useTargetPosition = false;

            if (instant)
            {
                transform.position = newTarget.position;
            }
        }

        public void SetPlayer(PlayerController player, bool instant)
        {
            ResetValues();

            this.player = player;

            smoothVelocity = Vector3.zero;

            useTargetPosition = false;

            if (instant)
            {
                transform.position = player.transform.position;
            }
        }

        public void SetTargetPosition(Vector3 position, bool instant)
        {
            ResetValues();

            targetPosition = position;

            smoothVelocity = Vector3.zero;

            useTargetPosition = true;

            if (instant)
            {
                transform.position = position;
            }
        }

        public void SetToTargetPosition()
        {
            if (useTargetPosition)
            {
                transform.position = targetPosition;
            }

            else if (target != null)
            {
                transform.localPosition = target.localPosition;
            }

            else if (player != null)
            {
                transform.localPosition = player.transform.localPosition;
            }
        }

        private void ResetValues()
        {
            isGrounded = true;
            airborne = false;

            heightCentered = false;
        }

        private Vector3 FollowTarget(Vector3 cameraPosition, Vector3 targetPosition, bool followPlayer)
        {
            if (followPlayer)
            {
                isGrounded = player.GetIsGrounded();

                if (!isGrounded)
                {
                    if (airborne)
                    {
                        if (!heightCentered)
                        {
                            deltaY = SmoothDamp(deltaY, 0f, ref smoothRef, 0.15f);

                            if (Mathf.Abs(deltaY) < 0.0001f)
                            {
                                deltaY = 0f;

                                heightCentered = true;
                            }
                        }
                    }

                    else
                    {
                        Transform parent = transform.parent;

                        Vector3 worldPosition = targetPosition;

                        if (parent != null)
                        {
                            worldPosition = transform.parent.TransformPoint(targetPosition);
                        }

                        deltaY = worldPosition.y - transform.position.y;

                        airborne = true;
                        heightCentered = false;
                    }
                }

                else
                {
                    deltaY = 0f;
                }

                // =========================================================

                if (airborne && isGrounded)
                {
                    airborne = false;
                }
            }


            float x = SmoothDamp(cameraPosition.x, targetPosition.x, ref smoothVelocity.x, smoothTime);
            float z = SmoothDamp(cameraPosition.z, targetPosition.z, ref smoothVelocity.z, smoothTime);

            float y = isGrounded ? SmoothDamp(cameraPosition.y, targetPosition.y, ref smoothVelocity.y, smoothTime): targetPosition.y - deltaY;

            Vector3 newPosition = new Vector3(x, y, z);

            return newPosition;
        }

        private float SmoothDamp(float current, float target, ref float velocity, float smoothTime)
        {
            float smoothDamp = Mathf.SmoothDamp(current, target, ref velocity, smoothTime, Mathf.Infinity, deltaTime);

            return smoothDamp;
        }

        public void UseFixedUpdate(bool value) // called by PlayerController.cs
        {
            useFixedUpdate = value;
        }

        public void LockPosition(bool value) // called by PauseMenu.cs
        {
            lockPosition = value;
        }

        public void SetFreeTransform(Transform target) // called by SceneHandler.cs
        {
            freeTransform = target;
        }

        public void UseFreeTransform(bool value) // called by PlayerController.cs
        {
            useFreeTransform = value;
        }
    }
}




