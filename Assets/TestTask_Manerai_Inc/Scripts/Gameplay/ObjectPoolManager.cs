using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public Transform hitParticles;

        public Transform damageIndicators;

        private void Awake()
        {
            CheckDamageIndicators();
        }

        public void ClearPool(Transform objectPool)
        {
            int childCount = objectPool.childCount;

            for (int i = 1; i < childCount; i ++)
            {
                Destroy(objectPool.GetChild(i).gameObject);
            }
        }

        public void ClearAllPools()
        {
            ClearPool(damageIndicators);

            // =========================================================

            int listCount = hitParticles.childCount;

            for (int i = 0; i < listCount; i ++)
            {
                ClearPool(hitParticles.GetChild(i));
            }
        }

        public static Transform GetFromPool(Transform objectPool, Vector3 position)
        {
            Transform objectTransform = null;

            int childCount = objectPool.childCount;

            if (childCount > 0)
            {
                Transform originalObject = objectPool.GetChild(0);

                bool originalEnabled = originalObject.gameObject.activeSelf;

                if (childCount > 1)
                {
                    Transform currentObject = objectPool.GetChild(1);

                    // if (currentObject.gameObject.activeSelf)

                    bool newInstance = (originalEnabled && currentObject.position.y > -900.0f) || (!originalEnabled && currentObject.gameObject.activeSelf);

                    if (newInstance)
                    {
                        objectTransform = AddToPool(originalObject, objectPool, position);
                    }

                    else
                    {
                        objectTransform = currentObject;

                        objectTransform.position = position;

                        objectTransform.SetAsLastSibling();
                    }
                }

                else
                {
                    objectTransform = AddToPool(originalObject, objectPool, position);
                }
            }

            return objectTransform;
        }

        private static Transform AddToPool(Transform originalObject, Transform objectPool, Vector3 position)
        {
            Transform objectTransform = Instantiate(originalObject, position, Quaternion.identity);

            objectTransform.gameObject.name = originalObject.gameObject.name;

            objectTransform.SetParent(objectPool);

            return objectTransform;
        }

        private void CheckDamageIndicators()
        {
            int childCount = damageIndicators.childCount;

            if (childCount < 3)
            {
                string logA = "The <color=#80E7FF>Damage Indicators</color> game object has less than three object pools.";
                string logB = "Go to the <color=#80E7FF>Version Changes</color> folder and view the <color=#80E7FF>Version 1.4 - Damage Indicators</color> guide to see how to set up the <color=#80E7FF>Damage Indicators</color> game object.";

                Debug.Log(logA + " " + logB);

            }
        }
    }
}









