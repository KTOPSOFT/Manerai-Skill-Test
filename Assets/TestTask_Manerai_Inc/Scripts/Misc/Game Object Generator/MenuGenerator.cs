#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;
using UnityEngine.EventSystems;

using UnityEditor;
using UnityEditor.Events;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class MenuGenerator
    {
        public static void CreateMenu()
        {
            Menu menu = NewRectObject("Menu").AddComponent<Menu>();

            menu.rows = 1;
            menu.columns = 2;

            // =========================================================

            Transform selection = Selection.activeTransform;

            if (selection != null)
            {
                SetParent(menu.gameObject, selection.gameObject);
            }

            // =========================================================

            GameObject menuWindow = NewRectObject("Menu Window");

            SetParent(menuWindow, menu.gameObject);

            // =========================================================

            GameObject background = NewRectObject("Background");

            SetParent(background, menuWindow);

            CreateBackground(background);

            // =========================================================

            GameObject buttons = NewRectObject("Buttons");

            SetParent(buttons, menuWindow);

            CreateButton("Button", buttons, new Vector2(-90.0f, -58.0f), menu);

            CreateButton("Button (1)", buttons, new Vector2(90.0f, -58.0f), menu);

            // =========================================================

            GameObject m_interface = NewRectObject("Interface");

            SetParent(m_interface, menuWindow);
        }

        private static void CreateBackground(GameObject parentObject)
        {
            GameObject bg = NewRectObject("Image");

            SetParent(bg, parentObject);

            // =========================================================

            RectTransform rect = bg.GetComponent<RectTransform>();

            rect.sizeDelta = new Vector2(480.0f, 260.0f);

            // =========================================================

            Image image = bg.AddComponent<Image>();

            image.color = new Color(0f, 0f, 0f, 0.9f);
        }

        private static void CreateButton(string name, GameObject parentObject, Vector3 localPosition, Menu menu)
        {
            GameObject button = NewRectObject(name);

            button.AddComponent<Image>();
            button.AddComponent<Button>();
            button.AddComponent<EventTrigger>();

            SetParent(button, parentObject);

            // =========================================================

            Button m_button = button.GetComponent<Button>();

            m_button.transition = Selectable.Transition.None;

            // =========================================================

            Navigation navigation = Navigation.defaultNavigation;

            navigation.mode = Navigation.Mode.None;

            m_button.navigation = navigation;

            // =========================================================

            EventTrigger eventTrigger = button.GetComponent<EventTrigger>();

            EventTriggerType pointerEnter = EventTriggerType.PointerEnter;

            AddEventTriggerListener(eventTrigger, pointerEnter, menu, button.transform);
            
            // =========================================================

            RectTransform rect = button.GetComponent<RectTransform>();

            rect.localPosition = localPosition;

            rect.sizeDelta = new Vector2(134.0f, 38.0f);

            // =========================================================

            button.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0f);

            button.GetComponent<CanvasRenderer>().cullTransparentMesh = false;

            // =========================================================
            
            Color colorA = new Color(1.0f, 1.0f, 1.0f, 0.05f);
            
            CreateImage("Background", button, colorA, true);
            
            // =========================================================
            
            Color colorB = new Color(1.0f, 1.0f, 1.0f, 0.25f);
            
            CreateImage("Highlight", button, colorB, false);
        }

        private static void CreateImage(string name, GameObject parentObject, Color color, bool active)
        {
            GameObject imageObject = NewRectObject(name);

            imageObject.AddComponent<Image>();

            SetParent(imageObject, parentObject);

            // =========================================================

            imageObject.GetComponent<RectTransform>().sizeDelta = new Vector2(134.0f, 38.0f);

            imageObject.GetComponent<CanvasRenderer>().cullTransparentMesh = false;

            // =========================================================
            
            Image image = imageObject.GetComponent<Image>();
            
            image.color = color;
            
            image.raycastTarget = false;
            
            // =========================================================

            imageObject.SetActive(active);
        }

        private static void AddEventTriggerListener(EventTrigger eventTrigger, EventTriggerType eventType, Menu menu, Transform button)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();

            entry.eventID = eventType;

            eventTrigger.triggers.Add(entry);

            // =========================================================

            UnityAction<Transform> action = new UnityAction<Transform>(menu.OnPointerEnter);

            UnityEventTools.AddObjectPersistentListener<Transform>(entry.callback, action, button);
        }

        private static GameObject NewRectObject(string name)
        {
            GameObject newObject = new GameObject(name);

            newObject.AddComponent<RectTransform>();

            return newObject;
        }

        private static void SetParent(GameObject childObject, GameObject parentObject)
        {
            GameObjectGenerator.SetParent(childObject, parentObject);
        }
    }
}

#endif







