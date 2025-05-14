using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TMPro;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasFadeIn : MonoBehaviour
    {
        private CanvasGroup canvasGroup;

        private float canvasAlpha;
        private float alphaVelocity;

        private int targetAlpha;

        private bool fadeIn;
        private bool updateAlpha;

        public bool fadeOnEnable;

        public float startingScale = 1.0f;

        public float smoothTime = 0.15f;

        public GameEvent onCanvasHidden = new GameEvent();
        
        // =====================================

        private List<SmoothTransform> smoothTransforms = new List<SmoothTransform>();
        
        private int listCount;
        
        // =====================================

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            GetSmoothTransforms();
        }

        private void OnEnable()
        {
            if (fadeOnEnable)
            {
                FadeIn(true);
            }
        }

        private void OnDisable()
        {
            ResetValues();
        }
        
        private void OnValidate()
        {
            ResetSmoothTransforms();
        }

        private void Update()
        {
            if (updateAlpha)
            {
                canvasAlpha = Mathf.SmoothDamp(canvasAlpha, targetAlpha, ref alphaVelocity, smoothTime);

                if (targetAlpha == 1 && canvasAlpha > 0.99f)
                {
                    canvasAlpha =  1.0f;

                    updateAlpha = false;
                }

                else if (targetAlpha == 0 && canvasAlpha < 0.01f)
                {
                    canvasAlpha = 0f;

                    updateAlpha = false;

                    onCanvasHidden.Invoke();
                }

                canvasGroup.alpha = canvasAlpha;
            }
        }

        public void FadeIn(bool value)
        {
            if (value && gameObject.activeInHierarchy)
            {
                fadeIn = true;

                StartCoroutine(IE_FadeIn());
            }

            else
            {
                fadeIn = false;

                targetAlpha = 0;

                updateAlpha = true;

                // =====================================

                ScaleTowards(Vector3.zero, smoothTime);
            }
        }

        private IEnumerator IE_FadeIn()
        {
            ResetValues();

            // =====================================

            yield return null;

            if (fadeIn)
            {
                targetAlpha = 1;

                ScaleTowards(Vector3.one, smoothTime);

                updateAlpha = true;
            }
        }

        private void ScaleTowards(Vector3 value, float smoothTime)
        {
            for (int i = 0; i < listCount; i ++)
            {
                smoothTransforms[i].ScaleTowards(value, smoothTime);
            }
        }

        private void ResetSmoothTransforms()
        {
            for (int i = 0; i < listCount; i ++)
            {
                SmoothTransform currentTransform = smoothTransforms[i];
                
                currentTransform.restartOnDisable = true;
                
                currentTransform.useStartingScale = true;
                
                // =====================================
                
                currentTransform.startingScale = Vector3.one * startingScale;
            }
        }

        private void GetSmoothTransforms()
        {
            smoothTransforms.Clear();

            // =====================================

            int childCount = transform.childCount;

            for (int i = 0; i < childCount; i ++)
            {
                SmoothTransform currentChild = transform.GetChild(i).GetComponent<SmoothTransform>();

                if (currentChild != null)
                {
                    smoothTransforms.Add(currentChild);
                }
            }
            
            listCount = smoothTransforms.Count;
        }

        private void ResetValues()
        {
            updateAlpha = false;

            canvasAlpha = 0f;

            canvasGroup.alpha = canvasAlpha;

            ResetSmoothTransforms();
        }

        public bool GetFadeIn()
        {
            return fadeIn;
        }
    }
}













