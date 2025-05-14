using UnityEngine;

namespace YukiOno.SkillTest
{
    public class CameraShakeProperties : MonoBehaviour
    {
        public PlayerController playerController;

        public float magnitude = 1.0f;

        public int cycles = 1;

        public bool isLocal = true;

        public void Shake()
        {
            if (playerController != null)
            {
                bool activePlayer = playerController.GetActivePlayer();

                CameraShake.Shake(magnitude, cycles, isLocal, activePlayer, playerController.transform.position);
            }
        }
    }
}



