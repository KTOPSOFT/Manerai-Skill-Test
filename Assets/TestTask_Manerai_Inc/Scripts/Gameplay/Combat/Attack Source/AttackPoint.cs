using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class AttackPoint : MonoBehaviour
    {
        public float radius;

        [HideInInspector]
        public List<Vector3> previousPositions = new List<Vector3>();
    }
}




