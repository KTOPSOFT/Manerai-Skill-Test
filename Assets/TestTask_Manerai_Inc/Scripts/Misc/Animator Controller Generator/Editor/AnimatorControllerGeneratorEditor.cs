#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace YukiOno.SkillTest
{
    [CustomEditor(typeof(AnimatorControllerGenerator))]
    class AnimatorControllerGeneratorEditor : Editor
    {
        private AnimatorControllerGenerator component;

        private void OnEnable()
        {
            component = (AnimatorControllerGenerator) target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if(GUILayout.Button("Create New Animator Controller"))
            {
                component.CreateAnimatorController();
            }
            
            if(GUILayout.Button("Create New Action State Recorder"))
            {
                component.CreateActionStateRecorder();
            }
        }
    }
}

#endif