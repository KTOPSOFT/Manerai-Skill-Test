using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;

        [Space(10)]

        public float cameraHeight = 1.5f;
        public float cameraDistance = 4.25f;

        [Space(10)]

        public float cameraRight = 0f;

        [Space(10)]

        [Range(0.01f, 0.5f)]
        public float nearClipPlane = 0.02f;

        public LayerMask collisionLayers;

        public Camera UI_Camera;

        // =========================================================

        private float currentHeight, heightSmooth;
        private float currentDistance, distanceSmooth;

        private float currentRight, rightSmooth;

        // =========================================================

        private Vector3 cameraPosition;
        private Quaternion cameraRotation;

        private float cameraSpeed = 1.0f;

        private static float mouseSensitivity = 2.25f;
        private static float gamepadSensitivity = 3.0f;

        private static float smoothRotation = 50.0f;

        // =========================================================

        private bool lockInput;

        private bool hideCursor;

        private float mouseX;
        private float mouseY;

        private bool lockMouseX;
        private bool lockMouseY;

        private float smoothRotationX;
        private float smoothRotationY;

        private Quaternion targetLookAtX;
        private Quaternion targetLookAtY;

        private float groundOffset;

        // =========================================================

        [Space(10)]

        [TextArea(4,4)]
        public string notes;

        // =========================================================

        [Space(30)]

        public float transitionTime = 0.15f;

        [Space(10)]

        public List<CameraState> cameraStates = new List<CameraState>();

        [System.Serializable]
        public class CameraState
        {
            public string name;

            [Space(10)]

            public float cameraHeight = 1.5f;
            public float cameraDistance = 4.25f;

            [Space(10)]

            public float cameraRight = 0f;

            public CameraState(string n, float h, float d, float r)
            {
                name = n;

                cameraHeight = h;
                cameraDistance = d;

                cameraRight = r;
            }
        }

        private float forcedTransitionTime = -1.0f;

        // =========================================================

        private bool useFixedUpdate;

        private float deltaTime;

        // =========================================================
        //    Component Dependencies
        // =========================================================

        private Camera m_camera;

        private SceneHandler sceneHandler;

        private InputManager inputManager;

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {
            GetComponents();

            UseCameraTarget();

            SetNearClipPlane(nearClipPlane);

            RevertSmoothRotation();

            // =========================================================

            currentHeight = cameraHeight;
            currentDistance = cameraDistance;

            currentRight = cameraRight;

            MoveCamera();
        }

        private void GetComponents()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            sceneHandler = elements.GetComponent<SceneHandler>();

            inputManager = elements.GetComponent<InputManager>();

            m_camera = transform.GetChild(0).GetComponent<Camera>();
        }

        private void Update()
        {
            CameraInput();
        }

        private void FixedUpdate()
        {
            if (useFixedUpdate)
            {
                deltaTime = Time.fixedDeltaTime;

                UpdateCamera();
            }
        }

        private void LateUpdate()
        {
            if (!useFixedUpdate)
            {
                deltaTime = Time.deltaTime;

                UpdateCamera();
            }
        }

        private void CameraInput()
        {
            var keyboard = Keyboard.current;

            if (keyboard.digit8Key.wasPressedThisFrame)
            {
                hideCursor = !hideCursor;
            }

            // =========================================================

            bool mouseInUse = inputManager.GetMouseInUse();

            if (mouseInUse)
            {
                var mouse = Mouse.current;

                float cameraSpeed = this.cameraSpeed * mouseSensitivity;

                float x = 0f;
                float y = 0f;

                if (!lockInput)
                {
                    Vector2 mouseDelta = mouse.delta.ReadValue() * 0.05f;

                    x = lockMouseX ? 0f : mouseDelta.x * cameraSpeed;
                    y = lockMouseY ? 0f : mouseDelta.y * cameraSpeed;
                }

                bool usingMenu = inputManager.GetUsingMenu();

                bool holdingLeftAlt = keyboard.leftAltKey.isPressed;
                bool holdingRightMouse = mouse.rightButton.isPressed;

                bool hidingCursor = hideCursor ? !holdingLeftAlt : holdingRightMouse;

                bool conditionA = !usingMenu && hidingCursor;
                bool conditionB = usingMenu && holdingRightMouse;

                if (conditionA || conditionB)
                {
                    RotateCamera(x, y);

                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }

                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }

            else if (Gamepad.current != null)
            {
                float cameraSpeed = this.cameraSpeed * gamepadSensitivity * Time.deltaTime;

                float x = 0f;
                float y = 0f;

                if (!lockInput)
                {
                    x = lockMouseX ? 0f : Gamepad.current.rightStick.x.ReadValue() * 60.0f * cameraSpeed;
                    y = lockMouseY ? 0f : Gamepad.current.rightStick.y.ReadValue() * 40.0f * cameraSpeed;
                }

                RotateCamera(x, y);
            }
        }

        private void UpdateCamera()
        {
            if (target != null)
            {
                UpdateValues();

                MoveCamera();

                transform.position = adjustedPosition + Vector3.up * groundOffset;

                transform.rotation = cameraRotation;
            }
        }

        private void UpdateValues()
        {
            float smoothTime = (forcedTransitionTime >= 0f) ? forcedTransitionTime : transitionTime;

            currentHeight = Mathf.SmoothDamp(currentHeight, cameraHeight, ref heightSmooth, smoothTime, Mathf.Infinity, deltaTime);
            currentDistance = Mathf.SmoothDamp(currentDistance, cameraDistance, ref distanceSmooth, smoothTime, Mathf.Infinity, deltaTime);

            currentRight = Mathf.SmoothDamp(currentRight, cameraRight, ref rightSmooth, smoothTime, Mathf.Infinity, deltaTime);
        }

        private void MoveCamera()
        {
            groundOffset = (mouseY < 0f) ? 0.25f : 0f;

            // =================================================

            Quaternion newRotation = Quaternion.Euler(mouseY, mouseX, 0f);

            targetLookAtX = Quaternion.Lerp(targetLookAtX, newRotation, smoothRotationY * Time.deltaTime);
            targetLookAtY = Quaternion.Lerp(targetLookAtY, newRotation, smoothRotationX * Time.deltaTime);

            float rotationX = targetLookAtX.eulerAngles.x;
            float rotationY = targetLookAtY.eulerAngles.y;

            cameraRotation = Quaternion.Euler(rotationX, rotationY, 0f);

            // =================================================

            Vector3 targetPosition = (target != null) ? target.position : Vector3.zero;

            targetPosition += Vector3.up * (currentHeight - groundOffset);

            // =================================================

            Vector3 backDirection = cameraRotation * Vector3.back;
            Vector3 rightDirection = cameraRotation * Vector3.right;

            Vector3 cameraOffset = (backDirection * currentDistance) + (rightDirection * currentRight);

            cameraPosition = targetPosition + cameraOffset;

            // =================================================

            UpdateCameraClipPoints();

            isColliding = CollisionDetected(clipPoints, targetPosition);

            adjustedDistance = isColliding ? GetAdjustedDistance(targetPosition) : 0f;

            adjustedPosition = isColliding ? targetPosition + cameraOffset.normalized * adjustedDistance : cameraPosition;
        }

        private void RotateCamera(float x, float y)
        {
            mouseX = GetAngle(mouseX + x);

            mouseY = GetAngleY(mouseY - y);
        }

        private float GetAngle(float angle)
        {
            while (angle < -360.0f || angle > 360.0f)
            {
                if (angle < -360.0f)
                    angle += 360.0f;

                else
                    angle -= 360.0f;
            }

            return angle;
        }

        private float GetAngleY(float angle)
        {
            float targetAngle = GetAngle(angle);

            float clampedAngle = Mathf.Clamp(targetAngle, -75.0f, 75.0f);

            return clampedAngle;
        }

        public void HideCursor(bool value) // called by PlayerManager.cs
        {
            hideCursor = value;
        }

        public void RevertSmoothRotation() // called by DialogueBox.cs
        {
            smoothRotationX = smoothRotation;

            smoothRotationY = smoothRotation;
        }

        public void LockInput(bool value) // called by PauseMenu.cs
        {
            lockInput = value;
        }

        public void LockMouseX(bool value)
        {
            lockMouseX = value;
        }

        public void LockMouseY(bool value) //called by NPC.cs and DialogueBox.cs
        {
            lockMouseY = value;
        }

        public void SetCameraRotation(float x, float y) // called by PlayerManager.cs
        {
            SetMouseX(x);
            SetMouseY(y);

            Quaternion newRotation = Quaternion.Euler(mouseY, mouseX, 0f);

            targetLookAtX = newRotation;
            targetLookAtY = newRotation;
        }

        private void UseCameraTarget()
        {
            if (sceneHandler != null)
            {
                CameraTarget cameraTarget = sceneHandler.cameraTarget;

                if (cameraTarget != null)
                {
                    target = cameraTarget.transform;
                }
            }
        }

        private void SetNearClipPlane(float value)
        {
            if (m_camera != null)
            {
                float farClipPlane = m_camera.farClipPlane - 0.01f;

                if (value > farClipPlane)
                    value = farClipPlane;

                nearClipPlane = value;

                m_camera.nearClipPlane = value;
            }
        }

        public void RevertCameraParent()
        {
            SetCameraParent(transform);

            lockInput = false;
        }

        public void SetCameraParent(Transform target)
        {
            Transform cameraTransform = m_camera.transform;

            cameraTransform.SetParent(target);

            cameraTransform.localPosition = Vector3.zero;
            cameraTransform.localRotation = Quaternion.identity;

            lockInput = target != transform;
        }

        public bool UsingCameraAngle()
        {       
            return m_camera.transform.parent != transform;
        }

        public void ForceTransitionTime(float value) // called by NPC.cs and DialogueBox.cs
        {
            forcedTransitionTime = value;
        }

        public void SetCameraState(int index) // called by NPC.cs
        {
            if (index >= 0)
            {
                int listCount = cameraStates.Count;

                if (index < listCount)
                {
                    CameraState targetState = cameraStates[index];

                    cameraHeight = targetState.cameraHeight;
                    cameraDistance = targetState.cameraDistance;

                    cameraRight = targetState.cameraRight;
                }
            }
        }
        
        private void Reset()
        {
            string[] layerNames = new string[2];

            layerNames[0] = "Ground";
            layerNames[1] = "Platform";

            collisionLayers = LayerMask.GetMask(layerNames);

            // =========================================================

            string noteA = "Please add the \"ThirdPersonCamera\" tag to this game object so that the Scene Handler can find and cache this component.";

            string noteB = "Otherwise, you will have to manually assign this component to the Scene Handler yourself.";

            notes = noteA + " " + noteB;

            // =========================================================

            cameraStates.Clear();

            CameraState stateA = new CameraState("Default", 1.5f, 4.25f, 0f);
            CameraState stateB = new CameraState("Off Center", 1.5f, 3.0f, 0.5f);

            cameraStates.Add(stateA);
            cameraStates.Add(stateB);
        }

        private void OnValidate()
        {
            SetNearClipPlane(nearClipPlane);
        }

        // =========================================================
        //    Camera Collision
        // =========================================================

        // camera collision solution by Renaissance Coders: https://www.youtube.com/watch?v=Uqi2jEgvVsI&ab_channel=RenaissanceCoders

        private bool isColliding;

        private Vector3[] clipPoints = new Vector3[5];

        private float[] rayDistances = new float[5];

        private float adjustedDistance;
        private Vector3 adjustedPosition;

        private void UpdateCameraClipPoints()
        {
            float z = m_camera.nearClipPlane;
            float x = Mathf.Tan(m_camera.fieldOfView / 2.0f) * z;
            float y = x / m_camera.aspect;

            // top left
            clipPoints[0] = (cameraRotation * new Vector3(-x, y, z)) + cameraPosition;

            // top right    
            clipPoints[1] = (cameraRotation * new Vector3(x, y, z)) + cameraPosition;

            // bottom left
            clipPoints[2] = (cameraRotation * new Vector3(-x, -y, z)) + cameraPosition;

            // bottom right        
            clipPoints[3] = (cameraRotation * new Vector3(x, -y, z)) + cameraPosition;

            // camera position
            clipPoints[4] = cameraPosition;
        }

        private bool CollisionDetected(Vector3[] clipPoints, Vector3 startPosition)
        {
            int arrayLength = clipPoints.Length;

            for (int i = 0; i < clipPoints.Length; i ++)
            {
                Ray ray = new Ray(startPosition, clipPoints[i] - startPosition);

                rayDistances[i] = Vector3.Distance(clipPoints[i], startPosition);

                if (Physics.Raycast(ray, rayDistances[i], collisionLayers))
                {
                    return true;
                }
            }

            return false;
        }

        private float GetAdjustedDistance(Vector3 startPosition)
        {
            float distance = -1.0f;

            // =========================================================

            int arrayLength = clipPoints.Length;

            for (int i = 0; i < arrayLength; i ++)
            {
                RaycastHit hit;

                Ray ray = new Ray(startPosition, clipPoints[i] - startPosition);

                if (Physics.Raycast(ray, out hit, rayDistances[i], collisionLayers))
                {
                    float hitDistance = hit.distance;

                    if (distance < 0f)
                    {
                        distance = hitDistance;
                    }

                    else if (hitDistance < distance)
                    {
                        distance = hitDistance;
                    }
                }
            }

            // =========================================================

            if (distance < 0f)
            {
                distance = 0f;
            }

            return distance;
        }

        public void UseFixedUpdate(bool value) // called by PlayerController.cs
        {
            useFixedUpdate = value;
        }

        // =========================================================
        //    Set Methods
        // =========================================================

        public void SetSmoothRotationX(float value)
        {
            if (!lockMouseX)
            {
                smoothRotationX = value;
            }
        }

        public void SetSmoothRotationY(float value)
        {
            if (!lockMouseY)
            {
                smoothRotationY = value;
            }
        }

        public void SetMouseX(float value)
        {
            mouseX = GetAngle(value);
        }

        public void SetMouseY(float value)
        {
            mouseY = GetAngleY(value);
        }

        public void SetCameraSpeed(float value)
        {
            cameraSpeed = value;
        }
    }
}









