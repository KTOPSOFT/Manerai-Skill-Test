using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class SmoothTransform : MonoBehaviour
    {   
        private Vector3 currentPosition;
        private Vector3 currentRotation;
        private Vector3 currentScale;

        public Vector3 targetPosition;
        public Vector3 targetRotation;
        public Vector3 targetScale;

        public float positionSmoothTime = 0.15f;
        public float rotationSmoothTime = 0.15f;
        public float scaleSmoothTime = 0.15f;

        private float deltaTime;

        private Vector3 positionVelocity;
        private Vector3 rotationVelocity;
        private Vector3 scaleVelocity;

        // ======================================

        public bool useStartingPosition = false;
        public bool useStartingRotation = false;
        public bool useStartingScale = false;

        public Vector3 startingPosition;
        public Vector3 startingRotation;
        public Vector3 startingScale;

        private Vector3 startingTargetPosition;
        private Vector3 startingTargetRotation;
        private Vector3 startingTargetScale;

        private float startingPositionSmoothTime;
        private float startingRotationSmoothTime;
        private float startingScaleSmoothTime;

        // ======================================

        public bool useUnscaledTime= false;

        public bool restartOnDisable = false;

        private void Awake()
        {
            SetStartingValues();
        }

        private void Update()
        {
            if (useUnscaledTime)
                deltaTime = Time.unscaledDeltaTime;

            else
                deltaTime = Time.deltaTime;

            // =============================================================

            UpdatePosition();

            if (!VectorsEqual(currentRotation, targetRotation))
                UpdateRotation();

            if (!VectorsEqual(currentScale, targetScale))
                UpdateScale();
        }

        private void OnDisable()
        {
            ResetValues();
        }

        public void UpdatePosition()
        {        
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref positionVelocity, positionSmoothTime, Mathf.Infinity, deltaTime);
        }

        private void UpdateRotation()
        {
            currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationVelocity, rotationSmoothTime, Mathf.Infinity, deltaTime);

            transform.localRotation = Quaternion.Euler(RemoveNegligibleDifference(currentRotation, targetRotation));
        }

        private void UpdateScale()
        {
            currentScale = Vector3.SmoothDamp(currentScale, targetScale, ref scaleVelocity, scaleSmoothTime, Mathf.Infinity, deltaTime);

            transform.localScale = RemoveNegligibleDifference(currentScale, targetScale);
        }

        public void TranslateTowards(Vector3 target, float smoothTime)
        {
            positionSmoothTime = smoothTime;

            targetPosition = target;
        }

        public void RotateTowards(Vector3 target, float smoothTime)
        {
            rotationSmoothTime = smoothTime;

            targetRotation = target;

            // keep the target rotation inside the [0, 360] range:

            while (targetRotation.x > 360.0f)
                targetRotation.x -= 360.0f;

            while (targetRotation.x < 0f)
                targetRotation.x += 360.0f;

            while (targetRotation.y > 360.0f)
                targetRotation.y -= 360.0f;

            while (targetRotation.y < 0f)
                targetRotation.y += 360.0f;

            while (targetRotation.z > 360.0f)
                targetRotation.z -= 360.0f;

            while (targetRotation.z < 0f)
                targetRotation.z += 360.0f;

            // change [currentRotation] so that the object chooses the shortest path towards [targetRotation]:

            if (Mathf.Abs(targetRotation.x - currentRotation.x) > 180.0f)
            {
                if (targetRotation.x > currentRotation.x)
                    currentRotation.x += 360.0f;

                else
                    currentRotation.x -= 360.0f;
            }

            if (Mathf.Abs(targetRotation.y - currentRotation.y) > 180.0f)
            {
                if (targetRotation.y > currentRotation.y)
                    currentRotation.y += 360.0f;

                else
                    currentRotation.y -= 360.0f;
            }

            if (Mathf.Abs(targetRotation.z - currentRotation.z) > 180.0f)
            {
                if (targetRotation.z > currentRotation.z)
                    currentRotation.z += 360.0f;

                else
                    currentRotation.z -= 360.0f;
            }
        }

        public void ScaleTowards(Vector3 target, float smoothTime)
        {
            scaleSmoothTime = smoothTime;

            targetScale = target;
        }

        private Vector3 RemoveNegligibleDifference(Vector3 current, Vector3 target)
        {
            float x = current.x;
            float y = current.y;
            float z = current.z;

            if (Mathf.Abs(current.x - target.x) < 0.005f)
                x = target.x;

            if (Mathf.Abs(current.y - target.y) < 0.005f)
                y = target.y;

            if (Mathf.Abs(current.z - target.z) < 0.005f)
                z = target.z;

            return new Vector3(x, y, z);
        }

        private void SetStartingValues()
        {
            currentPosition = targetPosition = startingTargetPosition = transform.localPosition;

            currentRotation = targetRotation = startingTargetRotation = transform.localEulerAngles;

            currentScale = targetScale = startingTargetScale = transform.localScale;

            // =============================================================

            startingPositionSmoothTime = positionSmoothTime;

            startingRotationSmoothTime = rotationSmoothTime;

            startingScaleSmoothTime = scaleSmoothTime;

            // =============================================================

            if (useStartingPosition)
                transform.localPosition = currentPosition = startingPosition;

            if (useStartingRotation)
                transform.localEulerAngles = currentRotation = startingRotation;

            if (useStartingScale)
                transform.localScale = currentScale = startingScale;
        }

        private void ResetValues()
        {
            if (restartOnDisable)
            {
                if (useStartingPosition)
                {
                    transform.localPosition = currentPosition = startingPosition;

                    targetPosition = startingTargetPosition;

                    positionSmoothTime = startingPositionSmoothTime;
                }

                if (useStartingRotation)
                {
                    transform.localEulerAngles = currentRotation = startingRotation;

                    targetRotation = startingTargetRotation;

                    rotationSmoothTime = startingRotationSmoothTime;
                }

                if (useStartingScale)
                {
                    transform.localScale = currentScale = startingScale;

                    targetScale = startingTargetScale;

                    scaleSmoothTime = startingScaleSmoothTime;
                }
            }
        }

        private static bool VectorsEqual(Vector3 current, Vector3 target)
        {
            return (current.x == target.x) && (current.y == target.y) && (current.z == target.z);
        }
    }
}










