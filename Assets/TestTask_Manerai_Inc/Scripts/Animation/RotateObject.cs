using UnityEngine;

using System.Collections;

namespace YukiOno.SkillTest
{
    public class RotateObject : MonoBehaviour
    {
        public Vector3 rotationSpeed;

        void FixedUpdate()
        {
            transform.Rotate(rotationSpeed * Time.fixedDeltaTime, Space.Self);
        }
    }
}