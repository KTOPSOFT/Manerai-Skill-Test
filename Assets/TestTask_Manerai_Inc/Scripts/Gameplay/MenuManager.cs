using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [RequireComponent(typeof(SceneHandler))]

    public class MenuManager : MonoBehaviour
    {
        public Menu applicationQuit;
        public Menu dialogueOptions;

        public DialogueBox dialogueBox;

        public List<Menu> openMenus = new List<Menu>();

        private IEnumerator currentCoroutine;

        // ===============================================
        //    Dependencies
        // ===============================================

        private SceneHandler sceneHandler;
        private InputManager inputManager;
        private PlayerManager playerManager;

        private PauseMenu pauseMenu;

        // ===============================================
        //    Standard Methods
        // ===============================================

        private void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            sceneHandler = elements.GetComponent<SceneHandler>();
            inputManager = elements.GetComponent<InputManager>();
            playerManager = elements.GetComponent<PlayerManager>();

            if (sceneHandler != null)
                pauseMenu = sceneHandler.pauseMenu;

            InitializeMenus();
        }

        private void InitializeMenus()
        {
            if (applicationQuit != null)
            {
                applicationQuit.rows = 1;
                applicationQuit.columns = 2;

                applicationQuit.resetSelection = true;
            }

            // ===============================================

            if (dialogueOptions != null)
            {
                dialogueOptions.confirmType = 1;

                dialogueOptions.SetClosable(false);
                dialogueOptions.UseKeyInput(false);

                dialogueOptions.resetSelection = true;
            }

            // ===============================================

            if (dialogueBox != null)
            {
                dialogueBox.gameObject.SetActive(true);
            }
        }

        public void SwitchActiveMenu(Menu selectedMenu)
        {
            int listCount = openMenus.Count;

            for (int i = 0; i < listCount; i ++)
            {
                if (openMenus[i] == selectedMenu)
                    openMenus[i].SetActiveMenu(true);

                else
                    openMenus[i].SetActiveMenu(false);
            }
        }

        public void OpenMenu(GameObject menu)
        {
            menu.SetActive(false);

            menu.SetActive(true);
        }

        public void RemoveMenu(Menu selectedMenu) // called by Menu.cs when closing or disabling a menu
        {   
            openMenus.Remove(selectedMenu);

            // ===============================================

            int listCount = openMenus.Count;

            if (listCount > 0)
            {
                SwitchActiveMenu(openMenus[listCount - 1]);
            }

            // ===============================================

            if (openMenus.Count == 0)
            {
                OnMenusClosed();
            }
        }

        public void CloseAllMenus()
        {
            int listCount = openMenus.Count;

            // ===============================================

            if (listCount > 0)
            {
                AudioSource audioSource = sceneHandler.audioManager.systemSounds.cancel;

                SceneHandler.PlayAudioSource(audioSource);
            }

            // ===============================================

            for (int i = 0; i < listCount; i ++)
            {
                openMenus[0].gameObject.SetActive(false);
            }
        }

        public void ResetMenu(Menu menu) // called by InputManager.cs
        {
            if (menu.gameObject.activeSelf && menu.GetActiveMenu())
            {
                menu.gameObject.SetActive(false);

                OpenMenu(menu.gameObject);
            }
        }

        public void OnMenusOpen() // called by Menu.cs
        {
            SetUsingMenu(true);

            // ===============================================

            if (playerManager != null)
            {
                PlayerController controller = playerManager.GetActivePlayer();

                if (controller != null)
                {
                    controller.DisableButtonInputs(true);
                }

                // ===============================================

                sceneHandler.SetCanvasAlpha(0f);

                sceneHandler.ShowUILayer(false);
            }
        }

        private void OnMenusClosed()
        {
            SetUsingMenu(false);

            // ===============================================

            if (playerManager != null && !dialogueBox.GetHidingInterface())
            {
                PlayerController controller = playerManager.GetActivePlayer();

                if (controller != null)
                {
                    controller.DisableButtonInputs(false);
                }

                // ===============================================

                float targetAlpha = dialogueBox.GetTargetAlpha();

                sceneHandler.SetCanvasAlpha(targetAlpha);

                sceneHandler.ShowUILayer(true);
            }
        }

        private void SetUsingMenu(bool value)
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);

                currentCoroutine = null;
            }

            // ===============================================

            if (gameObject.activeInHierarchy)
            {
                currentCoroutine = IE_SetUsingMenu(value);

                StartCoroutine(currentCoroutine);
            }
        }

        private IEnumerator IE_SetUsingMenu(bool value)
        {
            yield return null;

            inputManager.SetUsingMenu(value);
        }

        public void RequestApplicationQuit() // called by SceneHandler.cs
        {
            CloseAllMenus();

            // ===============================================

            if (pauseMenu.GetGamePaused())
                pauseMenu.ResumeAndOpenMenu(this, applicationQuit.gameObject);

            else
                OpenMenu(applicationQuit.gameObject);
        }

        public SceneHandler GetSceneHandler() // called by Menu.cs
        {
            return sceneHandler;
        }
    }
}







