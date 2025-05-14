using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class EnemyGroup : MonoBehaviour
    {
        [Tooltip("The enemy prefab to be spawned for each child transform. Determined by the enemy prefab index from the Enemy Spawner component.")]
        public int[] enemyTypes = new int[0];
    }
}



