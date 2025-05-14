using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(CanvasGroup))]
    public class WorldToScreenUI : MonoBehaviour
    {
        public Vector3 indicatorOffset;

        public bool showOffscreen;

        public Transform arrow;

        private Camera m_camera;

        private Canvas canvas;
        private CanvasGroup canvasGroup;

        private RectTransform canvasTransform;
        private RectTransform rectTransform;

        private Vector3 offset;

        private bool hideCanvas;

        private bool targetInFrame;

        private float angle;

        private void Awake()
        {
            m_camera = Camera.main;

            canvas = transform.GetChild(0).GetComponent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();

            canvasTransform = canvas.GetComponent<RectTransform>(); 
            rectTransform = canvasTransform.GetChild(0).GetComponent<RectTransform>();

            canvas.sortingOrder = -1; // be right behind the main canvas (sort order 0)
        }

        private void Start() // using Awake() to retrieve canvasTransform.sizeDelta will result in wrong values
        {
            offset = new Vector3(canvasTransform.sizeDelta.x / 2.0f, canvasTransform.sizeDelta.y / 2.0f, 0f);
        }

        private void OnEnable()
        {
            canvasGroup.alpha = 0f;

            UpdateScreenPosition();

            canvasGroup.alpha = 1.0f;
        }

        private void OnDisable()
        {
            canvasGroup.alpha = 0f;
        }

        private void LateUpdate()
        {
            UpdateScreenPosition();
        }

        private void UpdateScreenPosition()
        {
            if (m_camera != null)
            {
                float canvasScale = canvasTransform.localScale.x;

                Vector3 screenPosition = m_camera.WorldToScreenPoint(transform.position);

                Vector3 scaledPosition = (screenPosition / canvasScale) - offset + indicatorOffset;

                // ======================================================

                if (showOffscreen)
                {
                    bool targetVisible = TargetVisible(screenPosition, scaledPosition);

                    if (!targetVisible)
                    {
                        scaledPosition = GetOffscreenPosition(scaledPosition);
                    }

                    // ======================================================

                    Vector3 origin = Vector3.up * 45.0f;

                    angle = Mathf.Atan2(scaledPosition.y - origin.y, scaledPosition.x - origin.x);

                    Vector3 output = Ellipse.GetPointAndDistance(1375.0f, 800.0f, angle);

                    // ======================================================

                    float distance = Vector3.Distance(origin, scaledPosition);

                    targetInFrame = distance <= output.z;

                    if (!targetInFrame)
                    {
                        scaledPosition = new Vector3(output.x, output.y + 45.0f, 0f);
                    }
                }

                else
                {
                    if (screenPosition.z < 0f || hideCanvas)
                        canvasGroup.alpha = 0f;

                    else
                        canvasGroup.alpha = 1.0f;
                }

                // ======================================================

                rectTransform.localPosition = new Vector3(scaledPosition.x, scaledPosition.y, 0f);
            }
        }

        private static bool TargetVisible(Vector3 screenPosition, Vector3 scaledPosition)
        {
            Vector2 screenBounds = new Vector2(1375.0f, 800.0f) / 2.0f;

            bool targetVisible = screenPosition.z >= 0f;

            if (targetVisible)
            {
                float x = scaledPosition.x;
                float y = scaledPosition.y;

                targetVisible = (x > -screenBounds.x) && (x < screenBounds.x) && (y > -screenBounds.y + 45.0f) && (y < screenBounds.y + 45.0f);
            }

            return targetVisible;
        }

        private static Vector3 GetOffscreenPosition(Vector3 screenPosition)
        {
            Vector3 screenBounds = new Vector3(1375.0f, 800.0f, 0f) / 2.0f;

            if (screenPosition.z < 0f)
            {
                screenPosition *= -1.0f;
            }

            float angle = Mathf.Atan2(screenPosition.y, screenPosition.x);

            float slope = Mathf.Tan(angle);

            // ======================================================

            if (screenPosition.x > 0)
            {
                screenPosition = new Vector3(screenBounds.x, screenBounds.x * slope, 0f);
            }

            else
            {
                screenPosition = new Vector3(-screenBounds.x, -screenBounds.x * slope, 0f);
            }

            // ======================================================

            if (screenPosition.y > screenBounds.y + 45.0f)
            {
                screenPosition = new Vector3((screenBounds.y + 45.0f) / slope, screenBounds.y + 45.0f, 0f);
            }

            else if (screenPosition.y < -screenBounds.y + 45.0f)
            {
                screenPosition = new Vector3((-screenBounds.y + 45.0f) / slope, -screenBounds.y + 45.0f, 0f);
            }

            // ======================================================

            return screenPosition;
        }

        public bool GetTargetInFrame()
        {
            return targetInFrame;
        }

        public float GetAngle()
        {
            return angle;
        }
    }
}





