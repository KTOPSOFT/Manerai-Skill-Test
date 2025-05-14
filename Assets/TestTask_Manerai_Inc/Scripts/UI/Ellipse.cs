using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class Ellipse : MonoBehaviour
    {
        public float majorAxis = 512.0f;
        public float minorAxis = 512.0f;

        public float angle;

        public Transform image;
        public Transform arrow;

        private Vector3 point;

        private float distance;

        private void OnValidate()
        {
            SetAngle(angle * Mathf.Deg2Rad);
        }

        public void SetAngle(float angle)
        {
            this.angle = angle * Mathf.Rad2Deg;

            angle += Mathf.PI / 2.0f;

            // =========================================================

            distance = DetermineDistance(majorAxis, minorAxis, angle);

            point = DeterminePoint(distance, angle);

            // =========================================================

            if (image != null)
            {
                image.localPosition = point;
            }

            if (arrow != null)
            {
                arrow.localEulerAngles = Vector3.forward * this.angle;
            }
        }

        private static Vector2 DeterminePoint(float distance, float angle)
        {
            float x = distance * Mathf.Cos(angle);
            float y = distance * Mathf.Sin(angle);

            Vector2 point = new Vector2(x, y);

            return point;
        }

        private static float DetermineDistance(float majorAxis, float minorAxis, float angle)
        {
            float a = majorAxis / 2.0f;
            float b = minorAxis / 2.0f;

            float termA = a * a * Mathf.Pow(Mathf.Sin(angle), 2);
            float termB = b * b * Mathf.Pow(Mathf.Cos(angle), 2);

            float distance = (a * b) / Mathf.Sqrt(termA + termB);

            return distance;
        }

        public Vector3 GetPoint()
        {
            return point;
        }

        public float GetDistance()
        {
            return distance;
        }

        public static Vector3 GetPointAndDistance(float majorAxis, float minorAxis, float angle)
        {
            float distance = DetermineDistance(majorAxis, minorAxis, angle);

            Vector2 point = DeterminePoint(distance, angle);

            return new Vector3(point.x, point.y, distance);
        }
    }
}





