using UnityEngine;

using System.Collections;

namespace YukiOno.SkillTest
{
    public class AutoDisable : MonoBehaviour
    {
        public float timeToDisable = 1.0f;

        private float time;

        private void OnEnable()
        {
            time = 0f;
        }

        private void Update()
        {
            time += Time.deltaTime;

            if (time >= timeToDisable)
            {
                gameObject.SetActive(false);
            }
        }
    }
}




