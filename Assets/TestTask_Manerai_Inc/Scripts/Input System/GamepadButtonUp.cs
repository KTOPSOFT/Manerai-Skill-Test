using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class GamepadButtonUp : MonoBehaviour
    {
        public static bool North()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.buttonNorth.wasReleasedThisFrame;

            return value;
        }
        
        public static bool East()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.buttonEast.wasReleasedThisFrame;

            return value;
        }


        public static bool South()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.buttonSouth.wasReleasedThisFrame;

            return value;
        }
        
        public static bool West()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.buttonWest.wasReleasedThisFrame;

            return value;
        }

        public static bool LeftShoulder()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.leftShoulder.wasReleasedThisFrame;

            return value;
        }

        public static bool RightShoulder()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.rightShoulder.wasReleasedThisFrame;

            return value;
        }

        public static bool LeftTrigger()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.leftTrigger.wasReleasedThisFrame;

            return value;
        }

        public static bool RightTrigger()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.rightTrigger.wasReleasedThisFrame;

            return value;
        }

        public static bool Start()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.startButton.wasReleasedThisFrame;

            return value;
        }

        public static bool Select()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.selectButton.wasReleasedThisFrame;

            return value;
        }

        public static bool DpadLeft()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.dpad.left.wasReleasedThisFrame;

            return value;
        }

        public static bool DpadRight()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.dpad.right.wasReleasedThisFrame;

            return value;
        }

        public static bool DpadUp()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.dpad.up.wasReleasedThisFrame;

            return value;
        }

        public static bool DpadDown()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.dpad.down.wasReleasedThisFrame;

            return value;
        }

        public static bool LeftStickButton()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.leftStickButton.wasReleasedThisFrame;

            return value;
        }

        public static bool RightStickButton()
        {
            bool value = false;

            if (Gamepad.current != null)
                value = Gamepad.current.rightStickButton.wasReleasedThisFrame;

            return value;
        }
    }
}









