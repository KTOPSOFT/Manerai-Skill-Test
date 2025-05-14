using UnityEngine;

using System;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class CustomAnimation : MonoBehaviour
    {
        public float magnitude = 1.0f;
        public float radianOffset;

        [Serializable]
        public struct AnimationPoint
        {
            public Vector3 position;
            public Vector3 rotation;

            public float timeInterval;
        }

        private bool animationPlaying;

        private Vector3 startingPosition;

        public List<AnimationPoint> animationPoints;

        private float timer;
        private float progress;

        private AnimationPoint currentPoint;
        private AnimationPoint previousPoint;

        private int currentIndex;

        private Vector3 currentPosition;
        private Vector3 currentRotation;

        private void Start()
        {
            startingPosition = transform.localPosition;
        }

        private void Update()
        {
            if (animationPlaying)
            {
                timer += Time.deltaTime;

                progress = AnimationCurves.Sinusoidal.InOut(timer / currentPoint.timeInterval);

                currentPosition.x = progress * (currentPoint.position.x - previousPoint.position.x) + previousPoint.position.x;
                currentPosition.y = progress * (currentPoint.position.y - previousPoint.position.y) + previousPoint.position.y;
                currentPosition.z = progress * (currentPoint.position.z - previousPoint.position.z) + previousPoint.position.z;

                currentRotation.x = progress * (currentPoint.rotation.x - previousPoint.rotation.x) + previousPoint.rotation.x;
                currentRotation.y = progress * (currentPoint.rotation.y - previousPoint.rotation.y) + previousPoint.rotation.y;
                currentRotation.z = progress * (currentPoint.rotation.z - previousPoint.rotation.z) + previousPoint.rotation.z;

                // ==================================================

                Vector3 newRotation = currentRotation;

                newRotation.x = currentRotation.x * Mathf.Cos(radianOffset) - currentRotation.z * Mathf.Sin(radianOffset);
                newRotation.z = currentRotation.z * Mathf.Cos(radianOffset) - currentRotation.x * Mathf.Sin(radianOffset);

                // ==================================================

                transform.localPosition = currentPosition * magnitude + startingPosition;
                transform.localEulerAngles = newRotation * magnitude;

                if (timer >= currentPoint.timeInterval)
                {
                    if (currentIndex == animationPoints.Count - 1)
                    {
                        transform.localPosition = startingPosition;
                        transform.localRotation = Quaternion.identity;

                        animationPlaying = false;
                    }

                    else
                    {
                        currentIndex ++;

                        currentPoint = animationPoints[currentIndex];
                        previousPoint = animationPoints[currentIndex - 1];

                        timer -= previousPoint.timeInterval;
                    }
                }
            }
        }

        public void PlayAnimation()
        {
            currentPosition = Vector3.zero;
            currentRotation = Vector3.zero;

            transform.localPosition = startingPosition;
            transform.localRotation = Quaternion.identity;

            currentIndex = 0;

            currentPoint = animationPoints[currentIndex];
            previousPoint = new AnimationPoint();

            timer = 0f;

            animationPlaying = true;
        }
    }
}




