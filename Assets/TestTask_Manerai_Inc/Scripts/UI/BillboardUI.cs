using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class BillboardUI : MonoBehaviour
    {
        private Transform mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main.transform;

            gameObject.layer = 5;
        }

        private void OnEnable()
        {
            mainCamera = Camera.main.transform;
        }

        private void LateUpdate()
        {
            transform.LookAt(transform.position + mainCamera.forward);
        }
    }
}




