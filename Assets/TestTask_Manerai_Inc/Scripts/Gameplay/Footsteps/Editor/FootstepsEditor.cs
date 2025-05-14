#if UNITY_EDITOR

    using UnityEngine;

using UnityEditor;

using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [CustomEditor(typeof(Footsteps))]

    class FootstepsEditor : Editor
    {
        private Footsteps component;

        private void OnEnable()
        {
            component = (Footsteps) target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if(GUILayout.Button("Find Foot Transforms"))
            {
                FindTransforms();
            }
        }

        private void FindTransforms()
        {
            Transform armature = component.armature;

            if (armature != null)
            {
                Transform leftFoot = null;

                Transform rightFoot = null;

                // =========================================================

                List<Transform> transforms = new List<Transform>();

                TraverseHierarchy(armature, ref transforms, "foot");

                // =========================================================

                int listCount = transforms.Count;

                for (int i = 0; i < listCount; i ++)
                {
                    string name = transforms[i].gameObject.name;

                    if (name.Contains("Left") || name.Contains("left"))
                    {
                        leftFoot = transforms[i];
                    }

                    else if (name.Contains("Right") || name.Contains("right"))
                    {
                        rightFoot = transforms[i];
                    }
                }

                // =========================================================

                if (leftFoot == null)
                {
                    for (int i = 0; i < listCount; i ++)
                    {
                        string name = transforms[i].gameObject.name;
                        
                        if (name.Contains("L") || name.Contains("l"))
                        {
                            leftFoot = transforms[i];
                        }
                    }
                }

                if (rightFoot == null)
                {
                    for (int i = 0; i < listCount; i ++)
                    {
                        string name = transforms[i].gameObject.name;
                        
                        if (name.Contains("R") || name.Contains("r"))
                        {
                            rightFoot = transforms[i];
                        }
                    }
                }

                // =========================================================

                component.leftFoot = leftFoot;

                component.rightFoot = rightFoot;

                // =========================================================

                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

                if (prefabStage != null)
                {
                    EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                }
            }

            else
            {
                Debug.Log("Could not find foot transforms as the <color=#80E7FF>Armature</color> transform has not been set.");
            }
        }

        private void TraverseHierarchy(Transform root, ref List<Transform> transforms, string searchTag)
        {
            foreach (Transform child in root)
            {
                string name = child.gameObject.name;

                if (name.Contains("foot") || name.Contains("Foot"))
                {
                    transforms.Add(child);
                }

                TraverseHierarchy(child, ref transforms, searchTag);
            }
        }
    }
}

#endif