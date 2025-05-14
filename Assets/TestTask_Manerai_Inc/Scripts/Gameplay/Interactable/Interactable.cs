using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TMPro;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(SphereCollider))]
    public class Interactable : MonoBehaviour
    {
        public string m_name;

        public int interactionType;

        public bool viewDependent;

        // =========================================================

        protected int startingLayer;

        protected float interactionRange = 3.0f;

        protected float validTargetRange;

        protected float targetRange;

        // =========================================================

        protected SphereCollider sphereCollider;

        protected PlayerInteraction interaction;

        protected CanvasFadeIn canvasFade;

        // =========================================================

        public GameObject canvas;

        public TextMeshProUGUI nameText;

        private SmoothTransform smoothTransform;

        // =========================================================

        [Space(10)]

        public GameEvent onInteractionStart;

        // =========================================================

        [Space(10)]

        public SubTag subTag = new SubTag();

        [System.Serializable]
        public class SubTag
        {
            public string name = "";

            public string richTextTags= "";

            public float lineOffset;
        }

        // =========================================================
        //    Standard Methods
        // =========================================================

        protected virtual void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            interaction = elements.GetComponent<PlayerManager>().playerInteraction;

            if (interactionRange > 0f)
            {
                validTargetRange = Mathf.Pow(interactionRange, 2.0f);

                targetRange = Mathf.Pow(interactionRange + 1.0f, 2.0f);
            }

            // =========================================================

            startingLayer = gameObject.layer;

            gameObject.layer = elements.GetComponent<SceneHandler>().GetInteractionLayer();

            // =========================================================

            sphereCollider = GetComponent<SphereCollider>();

            sphereCollider.radius = 0.1f;

            // =========================================================

            if (nameText != null)
            {
                canvasFade = nameText.transform.parent.GetComponent<CanvasFadeIn>();

                smoothTransform = nameText.GetComponent<SmoothTransform>();

                SetNameTag();
            }

            if (canvas != null)
            {
                canvas.SetActive(false);
            }
        }

        protected virtual void OnDisable()
        {
            interaction.RemoveTarget(this);

            // =========================================================

            if (canvas != null)
            {
                canvas.SetActive(false);
            }
        }

        protected virtual void Reset()
        {
            sphereCollider = GetComponent<SphereCollider>();

            sphereCollider.radius = 0.1f;
        }

        protected virtual void OnValidate()
        {
            if (nameText != null)
            {
                SetNameTag();
            }
        }

        private void SetNameTag()
        {
            string name = m_name;

            float lineOffset = 0f;

            if (subTag.name.Length > 0)
            {
                name = m_name + "<br>" + subTag.richTextTags + "<\u200B" + subTag.name + "\u200B>";

                lineOffset = subTag.lineOffset;
            }

            nameText.SetText(name);

            // =========================================================

            nameText.transform.localPosition = Vector3.up * lineOffset;

            if (smoothTransform != null)
            {
                smoothTransform.targetPosition = Vector3.up * lineOffset;
            }
        }

        public void ShowTargetName(bool value) // called by PlayerInteraction.cs
        {
            if (canvas != null && canvasFade != null)
            {
                canvas.SetActive(true);

                canvasFade.FadeIn(value);
            }
        }

        public virtual void StartInteraction() // called by PlayerInteraction.cs
        {
            onInteractionStart.Invoke();
        }

        // =========================================================
        //    Get Methods
        // =========================================================

        public int GetStartingLayer() // called by InteractionTrigger.cs
        {
            return startingLayer;
        }

        public float GetInteractionRange() // called by PlayerInteraction.cs
        {
            return interactionRange;
        }

        public float GetValidTargetRange() // called by PlayerInteraction.cs
        {
            return validTargetRange;
        }

        public float GetTargetRange() // called by PlayerInteraction.cs
        {
            return targetRange;
        }
    }
}




