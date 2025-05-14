using UnityEngine;
using UnityEngine.Scripting;

using System;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(InputManager))]

    [RequireComponent(typeof(PlayerManager))]

    [RequireComponent(typeof(MenuManager))]

    [RequireComponent(typeof(AudioManager))]

    public class SceneHandler : MonoBehaviour
    {
        public CustomNetworkManager networkManager;

        public ThirdPersonCamera thirdPersonCamera;

        public CameraTarget cameraTarget;

        public PauseMenu pauseMenu;
        public DialogueBox dialogueBox;

        public ScreenFade screenFade;
        public HelpText helpText;

        public Transform objectPools;

        public MessageLists messageLists = new MessageLists();

        public AudioLists audioLists = new AudioLists();

        public AudioFade musicPlayer;

        // =========================================================

        [Space(10)]

        public LayerMask groundLayerMask;

        [Tooltip("Layers that cause the player to switch from <CharacterController> to <RigidBody> upon collision.")]
        public LayerMask physicsLayerMask;

        [Tooltip("Layers that force the character controller's step offset to become zero upon collision. Useful for preventing characters from stepping over small props.")]
        public LayerMask stepPrevention;

        // =========================================================

        private Transform freeTransform;

        private int playerLayer;
        private int enemyLayer;
        private int interactionLayer;
        private int targetLayer;
        private int platformLayer;
        private int collisionTriggerLayer;

        // =========================================================

        [Space(10)]

        [Tooltip("A list of canvas groups that will be hidden when a menu or dialogue box is open.")]
        public List<CanvasGroup> canvasGroups = new List<CanvasGroup>();

        public List<Color32> highlightColors = new List<Color32>();

        private List<string> hexValues = new List<string>();

        private Camera UI_Camera;

        private LayerMask UI_LayerMask;

        private AudioManager m_audioManager;

        public AudioManager audioManager { get { return m_audioManager; } }

        public DeveloperOptions developerOptions;

        // =========================================================

        [Space(10)]

        [TextArea(4,4)]
        public string notes;

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {   
            InitializeLayers();

            InitializeColors();

            CreateFreeTransforms();

            audioLists.Initialize();

            m_audioManager = GetComponent<AudioManager>();

            // =========================================================

            thirdPersonCamera = GameObject.FindWithTag("ThirdPersonCamera").GetComponent<ThirdPersonCamera>();

            Camera UI_Camera = thirdPersonCamera.UI_Camera;

            if (UI_Camera != null)
            {
                UI_LayerMask = UI_Camera.cullingMask & ~(1 << 5);
            }

            // =========================================================

            developerOptions.SetOptions(this);
        }

        private void InitializeLayers()
        {
            playerLayer = LayerMask.NameToLayer("Player");
            enemyLayer = LayerMask.NameToLayer("Enemy");

            interactionLayer = LayerMask.NameToLayer("Interaction");

            targetLayer = LayerMask.NameToLayer("Target");

            platformLayer = LayerMask.NameToLayer("Platform");

            collisionTriggerLayer = LayerMask.NameToLayer("Collision Trigger");
        }

        private void InitializeColors()
        {
            if (highlightColors.Count == 0)
            {
                Color32 newColor = new Color32(128, 231, 255, 255);

                highlightColors.Add(newColor);
            }

            // =========================================================

            hexValues.Clear();

            int listCount = highlightColors.Count;

            for (int i = 0; i < listCount; i ++)
            {
                string hexValue = GetHexValue(highlightColors[i]);

                hexValues.Add(hexValue);
            }
        }

        private void CreateFreeTransforms()
        {
            GameObject newObjectA = new GameObject("Free Transform");

            newObjectA.hideFlags = HideFlags.HideInHierarchy;

            freeTransform = newObjectA.transform;

            // =========================================================

            if (cameraTarget != null)
            {
                GameObject newObjectB = new GameObject("Free Transform");

                newObjectB.hideFlags = HideFlags.HideInHierarchy;

                cameraTarget.SetFreeTransform(newObjectB.transform);
            }
        }

        public void SetGCMode(int value)
        {
            if (value <= 0)
            {
                if (!developerOptions.useManualGC)
                {
                    GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
                }
            }

            else if (value == 1)
            {
                #if !UNITY_EDITOR

                    GarbageCollector.GCMode = GarbageCollector.Mode.Manual;

                #endif
            }
        }

        public void SetCanvasAlpha(float value) // called by MenuManager.cs and DialogueBox.cs
        {
            int listCount = canvasGroups.Count;

            for (int i = 0; i < listCount; i ++)
            {
                CanvasGroup currentGroup = canvasGroups[i];

                if (currentGroup != null)
                {
                    canvasGroups[i].alpha = value;
                }
            }
        }

        public void ShowUILayer(bool value) // called by MenuManager.cs and DialogueBox.cs
        {
            Camera UI_Camera = thirdPersonCamera.UI_Camera;

            if (UI_Camera != null)
            {
                if (value)
                {
                    UI_Camera.cullingMask = UI_LayerMask | (1 << 5);
                }

                else
                {
                    UI_Camera.cullingMask = UI_LayerMask & ~(1 << 5);
                }
            }
        }

        public static void PlayAudioSource(AudioSource audioSource)
        {
            if (audioSource != null)
            {
                AudioClip audioClip = audioSource.clip;

                if (audioClip != null)
                {
                    audioSource.Stop();

                    audioSource.Play();
                }
            }
        }

        public static string GetHexValue(Color32 color)
        {
            int r = color.r;
            int g = color.g;
            int b = color.b;

            string rHexValue = "0" + r.ToString("X");
            string gHexValue = "0" + g.ToString("X");
            string bHexValue = "0" + b.ToString("X");

            rHexValue = rHexValue.Substring(rHexValue.Length - 2);
            gHexValue = gHexValue.Substring(gHexValue.Length - 2);
            bHexValue = bHexValue.Substring(bHexValue.Length - 2);

            string hexValue = rHexValue + gHexValue + bHexValue;

            return hexValue;
        }

        public static void ResetGameObject(GameObject targetObject)
        {
            if (targetObject != null)
            {
                targetObject.SetActive(false);

                targetObject.SetActive(true);
            }
        }

        public string GetHexValue(int index)
        {
            int listCount = hexValues.Count;

            if (index < 0)
                index = 0;

            else if (index >= listCount)
                index = listCount - 1;

            return hexValues[index];
        }

        public void SetRefreshRate(int refreshRate)
        {
            Application.targetFrameRate = refreshRate;

            float fixedRate = refreshRate;

            if (fixedRate >= 100.0f)
                fixedRate /= 2.0f;

            Time.fixedDeltaTime = 1.0f / fixedRate;
        }

        public void SetVSyncCount(int value)
        {
            if (value >= 0 && value <= 2)
            {
                QualitySettings.vSyncCount = value;

                float fixedRate = Screen.currentResolution.refreshRate;

                if (value == 2)
                    fixedRate /= 2.0f;

                if (fixedRate >= 100.0f)
                    fixedRate /= 2.0f;

                Time.fixedDeltaTime = 1.0f / fixedRate;
            }
        }

        private void OnApplicationQuit()
        {
            if (!QuitConfirmation.quitConfirmation)
            {
                MenuManager menuManager = GetComponent<MenuManager>();

                menuManager.RequestApplicationQuit();
            }
        }

        public void QuitApplication() // invoked by "Application Quit" menu
        {
            #if UNITY_EDITOR

                UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Play");

            #endif

                // =============================================================

                QuitConfirmation.quitConfirmation = true;

            Application.Quit();
        }

        private void Reset()
        {
            highlightColors.Clear();

            Color32 newColor = new Color32(128, 231, 255, 255);

            highlightColors.Add(newColor);

            // =========================================================

            string[] layerNames = new string[2];

            layerNames[0] = "Ground";
            layerNames[1] = "Platform";

            groundLayerMask = LayerMask.GetMask(layerNames);

            // =========================================================

            layerNames = new string[3];

            layerNames[0] = "Enemy";
            layerNames[1] = "Platform";
            layerNames[2] = "Physics";

            physicsLayerMask = LayerMask.GetMask(layerNames);

            // =========================================================

            string noteA = "Please add the \"GameplayElements\" tag to this game object so that other components can find and cache the components of this object.";

            string noteB = "Failing to do this will result in NullReferenceException errors as many objects rely on the Scene Handler and its other components.";

            notes = noteA + " " + noteB;
        }

        // =============================================================
        //    Message Lists
        // =============================================================

        [Serializable]
        public class MessageLists
        {
            public MessageList messageList;

            public MessageList areaName;

            public MessageList notification;
        }

        // =============================================================
        //    Audio Lists
        // =============================================================

        [Serializable]
        public class AudioLists
        {
            public int waterListIndex = -1;

            public Transform m_footsteps;

            public Transform m_targetHitSounds;
            public Transform m_weaponHitSounds;

            // =========================================================

            [HideInInspector] public List<AudioListGroup> footsteps = new List<AudioListGroup>();

            [HideInInspector] public List<AudioList> targetHitSounds = new List<AudioList>();
            [HideInInspector] public List<AudioList> weaponHitSounds = new List<AudioList>();

            // =========================================================

            [HideInInspector] public List<AudioSequence> landingSounds = new List<AudioSequence>();

            // =========================================================

            public void Initialize()
            {
                if (m_footsteps != null)
                {
                    GetAudioListGroups(m_footsteps, footsteps);

                    GetAudioSequences(m_footsteps, landingSounds);
                }

                if (m_targetHitSounds != null)
                {
                    GetAudioLists(m_targetHitSounds, targetHitSounds);
                }

                if (m_weaponHitSounds != null)
                {
                    GetAudioLists(m_weaponHitSounds, weaponHitSounds);
                }
            }

            private void GetAudioListGroups(Transform listTransform, List<AudioListGroup> audioListGroups)
            {
                audioListGroups.Clear();

                int childCount = listTransform.childCount;

                for (int i = 0; i < childCount; i ++)
                {
                    AudioListGroup audioListGroup = listTransform.GetChild(i).GetComponent<AudioListGroup>();

                    audioListGroups.Add(audioListGroup);
                }
            }

            private void GetAudioLists(Transform listTransform, List<AudioList> audioLists)
            {
                audioLists.Clear();

                int childCount = listTransform.childCount;

                for (int i = 0; i < childCount; i ++)
                {
                    AudioList audioList = listTransform.GetChild(i).GetComponent<AudioList>();

                    audioLists.Add(audioList);
                }
            }

            private void GetAudioSequences(Transform listTransform, List<AudioSequence> audioSequences)
            {
                audioSequences.Clear();

                int childCount = listTransform.childCount;

                for (int i = 0; i < childCount; i ++)
                {
                    AudioSequence audioSequence = listTransform.GetChild(i).GetComponent<AudioSequence>();

                    audioSequences.Add(audioSequence);
                }
            }
        }

        // =============================================================
        //    Developer Options
        // =============================================================

        [Serializable]
        public class DeveloperOptions
        {
            public bool useManualGC;

            [Range(0, 2)]
            public int vSyncCount;

            public void SetOptions(SceneHandler sceneHandler)
            {
                Application.targetFrameRate = -1;

                if (useManualGC)
                {
                    sceneHandler.SetGCMode(1);
                }

                sceneHandler.SetVSyncCount(vSyncCount);
            }
        }

        // =============================================================
        //    Yield Instructions
        // =============================================================

        public static WaitForSecondsRealtime WaitForTenthSecond = new WaitForSecondsRealtime(0.1f);

        public static WaitForSecondsRealtime WaitForQuarterSecond = new WaitForSecondsRealtime(0.25f);

        public static WaitForSecondsRealtime WaitForThirdSecond = new WaitForSecondsRealtime(0.33f);

        public static WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();

        public static WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

        // =============================================================
        //    Get Methods - Variables
        // =============================================================

        public int GetPlayerLayer()
        {
            return playerLayer;
        }

        public int GetEnemyLayer()
        {
            return enemyLayer;
        }

        public int GetInteractionLayer()
        {
            return interactionLayer;
        }

        public int GetTargetLayer()
        {
            return targetLayer;
        }

        public int GetPlatformLayer()
        {
            return platformLayer;
        }

        public int GetCollisionTriggerLayer()
        {
            return collisionTriggerLayer;
        }

        // =============================================================
        //    Get Methods - Components
        // =============================================================

        public ThirdPersonCamera GetThirdPersonCamera()
        {
            return thirdPersonCamera;
        }

        public Camera GetUICamera()
        {
            return UI_Camera;
        }

        public Transform GetFreeTransform()
        {
            return freeTransform;
        }
    }
}









