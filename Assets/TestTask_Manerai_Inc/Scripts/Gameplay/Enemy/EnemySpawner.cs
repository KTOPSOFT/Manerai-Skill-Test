using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class EnemySpawner : MonoBehaviour
    {
        public Transform spawnPoints;

        [Space(10)]

        public List<GameObject> enemyPrefabs = new List<GameObject>();

        [Space(10)]

        public List<AudioListGroup> audioListGroups = new List<AudioListGroup>();

        private List<Enemy> instances = new List<Enemy>();

        private void Awake()
        {
            instances.Clear();

            // =========================================================

            int childCount = spawnPoints.childCount;

            for (int i = 0; i < childCount; i ++)
            {
                Transform spawnGroup = spawnPoints.GetChild(i);

                EnemyGroup enemyGroup = spawnGroup.GetComponent<EnemyGroup>();

                int arrayLength = enemyGroup.enemyTypes.Length;

                if (enemyGroup != null)
                {
                    int spawnCount = spawnGroup.childCount;

                    for (int j = 0; j < spawnCount; j ++)
                    {
                        if (j < arrayLength)
                        {
                            Transform spawnPoint = spawnGroup.GetChild(j);

                            Vector3 position = spawnPoint.position;

                            Quaternion rotation = spawnPoint.rotation;

                            // =========================================================

                            int enemyType = enemyGroup.enemyTypes[j];

                            GameObject newObject = GameObject.Instantiate(enemyPrefabs[enemyType], position, rotation, transform);

                            spawnPoint.gameObject.SetActive(false);

                            // =========================================================

                            Enemy enemy = newObject.GetComponent<Enemy>();

                            enemy.Initialize(audioListGroups[enemyType], spawnPoint);

                            instances.Add(enemy);
                        }
                    }
                }
            }
        }

        public void ResetValues()
        {
            int listCount = instances.Count;

            for (int i = 0; i < listCount; i ++)
            {
                instances[i].ResetValues();
            }
        }
    }
}




