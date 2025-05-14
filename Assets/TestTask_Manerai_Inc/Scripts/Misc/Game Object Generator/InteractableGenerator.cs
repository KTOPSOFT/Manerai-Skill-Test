#if UNITY_EDITOR

    using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;

using UnityEditor;
using UnityEditor.Events;

using System.Collections;
using System.Collections.Generic;

using TMPro;

namespace YukiOno.SkillTest
{
    public class InteractableGenerator
    {
        public static void CreateItem()
        {
            int layer = LayerMask.NameToLayer("Item");

            GameObject itemObject = CreateGameObject("New Item", layer);

            // =========================================================

            Transform selection = Selection.activeTransform;

            if (selection != null)
            {
                SetParent(itemObject, selection.gameObject);
            }

            itemObject.transform.localPosition = Vector3.up * 0.2f;

            // =========================================================

            Item item = itemObject.AddComponent<Item>();

            item.m_name = "Item";

            // =========================================================

            Vector3 localPosition = Vector3.up * 0.38f;

            CreateSphere(itemObject, layer);

            CreateCanvasTarget(item, layer, localPosition);
        }

        public static void CreateNPC()
        {
            int layer = LayerMask.NameToLayer("NPC");

            GameObject npcObject = CreateGameObject("New NPC", layer);

            // =========================================================

            Transform selection = Selection.activeTransform;

            if (selection != null)
            {
                SetParent(npcObject, selection.gameObject);
            }

            // =========================================================

            NPC npc = npcObject.AddComponent<NPC>();

            npc.m_name = "NPC";

            // =========================================================

            Vector3 localPosition = Vector3.up * 1.98f;

            CreateCube(npcObject, layer);

            CreateCanvasTarget(npc, layer, localPosition);

            CreateDialogueSets(npc, layer);
        }

        private static void CreateSphere(GameObject interactable, int layer)
        {
            GameObject mesh = CreateGameObject("Mesh", layer);

            SetParent(mesh, interactable);

            // =========================================================

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            sphere.layer = layer;

            SetParent(sphere, mesh);

            sphere.transform.localScale = Vector3.one * 0.4f;

            // =========================================================

            SphereCollider sphereCollider = sphere.GetComponent<SphereCollider>();

            UnityEditorInternal.ComponentUtility.MoveComponentDown(sphereCollider);
        }

        private static void CreateCube(GameObject interactable, int layer)
        {
            GameObject mesh = CreateGameObject("Mesh", layer);

            SetParent(mesh, interactable);

            // =========================================================

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            cube.layer = layer;

            SetParent(cube, mesh);

            cube.transform.localPosition = Vector3.up * 0.9f;

            cube.transform.localScale = new Vector3(0.4f, 1.8f, 0.4f);

            // =========================================================

            BoxCollider boxCollider = cube.GetComponent<BoxCollider>();

            UnityEditorInternal.ComponentUtility.MoveComponentDown(boxCollider);
        }

        private static void CreateCanvasTarget(Interactable interactable, int layer, Vector3 localPosition)
        {
            GameObject canvasTarget = CreateGameObject("Canvas Target", layer);

            SetParent(canvasTarget, interactable.gameObject);

            // =========================================================

            WorldToScreenUI worldToScreenUI = canvasTarget.AddComponent<WorldToScreenUI>();

            UnityEditorInternal.ComponentUtility.MoveComponentUp(worldToScreenUI);

            // =========================================================

            CanvasGroup canvasGroup = canvasTarget.GetComponent<CanvasGroup>();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // =========================================================

            canvasTarget.SetActive(false);

            interactable.canvas = canvasTarget;

            // =========================================================

            canvasTarget.transform.localPosition = localPosition;

            CreateCanvas(interactable, canvasTarget, layer);
        }

        private static void CreateCanvas(Interactable interactable, GameObject canvasTarget, int layer)
        {
            GameObject canvasObject = CreateGameObject("Canvas", layer);

            SetParent(canvasObject, canvasTarget);

            // =========================================================

            Canvas canvas = canvasObject.AddComponent<Canvas>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            // =========================================================

            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            canvasScaler.referenceResolution = new Vector2(1920.0f, 1080.0f);
            canvasScaler.referencePixelsPerUnit = 1.0f;

            // =========================================================

            RectTransform rectTransform = canvasObject.GetComponent<RectTransform>();

            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.pivot = Vector2.zero;

            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.zero;

            // =========================================================

            CreateCanvasFadeIn(interactable, canvasObject, layer);
        }

        private static void CreateCanvasFadeIn(Interactable interactable, GameObject canvas, int layer)
        {
            GameObject newObject = CreateGameObject("Name", layer);

            newObject.AddComponent<RectTransform>();

            SetParent(newObject, canvas);

            // =========================================================

            CanvasFadeIn canvasFadeIn = newObject.AddComponent<CanvasFadeIn>();

            // =========================================================

            CanvasGroup canvasGroup = newObject.GetComponent<CanvasGroup>();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // =========================================================

            GameEvent gameEvent = canvasFadeIn.onCanvasHidden;

            GameObject canvasTarget = newObject.transform.parent.parent.gameObject;

            SetUnityAction(gameEvent, canvasTarget);

            // =========================================================

            CreateText(interactable, newObject, layer);
        }

        private static void CreateText(Interactable interactable, GameObject canvasFadeIn, int layer)
        {
            GameObject textObject = CreateGameObject("Text", layer);

            SetParent(textObject, canvasFadeIn);

            // =========================================================

            TextMeshProUGUI text =  textObject.AddComponent<TextMeshProUGUI>();

            text.fontSize = 21.0f;

            text.alignment = TextAlignmentOptions.Center;

            text.SetText(interactable.m_name);

            // =========================================================

            SetFontAsset(text);

            textObject.AddComponent<SmoothTransform>();

            interactable.nameText = text;
        }

        private static void SetUnityAction(GameEvent gameEvent, GameObject canvasTarget)
        {
            UnityAction<bool> action = new UnityAction<bool>(canvasTarget.SetActive);

            UnityEventTools.AddBoolPersistentListener(gameEvent, action, false);
        }

        private static void SetFontAsset(TextMeshProUGUI text)
        {
            string[] fontAssets = AssetDatabase.FindAssets("Manrope SDF t:TMP_FontAsset");

            if (fontAssets.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(fontAssets[0]);

                TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TMP_FontAsset)) as TMP_FontAsset;

                text.font = fontAsset;
            }

            // =========================================================

            string[] fontMaterials = AssetDatabase.FindAssets("Manrope SDF - Underlay t:Material");

            if (fontMaterials.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(fontMaterials[0]);

                Material fontMaterial = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Material)) as Material;

                text.fontSharedMaterial = fontMaterial;
            }
        }

        private static void CreateDialogueSets(NPC npc, int layer)
        {
            GameObject dialogueSets = CreateGameObject("Dialogue Sets", layer);

            SetParent(dialogueSets, npc.gameObject);

            // =========================================================

            GameObject introduction = CreateGameObject("Introduction", layer);

            SetParent(introduction, dialogueSets);

            // =========================================================

            DialogueSet dialogueSet = introduction.AddComponent<DialogueSet>();

            string lineA = "This is a sample dialogue set. You can create|line breaks by using a vertical slash between lines.";
            string lineB = "You can also {highlight} words and phrases by using {braces}.|View the {Dialogue Set} component to see how this is done.";

            dialogueSet.dialogue.Add(lineA);
            dialogueSet.dialogue.Add(lineB);

            // =========================================================

            npc.currentDialogueSet = dialogueSet;
        }

        private static GameObject CreateGameObject(string name, int layer)
        {
            GameObject newObject = GameObjectGenerator.CreateGameObject(name, layer);

            return newObject;
        }

        private static void SetParent(GameObject childObject, GameObject parentObject)
        {
            GameObjectGenerator.SetParent(childObject, parentObject);
        }
    }
}

#endif







