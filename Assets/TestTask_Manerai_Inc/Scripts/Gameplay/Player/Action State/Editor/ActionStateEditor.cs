#if UNITY_EDITOR

using UnityEngine;

using UnityEditor;

using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

namespace YukiOno.SkillTest
{
    [CustomEditor(typeof(ActionState))]
    class ActionStateEditor : Editor
    {
        private ActionState component;

        private void OnEnable()
        {
            component = (ActionState) target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if(GUILayout.Button("Convert Move Speed To Distance"))
            {
                component.ConvertList();

                // ============================================

                var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

                if (prefabStage != null)
                {
                    EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                }
            }
        }
    }
}

#endif