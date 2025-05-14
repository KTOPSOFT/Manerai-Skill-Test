using UnityEngine;
using UnityEngine.UI;

using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class PauseMenu : MonoBehaviour
    {
        public bool stopTime = true;

        public Image screenCapture;

        public GameObject background;

        public GameObject menu;

        public List<CanvasGroup> canvasGroups = new List<CanvasGroup>();

        private bool gamePaused;

        private Menu m_menu;

        // ===============================================
        //    Component Dependencies
        // ===============================================

        private Camera mainCamera;

        private SceneHandler sceneHandler;
        private InputManager inputManager;
        private PlayerManager playerManager;

        private MenuManager menuManager;

        private ScreenFade screenFade;

        // ===============================================
        //    Standard Methods
        // ===============================================

        private void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            mainCamera = Camera.main;

            sceneHandler = elements.GetComponent<SceneHandler>();
            inputManager = elements.GetComponent<InputManager>();
            playerManager = elements.GetComponent<PlayerManager>();

            menuManager = elements.GetComponent<MenuManager>();

            screenFade = sceneHandler.screenFade;

            // ===============================================

            if (menu != null)
            {
                m_menu = menu.GetComponent<Menu>();
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;

            bool pauseInput =  GamepadButtonDown.Start();

            if (keyboard.escapeKey.wasPressedThisFrame || pauseInput) // pause input is separate since escape key already closes menus
            {
                if(gamePaused)
                    Resume(pauseInput);

                else if (!inputManager.GetUsingMenu() && !screenFade.isFading)
                    Pause();
            }
        }

        public void Pause()
        {
            if (!gamePaused)
            {
                gamePaused = true;

                menuManager.OnMenusOpen();

                SetCanvasAlpha(0f);

                if (stopTime)
                {
                    StartCoroutine(OverrideScreenCapture());
                }

                else
                {
                    SetActive(background, true);

                    SetActive(menu, true);
                }
            }
        }

        public void Resume() // invoked by events
        {
            Resume(false); // false since "confirm" sound will play from Menu.SelectionInput()
        }

        private void Resume(bool playSound)
        {
            if (gamePaused && gameObject.activeInHierarchy)
            {   
                StartCoroutine(IE_Resume(playSound));
            }
        }

        private IEnumerator IE_Resume(bool playSound)
        {
            if (playSound && m_menu != null)
            {
                m_menu.PlaySound(2);
            }

            // =========================================================

            yield return null;

            mainCamera.enabled = true;

            // =========================================================

            SetActive(menu, false);

            SetActive(background, false);

            if (screenCapture != null)
            {
                screenCapture.gameObject.SetActive(false);
            }

            // =========================================================

            Time.timeScale = 1.0f;

            playerManager.SetGamePaused(false);

            sceneHandler.GetThirdPersonCamera().LockInput(false);

            sceneHandler.cameraTarget.LockPosition(false);

            // =========================================================

            SetCanvasAlpha(1.0f);

            gamePaused = false;
        }

        private IEnumerator OverrideScreenCapture()
        {
            sceneHandler.cameraTarget.LockPosition(true);

            sceneHandler.GetThirdPersonCamera().LockInput(true);

            playerManager.SetGamePaused(true);

            Time.timeScale = 0f;

            // =========================================================

            yield return null; // wait one frame to remove screen effects such as motion blur

            if (screenCapture != null)
            {
                sceneHandler.SetCanvasAlpha(0f);

                screenCapture.gameObject.SetActive(false);

                yield return SceneHandler.WaitForEndOfFrame;

                Texture2D newTexture = ScreenCapture.CaptureScreenshotAsTexture();

                int width = newTexture.width;
                int height = newTexture.height;

                screenCapture.material.SetTexture("_Texture", newTexture);

                screenCapture.gameObject.SetActive(true);

                mainCamera.enabled = false;
            }

            SetActive(background, true);

            SetActive(menu, true);
        }

        private void SetActive(GameObject m_object, bool value)
        {
            if (m_object != null)
            {
                m_object.SetActive(value);
            }
        }

        private void SetCanvasAlpha(float value)
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

        public void ResumeAndOpenMenu(MenuManager menuManager, GameObject menu) // called by MenuManager.cs
        {
            StartCoroutine(IE_ResumeAndOpenMenu(menuManager, menu));
        }

        private IEnumerator IE_ResumeAndOpenMenu(MenuManager menuManager, GameObject menu)
        {
            yield return StartCoroutine(IE_Resume(false)); // false since "cancel" sound will play from MenuManager.CloseAllMenus()

            menuManager.OpenMenu(menu);
        }

        public bool GetGamePaused() // called by MenuManager.cs
        {
            return gamePaused;
        }
    }
}









