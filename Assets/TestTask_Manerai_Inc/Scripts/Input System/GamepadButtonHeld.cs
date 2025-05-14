using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class GamepadButtonHeld : MonoBehaviour
    {
        public static bool North()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.buttonNorth.isPressed;

            return value;
        }
        
        public static bool East()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.buttonEast.isPressed;

            return value;
        }


        public static bool South()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.buttonSouth.isPressed;

            return value;
        }
        
        public static bool West()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.buttonWest.isPressed;

            return value;
        }
        
        public static bool LeftShoulder()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.leftShoulder.isPressed;

            return value;
        }

        public static bool RightShoulder()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.rightShoulder.isPressed;

            return value;
        }

        public static bool LeftTrigger()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.leftTrigger.isPressed;

            return value;
        }

        public static bool RightTrigger()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.rightTrigger.isPressed;

            return value;
        }

        public static bool Start()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.startButton.isPressed;

            return value;
        }

        public static bool Select()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.selectButton.isPressed;

            return value;
        }

        public static bool DpadLeft()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.dpad.left.isPressed;

            return value;
        }

        public static bool DpadRight()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.dpad.right.isPressed;

            return value;
        }

        public static bool DpadUp()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.dpad.up.isPressed;

            return value;
        }

        public static bool DpadDown()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.dpad.down.isPressed;

            return value;
        }

        public static bool LeftStickButton()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.leftStickButton.isPressed;

            return value;
        }

        public static bool RightStickButton()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.rightStickButton.isPressed;

            return value;
        }
    }
}










