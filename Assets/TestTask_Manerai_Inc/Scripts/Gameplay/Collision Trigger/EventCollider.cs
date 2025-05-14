using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class EventCollider : MonoBehaviour
    {
        public GameEvent onEnter;

        public GameEvent onExit;

        protected virtual void OnTriggerEnter(Collider other)
        {
            CollisionTrigger playerTrigger = other.GetComponent<CollisionTrigger>();

            if (playerTrigger != null)
            {
                PlayerController player = playerTrigger.GetPlayerController();

                if (player != null && player.GetActivePlayer())
                {
                    player.AddEventCollider(this);

                    onEnter.Invoke();
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            CollisionTrigger playerTrigger = other.GetComponent<CollisionTrigger>();

            if (playerTrigger != null)
            {
                PlayerController player = playerTrigger.GetPlayerController();

                if (player != null && player.GetActivePlayer())
                {
                    player.RemoveEventCollider(this);

                    onExit.Invoke();
                }
            }
        }
    }
}




