using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class DamageIndicator : MonoBehaviour
    { 
        private float currentAlpha;

        private Transform container;

        private CanvasGroup canvasGroup;

        private float containerScale;

        private float smoothVelocity;

        private float currentPosition = 0f;
        private float timer = 0f;

        private bool forceExit;

        private bool canvasEnabled; // use this instead of calling gameObject.SetActive(), which generates garbage from the TextMeshProUGUI component

        private SmoothTransform smoothTransform;

        private TextGroup textGroup;

        // ========================================

        public CanvasGroup textCanvas;

        public float scrollSpeed = 0f;
        public float exitSpeed = 400.0f;

        // ========================================

        [Space(10)]

        public bool monospace;

        public float spacing = 0.575f;

        // ========================================

        private PlayerController sender;

        private Target receiver;

        private void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            container = transform.GetChild(0);

            smoothTransform = container.GetChild(0).GetComponent<SmoothTransform>();

            textGroup = GetComponent<TextGroup>();

            SetWorldCamera(elements);

            // ========================================

            canvasGroup = GetComponent<CanvasGroup>();

            canvasGroup.alpha = 1.0f;

            // ========================================

            currentAlpha = 0f;

            textCanvas.alpha = currentAlpha;
        }

        public void EnableCanvas() // called by Target.cs
        {
            timer = 0f;

            currentPosition = 0f;

            container.localPosition = Vector3.zero;

            containerScale = container.localScale.x;

            // =====================================

            currentAlpha = 1.0f;

            textCanvas.alpha = currentAlpha;

            // =====================================

            smoothTransform.enabled = true;

            canvasEnabled = true;
        }

        private void ReturnToPool()
        {
            canvasEnabled = false;

            // =====================================

            currentAlpha = 0f;

            textCanvas.alpha = currentAlpha;

            // =====================================

            transform.position = Vector3.down * 1000.0f;

            container.localPosition = Vector3.zero;

            smoothTransform.enabled = false;
        }

        private void Update()
        {
            if (canvasEnabled)
            {
                timer += Time.deltaTime;

                if (timer >= 1.0f || forceExit)
                {
                    currentPosition += Time.deltaTime * exitSpeed;

                    currentAlpha = Mathf.SmoothDamp(currentAlpha, 0f, ref smoothVelocity, 0.15f);

                    textCanvas.alpha = currentAlpha;
                }

                else
                {
                    currentPosition += Time.deltaTime * scrollSpeed;
                }

                container.localPosition = Vector3.up * currentPosition * containerScale;

                if (currentAlpha < 0.01f)
                {
                    receiver.GetActiveIndicators().Remove(this);

                    if (this == receiver.GetCenteredIndicator())
                        receiver.SetCenteredIndicator(null);

                    forceExit = false;

                    ReturnToPool();
                }
            }
        }

        public void ForceExit()
        {
            forceExit = true;
        }

        public void SetSender(PlayerController sender) // called by Target.cs
        {
            this.sender = sender;
        }

        public void SetReceiver(Target receiver) // called by Target.cs
        {
            this.receiver = receiver;
        }

        public void SetText(int damage) // called by Target.cs
        {
            if (monospace)
            {   
                textGroup.SetText(string.Format("<mspace={0}em>{1}</mspace>", spacing, damage));
            }

            else
            {
                textGroup.SetText(damage.ToString());
            }
        }

        private void SetWorldCamera(Transform elements)
        {
            Canvas canvas = GetComponent<Canvas>();

            Camera UI_Camera = elements.GetComponent<SceneHandler>().GetUICamera();

            canvas.worldCamera = UI_Camera;
        }

        public void SetCanvasGroupAlpha(float value)
        {
            canvasGroup.alpha = value;
        }
    }
}









