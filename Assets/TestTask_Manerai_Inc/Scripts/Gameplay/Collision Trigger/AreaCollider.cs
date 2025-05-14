using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class AreaCollider : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            CollisionTrigger playerTrigger = other.GetComponent<CollisionTrigger>();

            if (playerTrigger != null)
            {
                PlayerController player = playerTrigger.GetPlayerController();

                if (player != null && player.GetActivePlayer())
                {
                    playerTrigger.SetNewArea(this);
                }
            }
        }
    }
}




