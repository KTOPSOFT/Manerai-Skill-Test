using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(SceneHandler))]

    public class InputManager : MonoBehaviour
    {
        [SerializeField]
        private bool mouseInUse = true;

        private bool usingMenu;

        private int controlType;

        private int confirmButton;

        // =========================================================

        private List<ButtonPrompts> m_buttonPrompts = new List<ButtonPrompts>();

        public List<ButtonPrompts> buttonPrompts { get { return m_buttonPrompts; } }

        // =========================================================

        private SceneHandler sceneHandler;

        private PlayerManager playerManager;

        private MenuManager menuManager;

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {
            sceneHandler = GetComponent<SceneHandler>();

            playerManager = GetComponent<PlayerManager>();

            menuManager = GetComponent<MenuManager>();

            // =========================================================

            m_buttonPrompts.Clear();
        }

        private void Update()
        {
            CheckInputType();
        }

        private void CheckInputType()
        {
            if (mouseInUse)
            {
                if (ControllerInput())
                {
                    SwitchToGamepad();
                }
            }

            else
            {
                if (MouseInput())
                {
                    mouseInUse = true;

                    controlType = 0;

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    ControlChange();
                }
            }
        }

        private void SwitchToGamepad()
        {
            mouseInUse = false;

            if (Gamepad.current.ToString().Contains("XInput"))
            {
                controlType = 1;
            }

            if (Gamepad.current.ToString().Contains("DualShock"))
            {
                controlType = 2;
            }

            // ====================================================

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            ControlChange();
        }

        private void ControlChange() // call all methods that rely on control change
        {
            Menu dialogueOptions = menuManager.dialogueOptions;

            if (dialogueOptions != null)
            {
                menuManager.ResetMenu(dialogueOptions);
            }

            // ====================================================

            PlayerInteraction interaction = playerManager.playerInteraction;

            if (interaction != null)
            {
                interaction.ResetActionPrompt(controlType, false);
            }
            
            // ====================================================
            
            int listCount = m_buttonPrompts.Count;
            
            for (int i = 0; i < listCount; i ++)
            {
                m_buttonPrompts[i].SwitchType(controlType);
            }
        }

        public bool MouseInput()
        {
            var mouse = Mouse.current;

            // ====================================================

            bool mouseMoved = false;

            bool mouseInput = false;

            // ====================================================

            mouseMoved = mouse.delta.ReadValue() != Vector2.zero;

            mouseInput = mouseMoved || mouse.leftButton.isPressed || mouse.rightButton.isPressed;

            // ====================================================

            return mouseInput;
        }

        private bool ControllerInput()
        {
            bool button = false;

            bool stick = false;
            bool dpad = false;
            bool trigger = false;

            Gamepad current = Gamepad.current;

            if (current != null)
            {
                button = GamepadButtonDown.AnyButton();

                stick = current.leftStick.ReadValue() != Vector2.zero || current.rightStick.ReadValue() != Vector2.zero;;
                dpad = current.dpad.ReadValue() != Vector2.zero;
                trigger = current.leftTrigger.isPressed || current.rightTrigger.isPressed;
            }

            return button || stick || dpad || trigger;
        }

        public bool ConfirmButtonDown()
        {
            bool confirmButtonDown = (confirmButton == 0 && GamepadButtonDown.South()) || (confirmButton == 1 && GamepadButtonDown.East());

            return confirmButtonDown;
        }

        public bool CancelButtonDown()
        {
            bool cancelButtonDown = (confirmButton == 0 && GamepadButtonDown.East()) || (confirmButton == 1 && GamepadButtonDown.South());

            return cancelButtonDown;
        }

        // =============================================================
        //    Get Methods
        // =============================================================

        public bool GetMouseInUse()
        {
            return mouseInUse;
        }

        public bool GetUsingMenu()
        {
            return usingMenu;
        }

        public int GetControlType()
        {
            return controlType;
        }

        // =============================================================
        //    Get Methods
        // =============================================================

        public void SetUsingMenu(bool value) // called by MenuManager.cs
        {
            usingMenu = value;
        }
    }
}




