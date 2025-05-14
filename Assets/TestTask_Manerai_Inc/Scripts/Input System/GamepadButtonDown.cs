using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class GamepadButtonDown : MonoBehaviour
    {
        public static bool North()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.buttonNorth.wasPressedThisFrame;

            return value;
        }
        
        public static bool East()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.buttonEast.wasPressedThisFrame;

            return value;
        }


        public static bool South()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.buttonSouth.wasPressedThisFrame;

            return value;
        }
        
        public static bool West()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.buttonWest.wasPressedThisFrame;

            return value;
        }

        public static bool LeftShoulder()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.leftShoulder.wasPressedThisFrame;

            return value;
        }

        public static bool RightShoulder()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.rightShoulder.wasPressedThisFrame;

            return value;
        }

        public static bool LeftTrigger()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.leftTrigger.wasPressedThisFrame;

            return value;
        }

        public static bool RightTrigger()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.rightTrigger.wasPressedThisFrame;

            return value;
        }

        public static bool Start()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.startButton.wasPressedThisFrame;

            return value;
        }

        public static bool Select()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.selectButton.wasPressedThisFrame;

            return value;
        }

        public static bool DpadLeft()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.dpad.left.wasPressedThisFrame;

            return value;
        }

        public static bool DpadRight()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.dpad.right.wasPressedThisFrame;

            return value;
        }

        public static bool DpadUp()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.dpad.up.wasPressedThisFrame;

            return value;
        }

        public static bool DpadDown()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.dpad.down.wasPressedThisFrame;

            return value;
        }

        public static bool LeftStickButton()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.leftStickButton.wasPressedThisFrame;

            return value;
        }

        public static bool RightStickButton()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.rightStickButton.wasPressedThisFrame;

            return value;
        }

        public static bool AnyButton()
        {
            bool value = false;

            Gamepad current = Gamepad.current;

            if (current != null)
            {
                bool north = current.buttonNorth.wasPressedThisFrame;
                bool east = current.buttonEast.wasPressedThisFrame;
                bool south = current.buttonSouth.wasPressedThisFrame;
                bool west = current.buttonWest.wasPressedThisFrame;

                bool start = current.startButton.wasPressedThisFrame;
                bool select = current.selectButton.wasPressedThisFrame;

                bool leftStick = current.leftStickButton.wasPressedThisFrame;
                bool rightStick = current.rightStickButton.wasPressedThisFrame;

                bool leftShoulder = current.leftShoulder.wasPressedThisFrame;
                bool rightShoulder = current.rightShoulder.wasPressedThisFrame;

                // ==========================================

                bool faceButton = north || east || south || west;
                bool systemButton = start || select;
                bool stickButton = leftStick || rightStick;
                bool shoulder = leftShoulder || rightShoulder;

                if (faceButton || systemButton || stickButton || shoulder)
                {
                    value = true;
                }
            }

            return value;
        }
    }
}









