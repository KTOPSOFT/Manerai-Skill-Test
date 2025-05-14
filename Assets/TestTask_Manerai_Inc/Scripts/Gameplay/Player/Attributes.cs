using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TMPro;

namespace YukiOno.SkillTest
{
    public class Attributes : MonoBehaviour
    {
        // =========================================================
        //    Attributes
        // =========================================================

        public Attribute health;

        [Space(10)]

        public Attribute energy;

        [Space(10)]

        public GameEvent onHealthDepleted;

        public bool resetHealthOnEnable;

        public bool fullHealth { get { return health.currentValue >= health.maxValue; } }

        // =========================================================
        //    Attack Variables
        // =========================================================

        [Space(10)]

        public float baseAttack = 10.0f;
        public float attackModifier = 1.0f;

        public float criticalRate = 0f;
        public float criticalModifier = 1.25f;

        private bool ignoreDamageModifiers;

        // =========================================================
        //    Evasion Variables
        // =========================================================

        private float evadeWindow = 0.25f;

        private float evadeTimer = 0f;

        // =========================================================
        //    Component Dependencies
        // =========================================================

        private Animator animator;

        private bool usingAnimator;

        // =========================================================
        //    Animation Parameters
        // =========================================================

        readonly int hashEnergy = Animator.StringToHash("Energy");

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {
            animator = GetComponent<Animator>();

            usingAnimator = animator != null;

            // =========================================================

            health.CheckForMeter();
            energy.CheckForMeter();

            ResetHealth(true);
            ResetEnergy();
        }

        private void OnEnable()
        {
            if (resetHealthOnEnable)
            {
                ResetHealth(true);
            }
        }

        private IEnumerator RunEvadeTimer()
        {
            evadeTimer = evadeWindow;

            while (evadeTimer > 0f)
            {
                evadeTimer -= Time.deltaTime;

                if (evadeTimer < 0f)
                {
                    evadeTimer = 0f;
                }

                yield return null;
            }
        }

        public bool GetIgnoreDamageModifiers()
        {
            return ignoreDamageModifiers;
        }

        public float GetEvadeTimer()
        {
            return evadeTimer;
        }

        public void ResetHealth()
        {
            bool hideMeter = false;

            ResetHealth(hideMeter);
        }

        public void ResetHealth(bool hideMeter)
        {
            Meter healthMeter = health.meter;

            health.SetCurrentValue(health.maxValue);

            if (healthMeter != null)
            {
                healthMeter.ResetValues();

                if (hideMeter)
                {
                    healthMeter.HideMeter();
                }
            }

            UpdateTextValues(health);
        }

        public void ResetEnergy()
        {
            energy.SetCurrentValue(energy.maxValue);

            if (energy.meter != null)
            {
                energy.meter.ResetValues();
            }

            SetInteger(hashEnergy, (int) energy.currentValue);

            UpdateTextValues(energy);
        }

        public void UpdateHealth(float delta) // called by Target.cs
        {
            Meter healthMeter = health.meter;

            bool useHealthMeter = health.useMeter;

            if (useHealthMeter && delta < 0f)
            {
                healthMeter.FlashSubBar(Mathf.Abs(delta), health.maxValue);
            }

            // =============================================================

            float newValue = UpdateAttribute(health.currentValue, health.maxValue, delta);

            health.SetCurrentValue(newValue);

            if (health.currentValue <= 0f)
            {
                onHealthDepleted.Invoke();
            }

            // =============================================================

            if (useHealthMeter)
            {
                healthMeter.SetValue(health.currentValue / health.maxValue);
            }

            UpdateTextValues(health);
        }

        public void UpdateEnergy(float delta) // called by AttackSource.cs
        {
            Meter energyMeter = energy.meter;

            bool useEnergyMeter = energy.useMeter;

            float newValue = UpdateAttribute(energy.currentValue, energy.maxValue, delta);

            energy.SetCurrentValue(newValue);

            SetInteger(hashEnergy, (int) energy.currentValue);

            // =============================================================

            if (useEnergyMeter)
            {
                energyMeter.SetValue(energy.currentValue / energy.maxValue);
            }

            UpdateTextValues(energy);
        }

        private static float UpdateAttribute(float current, float max, float delta)
        {
            current += delta;

            if (current > max)
            {
                current = max;
            }

            else if (current <= 0f)
            {
                current = 0f;
            }

            return current;
        }

        private void SetInteger(int hash, int value)
        {
            if (usingAnimator)
            {
                animator.SetInteger(hash, value);
            }
        }

        private static void UpdateTextValues(Attribute attribute)
        {
            bool useText = attribute.textValues.useText;

            if (useText)
            {
                bool monospace = attribute.textValues.monospace;

                float spacing = attribute.textValues.spacing;

                SetTextValue(attribute.textValues.currentValue, attribute.currentValue, monospace, spacing);

                SetTextValue(attribute.textValues.maxValue, attribute.maxValue, monospace, spacing);
            }
        }

        private static void SetTextValue(TextMeshProUGUI textMesh, float value, bool monospace, float spacing)
        {
            string text;

            if (monospace)
            {
                text = "<mspace=" + spacing + "em>" + (int) value;
            }

            else
            {
                text = "" + (int) value;
            }

            textMesh.SetText(text);
        }

        // =========================================================
        //    Attribute Class
        // =========================================================

        [System.Serializable]
        public class Attribute
        {
            public float maxValue = 100.0f;

            [SerializeField]
            private float m_currentValue;

            public Meter meter;

            private bool m_useMeter;

            [Space(10)]

            public TextValues textValues;

            public float currentValue { get { return m_currentValue; } }

            public bool useMeter { get { return m_useMeter; } }

            public void SetCurrentValue(float value)
            {
                m_currentValue = value;
            }

            public void CheckForMeter()
            {
                m_useMeter = (meter != null);
            }
        }

        [System.Serializable]
        public class TextValues
        {
            public bool useText;

            [Space(10)]

            public bool monospace;

            public float spacing = 0.575f;

            [Space(10)]

            public TextMeshProUGUI maxValue;
            public TextMeshProUGUI currentValue;
        }
    }
}



