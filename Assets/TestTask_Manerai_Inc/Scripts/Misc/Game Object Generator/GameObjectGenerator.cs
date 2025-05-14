#if UNITY_EDITOR

using UnityEngine;

using UnityEditor;

using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

namespace YukiOno.SkillTest
{
    public class GameObjectGenerator : Editor
    {
        [MenuItem("GameObject/YukiOno.SkillTest/Player Controller")]
        public static void CreatePlayerController()
        {
            PlayerControllerGenerator.CreatePlayerController();

            MarkCurrentSceneDirty();
        }
        
        [MenuItem("GameObject/YukiOno.SkillTest/Item")]
        public static void CreateItem()
        {
            InteractableGenerator.CreateItem();
            
            MarkCurrentSceneDirty();
        }
        
        [MenuItem("GameObject/YukiOno.SkillTest/NPC")]
        public static void CreateNPC()
        {
            InteractableGenerator.CreateNPC();
            
            MarkCurrentSceneDirty();
        }

        [MenuItem("GameObject/YukiOno.SkillTest/Menu")]
        public static void CreateMenu()
        {
            MenuGenerator.CreateMenu();

            MarkCurrentSceneDirty();
        }

        public static GameObject CreateGameObject(string name, int layer)
        {
            GameObject newObject = new GameObject(name);

            newObject.layer = layer;

            return newObject;
        }

        public static void SetParent(GameObject childObject, GameObject parentObject)
        {
            Transform child = childObject.transform;

            child.SetParent(parentObject.transform);

            child.localPosition = Vector3.zero;
            child.localEulerAngles = Vector3.zero;

            child.localScale = Vector3.one;
        }

        public static void MarkCurrentSceneDirty()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            var activeScene = EditorSceneManager.GetActiveScene();

            if (prefabStage != null)
            {
                EditorSceneManager.MarkSceneDirty(prefabStage.scene);
            }

            else if (activeScene != null)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }

        }
    }
}

#endif







