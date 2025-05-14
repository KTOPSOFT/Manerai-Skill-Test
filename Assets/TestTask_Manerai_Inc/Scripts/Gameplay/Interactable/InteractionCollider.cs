using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class InteractionCollider : MonoBehaviour
    {
        private Attributes attributes;

        public void Initialize(Attributes attributes, int layer) // called by Target.cs
        {
            this.attributes = attributes;

            gameObject.layer = layer;
        }

        public void ShowHealthMeter()
        {
            if (attributes != null)
            {
                Meter healthMeter = attributes.health.meter;

                if (healthMeter != null && !attributes.fullHealth)
                {
                    bool active = healthMeter.gameObject.activeSelf;

                    if (active)
                        healthMeter.FadeIn();

                    else
                        healthMeter.gameObject.SetActive(true);
                }
            }
        }

        public void HideHealthMeter()
        {
            if (attributes != null)
            {
                Meter healthMeter = attributes.health.meter;

                if (healthMeter != null)
                {       
                    healthMeter.FadeOut();
                }
            }
        }

        private void Reset()
        {
            gameObject.name = "Interaction Collider";

            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
        }
    }
}
