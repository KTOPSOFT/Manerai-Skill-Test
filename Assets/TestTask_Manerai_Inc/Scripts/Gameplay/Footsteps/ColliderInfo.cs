using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace YukiOno.SkillTest
{
    public class ColliderInfo : MonoBehaviour
    {
        public bool isTerrain;

        public int[] materialTypes;

        private Terrain terrain;

        private void Start()
        {
            if (isTerrain)
            {
                terrain = GetComponent<Terrain>();

                isTerrain = terrain != null;
            }
        }

        private void Reset()
        {
            materialTypes = new int[1];

            materialTypes[0] = -1;
        }

        public Terrain GetTerrain() // called by Footsteps.cs
        {
            return terrain;
        }
    }
}









