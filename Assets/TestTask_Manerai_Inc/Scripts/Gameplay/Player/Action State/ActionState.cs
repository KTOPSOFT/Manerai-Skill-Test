using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class ActionState : MonoBehaviour
    {
        [Range(0, 1)]
        public float startTime;

        public int listCount;

        public int listCountOverride = -1;

        public List<float> distanceFromOrigin = new List<float>();
        public List<float> moveSpeed = new List<float>();

        public float moveDirection = 0f;
        public float moveMultiplier = 1.0f;

        public float timeInterval = 0.01f;
        
        public List<GameEvent> actionEvents = new List<GameEvent>();

        public void ConvertList() // called by ActionStateEditor.cs
        {
            distanceFromOrigin.Clear();

            if (moveSpeed.Count > 0)
            {
                distanceFromOrigin.Add(moveSpeed[0]);

                // =============================================

                int listCount = moveSpeed.Count;

                for (int i = 1; i < listCount; i ++)
                {
                    float currentDistance = distanceFromOrigin[i - 1];

                    distanceFromOrigin.Add(currentDistance + moveSpeed[i]);
                }
            }
        }
    }
}









