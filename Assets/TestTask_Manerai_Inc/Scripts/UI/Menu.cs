using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Events;

using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class Menu : MonoBehaviour
    {
        public int rows;
        public int columns;

        private bool activeMenu;

        private bool mouseMoved;

        private bool closable = true; // disabled for dialogue options
        private bool useKeyInput = true; // disabled for dialogue options

        private int currentRow = 0;
        private int currentColumn = 0;

        public float animationDelay = 0f; // the time between each button's entry animation

        private Transform currentSelection;

        public bool resetSelection = true; // if true, always select first button when menu opens

        public bool useHighlight; // if enabled, the second image of the selection will be placed on top of the first; otherwise, the second image will replace the first

        public int confirmType; // (0 = south, 1 = west) can be used to prevent a choice when spamming confirm to skip dialogue

        public Transform defaultSelection;

        private bool menuOpening;

        private int lastInput = -1;

        private Transform buttons;
        private Transform m_interface;

        public GameEvent onMenuOpened;
        public GameEvent onMenuClosed;

        private IEnumerator currentCoroutine;

        private bool usingPlayerManager;

        private float selectTime;

        public SoundOverrides soundOverrides = new SoundOverrides();

        [Space(10)]

        public List<GameObject> menuItems = new List<GameObject>();

        private GameObject currentMenuItem;

        // ===============================================
        //    Selection Input Variables
        // ===============================================

        private float inputDelayTimer;
        private float inputRepeatTimer;

        private static float inputDelayTime = 0.5f;
        private static float inputRepeatTime = 0.075f;

        private bool useHoldInput;

        private bool selectionInputHeld;

        private int deltaRow;
        private int deltaColumn;

        // ===============================================
        //    Dependencies
        // ===============================================

        private InputManager inputManager;
        private PlayerManager playerManager;

        private MenuManager menuManager;

        private void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            inputManager = elements.GetComponent<InputManager>();
            playerManager = elements.GetComponent<PlayerManager>();

            menuManager = elements.GetComponent<MenuManager>();

            usingPlayerManager = playerManager != null;

            // ===============================================

            buttons = transform.GetChild(0).GetChild(1);
            m_interface = transform.GetChild(0).GetChild(2);

            // ===============================================

            soundOverrides.InitializeOverrides();

            InitializeButtons();

            DisableMenuItems();

            OnDisable();
        }

        private void OnEnable()
        {
            onMenuOpened.Invoke();

            PlaySound(3);

            // ===============================================

            buttons.gameObject.SetActive(true);

            m_interface.gameObject.SetActive(true);

            // ===============================================

            useHoldInput = false;

            mouseMoved = false;

            menuOpening = true;

            transform.localScale = Vector3.one;

            // ===============================================

            if (menuManager.openMenus.Count == 0)
            {
                menuManager.OnMenusOpen();
            }

            // ===============================================

            if (!menuManager.openMenus.Contains(this))
            {
                menuManager.openMenus.Add(this);
            }

            menuManager.SwitchActiveMenu(this);

            // ===============================================

            StartCoroutine(EntryAnimation());
        }

        private void OnDisable()
        {
            if (activeMenu)
            {
                onMenuClosed.Invoke();
            }

            // ===============================================

            int childCount = buttons.childCount;

            for (int i = 0; i < childCount; i ++)
            {
                buttons.GetChild(i).gameObject.SetActive(false);
            }

            // ===============================================

            if (menuManager != null && menuManager.openMenus.Contains(this))
            {
                menuManager.RemoveMenu(this);
            }
        }

        private void Update()
        {
            if (activeMenu)
            {
                if (!menuOpening)
                {
                    var keyboard = Keyboard.current;

                    HandleInput(keyboard);

                    SelectionInput(keyboard);
                    SelectionInputHeld(keyboard);
                }

                else
                {
                    menuOpening = false;
                }
            }
        }

        private IEnumerator EntryAnimation()
        {
            int childCount = buttons.childCount;

            for (int i = 0; i < childCount; i ++)
            {
                Button currentButton = buttons.GetChild(i).GetComponent<Button>();

                if (currentButton.enabled)
                {
                    currentButton.gameObject.SetActive(true);

                    yield return new WaitForSecondsRealtime(animationDelay);
                }
            }
        }

        private void DetermineCursorPosition(int controlType)
        {
            if (resetSelection)
            {
                CursorExitAll();

                if (buttons.childCount > 0)
                {
                    Transform temp = buttons.GetChild(0);

                    if (controlType == 0 && !useKeyInput)
                        temp = null;

                    else if (defaultSelection != null)
                        temp = defaultSelection;

                    // ===============================================

                    if (temp != null)
                    {
                        CursorEnter(temp);
                    }
                }
            }
        }

        public void CloseMenu()
        {
            activeMenu = false; // force disable to immediately prevent further input

            menuManager.RemoveMenu(this);

            // ===============================================

            int eventCount = onMenuClosed.GetPersistentEventCount();

            if (eventCount > 0)
                onMenuClosed.Invoke();

            else
                gameObject.SetActive(false);
        }

        public void StartCloseAnimation() // invoked by onMenuClosed: this is the default closing animation of the menu
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            currentCoroutine = CloseAnimation();

            // ===============================================

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(currentCoroutine);
            }
        }

        private IEnumerator CloseAnimation()
        {
            buttons.gameObject.SetActive(false);

            m_interface.gameObject.SetActive(false);

            // ===============================================

            Transform window = transform.GetChild(0);

            SmoothTransform smoothTransform = window.GetComponent<SmoothTransform>();

            if (smoothTransform != null)
            {
                smoothTransform.ScaleTowards(Vector3.zero, 0.1f);
            }

            // ===============================================

            while (window.localScale.x > 0.05f)
            {
                yield return null;
            }

            // ===============================================

            smoothTransform.ScaleTowards(Vector3.zero, 0f);

            window.localScale = Vector3.zero;

            // ===============================================

            gameObject.SetActive(false);
        }

        public void CursorEnter(Transform button)
        {   
            if (activeMenu && button.GetComponent<Button>() != null)
            {
                bool validButton = button.childCount >= 2 && button != currentSelection;

                if (validButton)
                {   
                    if (currentSelection != null)
                    {
                        CursorExit(currentSelection);
                    }

                    // ===============================================

                    if (currentMenuItem != null)
                    {
                        currentMenuItem.SetActive(false);

                        currentMenuItem = null;
                    }

                    int index = button.GetSiblingIndex();

                    OpenMenuItem(index);

                    // ===============================================

                    button.GetChild(0).gameObject.SetActive(useHighlight);

                    button.GetChild(1).gameObject.SetActive(true);

                    currentSelection = button;

                    // ===============================================

                    int siblingIndex = button.GetSiblingIndex();

                    currentRow = siblingIndex / columns;

                    currentColumn = siblingIndex - currentRow * columns;
                }
            }
        }

        public void OnPointerEnter(Transform button) // invoke by event trigger - pointer enter
        {
            if (button != currentSelection)
            {
                PlaySound(0);
            }

            CursorEnter(button);
        }

        public void OnPointerExit(Transform button) // invoked by event trigger - pointer exit
        {
            int controlType = inputManager.GetControlType();

            if (controlType == 0)
            {
                CursorExit(button);
            }
        }

        public void CursorExit(Transform button)
        {
            if (currentMenuItem != null)
            {
                currentMenuItem.SetActive(false);

                currentMenuItem = null;
            }

            // ===============================================

            button.GetChild(0).gameObject.SetActive(true);

            button.GetChild(1).gameObject.SetActive(false);

            currentSelection = null;

            // ===============================================

            // currentRow = 0;

            // currentColumn = 0;
        }

        public void CursorExitAll()
        {
            int buttonCount = buttons.childCount;

            for (int i = 0; i < buttonCount; i ++)
            {
                CursorExit(buttons.GetChild(i));
            }
        }

        private void OpenMenuItem(int index)
        {
            if (index < menuItems.Count)
            {
                currentMenuItem = menuItems[index];

                currentMenuItem.SetActive(true);
            }
        }

        private void SetNewSelection(GameObject selection)
        {
            if (selection != null && selection.GetComponent<Button>() != null)
            {
                Transform button = selection.transform;

                Transform menuTransform = button.parent.parent.parent;

                if (menuTransform != null)
                {
                    Menu menu = menuTransform.GetComponent<Menu>();

                    if (menu != null && menu.GetActiveMenu())
                    {   
                        OnPointerEnter(button);
                    }
                }
            }
        }

        private void HandleInput(Keyboard keyboard)
        {
            if (!mouseMoved)
            {
                if (inputManager.MouseInput())
                {
                    GameObject cursorSelection = CursorSelection();

                    SetNewSelection(cursorSelection);

                    mouseMoved = true;
                }
            }

            // ===============================================

            bool escapeKeyDown = keyboard.escapeKey.wasPressedThisFrame;

            bool xKeyDown = keyboard.xKey.wasPressedThisFrame;

            // ===============================================

            if (closable && (escapeKeyDown || xKeyDown || inputManager.CancelButtonDown()))
            {
                PlaySound(2);

                CloseMenu();
            }
        }

        private GameObject CursorSelection() // solved by Michal_Stangel: https://forum.unity.com/threads/method-to-check-if-mouse-is-hovering-over-some-ui-object.934359/
        {
            GameObject cursorSelection = null;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
            };

            Vector2 mousePosition = Mouse.current.position.ReadValue();

            pointerData.position = new Vector2(mousePosition.x, mousePosition.y);

            List<RaycastResult> results = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                cursorSelection = results[0].gameObject;
            }

            return cursorSelection;
        }

        public void SetActiveMenu(bool value)
        {   
            activeMenu = value;

            useHoldInput = false;

            // ===============================================

            int childCount = buttons.childCount;

            for (int i = 0; i < childCount; i ++)
            {
                buttons.GetChild(i).GetComponent<Button>().interactable = value;
            }

            // ===============================================

            if (value)
            {
                mouseMoved = false;

                DetermineCursorPosition(inputManager.GetControlType());
            }

            else
            {
                CursorExitAll();
            }
        }

        private void SelectionInput(Keyboard keyboard)
        {
            int selectionInput = -1;

            int navigationInput = GetNavigationInputDown(keyboard);

            // ===============================================

            if (navigationInput == 0 && rows > 1)
                selectionInput = 0;

            else if (navigationInput == 1 && columns > 1)
                selectionInput = 1;

            else if (navigationInput == 2 && rows > 1)
                selectionInput = 2;

            else if (navigationInput == 3 && columns > 1)
                selectionInput = 3;

            // ===============================================

            if (selectionInput >= 0)
            {
                if (selectionInput != lastInput)
                {
                    inputDelayTimer = 0f;
                    inputRepeatTimer = inputRepeatTime;

                    lastInput = selectionInput;
                }

                SwitchSelection(selectionInput);
            }

            // ===============================================

            if (ConfirmButtonDown(keyboard))
            {
                if (currentSelection != null)
                {
                    PlaySound(1);

                    Button button = currentSelection.GetComponent<Button>();

                    button.onClick.Invoke();
                }
            }
        }

        private void SelectionInputHeld(Keyboard keyboard)
        {
            selectionInputHeld = false;

            deltaRow = 0;
            deltaColumn = 0;

            int navigationInput = GetNavigationInputHeld(keyboard);

            // ===============================================

            if (navigationInput == 0 && rows > 1)
                DeltaRow(-1);

            else if (navigationInput == 1 && columns > 1)
                DeltaColumn(1);

            else if (navigationInput == 2 && rows > 1)
                DeltaRow(1);

            else if (navigationInput == 3 && columns > 1)
                DeltaColumn(-1);

            // ===============================================

            if (selectionInputHeld)
            {
                inputDelayTimer += Time.unscaledDeltaTime;

                if (inputDelayTimer >= inputDelayTime)
                {
                    inputRepeatTimer += Time.unscaledDeltaTime;

                    if (inputRepeatTimer >= inputRepeatTime)
                    {
                        SwitchSelection(deltaRow, deltaColumn);

                        inputRepeatTimer = 0f;
                    }
                }
            }

            else
            {
                lastInput = -1;

                inputDelayTimer = 0f;
                inputRepeatTimer = inputRepeatTime;
            }
        }

        private bool ConfirmButtonDown(Keyboard keyboard)
        {
            bool keyInput = useKeyInput && (keyboard.fKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame);

            bool gamepadInput = (confirmType == 0 && inputManager.ConfirmButtonDown()) || (confirmType == 1 && GamepadButtonDown.West());

            return (keyInput || gamepadInput);
        }

        private int GetNavigationInputDown(Keyboard keyboard)
        {
            int input = -1;

            bool useWASD = true;

            if (usingPlayerManager)
            {
                PlayerController activePlayer = playerManager.GetActivePlayer();

                useWASD = activePlayer != null ? activePlayer.GetDisableMoveInput() : true;
            }

            bool keyUp = useKeyInput && (keyboard.upArrowKey.wasPressedThisFrame || (useWASD && keyboard.wKey.wasPressedThisFrame));
            bool keyLeft = useKeyInput && (keyboard.leftArrowKey.wasPressedThisFrame || (useWASD && keyboard.aKey.wasPressedThisFrame));
            bool keyDown = useKeyInput && (keyboard.downArrowKey.wasPressedThisFrame || (useWASD && keyboard.sKey.wasPressedThisFrame));
            bool keyRight = useKeyInput && (keyboard.rightArrowKey.wasPressedThisFrame || (useWASD && keyboard.dKey.wasPressedThisFrame));

            // ===============================================

            if (keyUp || GamepadButtonDown.DpadUp())
                input = 0;

            if (keyLeft || GamepadButtonDown.DpadLeft())
                input = 1;

            if (keyDown || GamepadButtonDown.DpadDown())
                input = 2;

            if (keyRight || GamepadButtonDown.DpadRight())
                input = 3;

            // ===============================================

            if (!useHoldInput)
            {
                useHoldInput = input >= 0;
            }

            return input;
        }

        private int GetNavigationInputHeld(Keyboard keyboard)
        {
            int input = -1;

            if (useHoldInput)
            {
                bool useWASD = true;

                if (usingPlayerManager)
                {
                    PlayerController activePlayer = playerManager.GetActivePlayer();

                    useWASD = activePlayer != null ? activePlayer.GetDisableMoveInput() : true;
                }

                bool keyUp = useKeyInput && (keyboard.upArrowKey.isPressed || (useWASD && keyboard.wKey.isPressed));
                bool keyLeft = useKeyInput && (keyboard.leftArrowKey.isPressed || (useWASD && keyboard.aKey.isPressed));
                bool keyDown = useKeyInput && (keyboard.downArrowKey.isPressed || (useWASD && keyboard.sKey.isPressed));
                bool keyRight = useKeyInput && (keyboard.rightArrowKey.isPressed || (useWASD && keyboard.dKey.isPressed));

                if (keyUp || GamepadButtonHeld.DpadUp())
                    input = 0;

                if (keyLeft || GamepadButtonHeld.DpadLeft())
                    input = 1;

                if (keyDown || GamepadButtonHeld.DpadDown())
                    input = 2;

                if (keyRight || GamepadButtonHeld.DpadRight())
                    input = 3;
            }

            return input;
        }

        private void DeltaRow(int delta)
        {
            selectionInputHeld = true;

            deltaRow += delta;
        }

        private void DeltaColumn(int delta)
        {
            selectionInputHeld = true;

            deltaColumn += delta;
        }

        private void SwitchSelection(int selectionInput)
        {
            PlaySound(0);

            mouseMoved = false;

            if (selectionInput == 0)
            {
                currentRow --;

                if (currentRow < 0)
                    currentRow = rows - 1;
            }

            else if (selectionInput == 1)
            {
                currentColumn --;

                if (currentColumn < 0)
                    currentColumn = columns - 1;
            }

            else if (selectionInput == 2)
            {
                currentRow ++;

                if (currentRow >= rows)
                    currentRow = 0;
            }

            else if (selectionInput == 3)
            {
                currentColumn ++;

                if (currentColumn >= columns)
                    currentColumn = 0;
            }

            // ===============================================

            int index = currentRow * columns + currentColumn;

            TrySwitch(index);
        }

        private void SwitchSelection(int deltaRow, int deltaColumn)
        {
            PlaySound(0);

            mouseMoved = false;

            if (deltaRow != 0 || deltaColumn != 0)
            {
                if (deltaRow == -1)
                {
                    currentRow --;

                    if (currentRow < 0)
                        currentRow = rows - 1;
                }

                else if (deltaRow == 1)
                {
                    currentRow ++;

                    if (currentRow >= rows)
                        currentRow = 0;
                }

                // ===============================================

                if (deltaColumn == -1)
                {
                    currentColumn --;

                    if (currentColumn < 0)
                        currentColumn = columns - 1;
                }

                else if (deltaColumn == 1)
                {
                    currentColumn ++;

                    if (currentColumn >= columns)
                        currentColumn = 0;
                }

                // ===============================================

                int index = currentRow * columns + currentColumn;

                TrySwitch(index);
            }
        }

        private void TrySwitch(int index)
        {
            if (index < buttons.childCount)
            {
                Transform newSelection = buttons.GetChild(index);

                CursorEnter(newSelection);
            }

            else
            {
                if (currentSelection != null)
                {
                    CursorExit(currentSelection);
                }
            }
        }

        public void PlaySound(int index) // called internally and by PauseMenu.cs
        {
            SceneHandler sceneHandler = menuManager.GetSceneHandler();

            if (sceneHandler != null)
            {
                AudioSource audioSource = null;

                // ===============================================

                if (index == 0)
                {
                    if (activeMenu)
                    {
                        float unscaledTime = Time.unscaledTime;

                        if (unscaledTime >= selectTime + 0.05f)
                        {
                            if (soundOverrides.useOverride[0])
                                audioSource = soundOverrides.select;

                            else
                                audioSource = sceneHandler.audioManager.systemSounds.select;

                            selectTime = unscaledTime;
                        }
                    }
                }

                else if (index == 1)
                {
                    if (soundOverrides.useOverride[1])
                        audioSource = soundOverrides.confirm;

                    else
                        audioSource = sceneHandler.audioManager.systemSounds.confirm;
                }

                else if (index == 2)
                {
                    if (soundOverrides.useOverride[2])
                        audioSource = soundOverrides.cancel;

                    else
                        audioSource = sceneHandler.audioManager.systemSounds.cancel;
                }

                else if (index == 3)
                {
                    if (soundOverrides.useOverride[3])
                        audioSource = soundOverrides.openMenu;

                    else
                        audioSource = sceneHandler.audioManager.systemSounds.openMenu;
                }

                // ===============================================

                SceneHandler.PlayAudioSource(audioSource);
            }
        }

        private void PlayConfirmSound()
        {
            PlaySound(1);
        }

        public void SetClosable(bool value) // called by MenuManager.cs
        {
            closable = value;
        }

        public void UseKeyInput(bool value) // called by MenuManager.cs
        {
            useKeyInput = value;
        }

        public bool GetActiveMenu() // called by MenuManager.cs
        {
            return activeMenu;
        }

        private void InitializeButtons()
        {
            int childCount = buttons.childCount;

            for (int i = 0; i < childCount; i ++)
            {
                Button button = buttons.GetChild(i).GetComponent<Button>();

                if (button != null)
                {
                    button.onClick.AddListener(PlayConfirmSound);
                }
            }
        }
        
        private void DisableMenuItems()
        {
            int listCount = menuItems.Count;
            
            for (int i = 0; i < listCount; i ++)
            {
                menuItems[i].SetActive(false);
            }
        }

        // =============================================================
        //    Sound Overrides
        // =============================================================

        [System.Serializable]
        public class SoundOverrides
        {
            public AudioSource select;
            public AudioSource confirm;
            public AudioSource cancel;

            public AudioSource openMenu;

            // ===============================================

            private bool[] m_useOverride = new bool[4];

            public bool[] useOverride { get { return m_useOverride; } }

            public void InitializeOverrides()
            {
                m_useOverride[0] = (select != null);
                m_useOverride[1] = (confirm != null);
                m_useOverride[2] = (cancel != null);

                m_useOverride[3] = (openMenu != null);
            }
        }
    }
}




