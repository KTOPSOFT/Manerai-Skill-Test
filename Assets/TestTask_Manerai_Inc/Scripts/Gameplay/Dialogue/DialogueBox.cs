using UnityEngine;
using UnityEngine.Profiling;

using UnityEngine.UI;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

using System.Text;

using TMPro;

namespace YukiOno.SkillTest
{
    public class DialogueBox : MonoBehaviour
    {
        public GameObject container;

        public TextGroup speakerName;
        public TextGroup dialogueText;

        public bool uppercaseName;

        private bool active;

        private float timer = 0f;

        private string combinedString;
        private int stringLength;

        private string coloredString;

        private StringBuilder newString = new StringBuilder();

        private string finishedString = "";

        private float[] alphaValues;

        private static string punctuations = ".?!";

        private List<int> punctuation = new List<int>();

        private int punctuationListIndex;
        private int punctuationCount;

        private List<int> highlight = new List<int>();

        private int highlightListIndex;
        private int highlightCount;

        private int charsRevealed = 0;
        private int charsFinished = 0;

        private string textColorCode;
        private string highlightColorCode;

        private bool isHidden;

        // =========================================================

        private NPC speaker;

        private PlayerController playerController;

        private bool updateDialogue;
        private bool hidingInterface;

        private static float pauseTime = 0.1f;

        private static float alphaOffset = 0.06f;
        private static float updateSpeed = 6.0f;

        private float targetAlpha = 1.0f;

        private float smoothVelocityA;
        private float smoothVelocityB;

        private string currentSpeaker;

        private DialogueSet currentSet;

        private int dialogueIndex;

        private int eventIndex;

        private IEnumerator resetContainer;

        private IEnumerator closeCoroutine;

        private const long memoryLimit = 64 * 1024 * 1024; // perform an instant, full GC if more than 64 MB of managed heap is used

        // =========================================================
        //    Game Events
        // =========================================================

        private GameEvent openDialogue = new GameEvent();

        private GameEvent revertCameraParent = new GameEvent();

        // =========================================================
        //    Component Dependencies
        // =========================================================

        private SceneHandler sceneHandler;
        private InputManager inputManager;
        private PlayerManager playerManager;
        private MenuManager menuManager;

        private CameraTarget cameraTarget;

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {
            GetComponents();

            combinedString = "";

            // ==========================================================

            textColorCode = SceneHandler.GetHexValue(dialogueText.textLayers[0].color);
            highlightColorCode = sceneHandler.GetHexValue(0);

            ShowText(false);

            // ==========================================================

            container.SetActive(false);

            gameObject.SetActive(false);

            // ==========================================================

            var tpc = sceneHandler.thirdPersonCamera;

            revertCameraParent.RemoveAllListeners();

            revertCameraParent.AddListener(delegate{tpc.RevertCameraParent();});

            revertCameraParent.AddListener(delegate{playerManager.ShowActivePlayer(true);});
        }

        private void GetComponents()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            sceneHandler = elements.GetComponent<SceneHandler>();
            inputManager = elements.GetComponent<InputManager>();
            playerManager = elements.GetComponent<PlayerManager>();
            menuManager = elements.GetComponent<MenuManager>();

            cameraTarget = sceneHandler.cameraTarget;
        }

        private void Update()
        {
            if (updateDialogue)
            {
                HandleInput();

                UpdateText();
            }
        }

        private void HandleInput()
        {
            var keyboard = Keyboard.current;

            bool fKeyDown = keyboard.fKey.wasPressedThisFrame;

            bool spaceKeyDown = keyboard.spaceKey.wasPressedThisFrame;

            bool enterKeyDown = keyboard.enterKey.wasPressedThisFrame;

            // ==========================================================

            bool input = fKeyDown || spaceKeyDown || enterKeyDown || GamepadButtonDown.South() || GamepadButtonDown.East();

            if (input && !isHidden && charsRevealed > 0)
            {
                if (!inputManager.GetUsingMenu())
                {
                    dialogueText.SetText(coloredString);

                    if (charsRevealed < stringLength)
                    {
                        timer = 0f;

                        charsRevealed = stringLength;
                        charsFinished = stringLength;
                    }

                    else
                    {
                        if (dialogueIndex < currentSet.dialogue.Count - 1)
                        {
                            ResumeDialogue();
                        }

                        else
                        {
                            int listCount = currentSet.dialogueOptions.Count;

                            if (listCount > 0)
                            {
                                timer = 0f;

                                charsRevealed = stringLength;
                                charsFinished = stringLength;
                            }

                            else
                            {
                                AudioSource resumeDialogue = sceneHandler.audioManager.interactionSounds.resumeDialogue;

                                SceneHandler.PlayAudioSource(resumeDialogue);

                                GameEvent gameEvent = currentSet.onDialogueFinish;

                                if (gameEvent.GetPersistentEventCount() > 0)
                                    gameEvent.Invoke();

                                Close();
                            }
                        }
                    }
                }
            }
        }

        private void UpdateText()
        {
            if (charsFinished < stringLength)
            {
                newString.Clear();

                newString.Append(finishedString);

                // ==========================================================

                if (punctuationCount > 0 && punctuationListIndex < punctuationCount)
                {
                    if (charsRevealed != punctuation[punctuationListIndex] + 1)
                    {
                        UpdateAlphaValues(punctuation[punctuationListIndex] + 1);
                    }

                    else if (timer < pauseTime)
                    {
                        charsRevealed = punctuation[punctuationListIndex] + 1;

                        UpdateAlphaValues(charsRevealed);

                        if (charsFinished == charsRevealed)
                            timer += Time.unscaledDeltaTime;
                    }

                    else
                    {
                        charsRevealed = punctuation[punctuationListIndex] + 1;

                        UpdateAlphaValues(charsRevealed);

                        timer = 0f;
                        punctuationListIndex ++;

                        RecalculateAlphaValues(charsFinished + 1);
                    }
                }

                else
                {
                    UpdateAlphaValues(stringLength);
                }

                if (highlightCount > 0)
                {   
                    newString.Replace("{", "<color=#" + highlightColorCode + ">");
                    newString.Replace("}", "</color>");
                }

                // ==========================================================

                dialogueText.SetText(newString.ToString());
            }

            else if (!menuManager.dialogueOptions.gameObject.activeSelf)
            {
                CheckForDialogueOptions();
            }
        }

        public void SwitchToSet(Transform newSet)
        {
            currentSet = newSet.GetComponent<DialogueSet>();

            SetDialogue(currentSpeaker, currentSet, 0);

            timer = 0f;
            charsFinished = 0;

            // audioSource.Play();
        }

        public void BranchToSet(int index) // invoked by dialogue options
        {
            AudioSource selectOption = sceneHandler.audioManager.interactionSounds.selectOption;

            SceneHandler.PlayAudioSource(selectOption);

            // ==========================================================

            if (currentSet != null && index < currentSet.transform.childCount)
            {   
                Transform newSet = currentSet.transform.GetChild(index);

                SwitchToSet(newSet);
            }

            else
            {
                if (updateDialogue)
                {
                    Close();
                }

                sceneHandler.ShowUILayer(true);
            }
        }

        private void CheckForDialogueOptions()
        {
            if (!inputManager.GetUsingMenu())
            {
                int dialogueCount = currentSet.dialogue.Count - 1;
                int choiceCount = currentSet.dialogueOptions.Count;

                if (dialogueIndex == dialogueCount && choiceCount > 0)
                {
                    Transform dialogueOptions = menuManager.dialogueOptions.transform;
                    Transform buttons = dialogueOptions.GetChild(0).GetChild(1);

                    int listCount = currentSet.dialogueOptions.Count;
                    int childCount = buttons.childCount;

                    buttons.localPosition = Vector3.down * 70.0f;

                    menuManager.dialogueOptions.rows = 0;
                    menuManager.dialogueOptions.columns = 1;

                    // =========================================================

                    for (int i = 0; i < childCount; i ++) // needed to determine if button will display on screen (controlled by Menu.EntryAnimation())
                    {
                        buttons.GetChild(i).GetComponent<Button>().enabled = false;
                    }

                    for (int i = 0; i < listCount; i ++)
                    {
                        if (i < childCount)
                        {
                            Transform currentButton = buttons.GetChild(i);

                            TextMeshProUGUI optionText = currentButton.GetChild(2).GetComponent<TextMeshProUGUI>();

                            optionText.SetText(currentSet.dialogueOptions[i]);

                            buttons.localPosition += Vector3.up * 70.0f;

                            currentButton.GetComponent<Button>().enabled = true;

                            menuManager.dialogueOptions.rows ++;
                        }
                    }

                    // =========================================================

                    menuManager.OpenMenu(dialogueOptions.gameObject);
                }
            }
        }

        private string ReplaceAtIndex(string str, string replacement, int index)
        {
            return str.Substring(0, index) + replacement + str.Substring(index + 1);
        }

        private void UpdateAlphaValues(int stopCondition)
        {
            for (int i = charsFinished; i < stringLength; i ++)
            {
                if (i < stopCondition)
                {
                    alphaValues[i] += Time.unscaledDeltaTime * updateSpeed;

                    int value = (int) (alphaValues[i] * 255.0f);

                    if (value < 255)
                    {
                        if (value > 0)
                        {
                            string hexValue = "0" + value.ToString("X");
                            hexValue = hexValue.Substring(hexValue.Length - 2);

                            string s = "<color=#" + CurrentColorCode(i) + hexValue + ">" + combinedString[i] + "</color>";
                            AddToNewString(s, combinedString[i]);

                            charsRevealed = i + 1;
                        }

                        else // if value is 0, skip the calculation process
                        {
                            string s = "<color=#" + CurrentColorCode(i) + "00>" + combinedString[i] + "</color>";
                            AddToNewString(s, combinedString[i]);
                        }
                    }

                    else
                    {
                        string s = "" + combinedString[i];
                        AddToNewString(s, combinedString[i]);

                        finishedString = combinedString.Substring(0, i + 1);

                        charsFinished = i + 1;

                        if (highlightListIndex + 2 < highlightCount && charsFinished > highlight[highlightListIndex + 1])
                            highlightListIndex += 2;
                    }
                }

                else
                {   
                    newString.Append("<color=#" + textColorCode + "00>" + combinedString[i] + "</color>");
                }
            }
        }

        private string CurrentColorCode(int i)
        {
            string colorCode = textColorCode;

            if (highlightCount > 0 && highlight[highlightListIndex] <= i && i <= highlight[highlightListIndex + 1])
                colorCode = highlightColorCode;

            return colorCode;
        }

        private void AddToNewString(string s, char c)
        {
            if (c == '{')
                newString.Append("<color=#" + highlightColorCode + ">");

            else if (c == '}')
                newString.Append("</color>");

            else
                newString.Append(s);
        }

        private void RecalculateAlphaValues(int start)
        {
            if (start >= 0 && start < alphaValues.Length)
            {
                alphaValues[start] = 0f;
            }

            // =========================================================

            for (int i = start + 1; i < stringLength; i ++)
            {
                alphaValues[i] = alphaValues[i - 1] - alphaOffset;
            }
        }

        private void StartEvent(DialogueSet dialogueSet)
        {
            if (eventIndex < dialogueSet.events.Count)
            {
                var currentEvent = dialogueSet.events[eventIndex];

                if (currentEvent.dialogueIndex == dialogueIndex)
                {
                    if (currentEvent.speaker != null)
                    {
                        string targetSpeaker = currentEvent.speaker.m_name;

                        if (!targetSpeaker.Equals(currentSpeaker))
                        {
                            currentSpeaker = targetSpeaker;

                            ResetContainer();
                        }
                    }

                    // =========================================================

                    Transform cameraAngle = currentEvent.cameraAngle;

                    if (cameraAngle != null)
                    {
                        sceneHandler.thirdPersonCamera.SetCameraParent(cameraAngle);
                    }

                    // =========================================================

                    if (currentEvent.hideDialogue)
                    {   
                        isHidden = true;

                        container.SetActive(false);
                    }
                    
                    bool showPlayer = !currentEvent.hidePlayer;
                    
                    playerManager.ShowActivePlayer(showPlayer);

                    // =========================================================

                    eventIndex ++;
                }
            }
        }

        private void ResetContainer()
        {
            if (resetContainer != null)
            {
                StopCoroutine(resetContainer);
            }

            resetContainer = IE_ResetContainer();

            StartCoroutine(resetContainer);
        }

        private IEnumerator IE_ResetContainer()
        {
            updateDialogue = false;

            container.SetActive(false);

            yield return SceneHandler.WaitForTenthSecond;

            container.SetActive(true);

            updateDialogue = true;
        }

        public void SetDialogue(string stringName, DialogueSet dialogueSet, int dialogueValue)
        {
            long memoryUsed = Profiler.GetMonoUsedSizeLong();

            if (memoryUsed > memoryLimit)
            {
                System.GC.Collect(0);
            }

            // =========================================================

            currentSpeaker = stringName;
            currentSet = dialogueSet;

            dialogueIndex = dialogueValue;

            // =========================================================

            if (dialogueValue == 0)
            {
                eventIndex = 0;
            }

            StartEvent(dialogueSet);

            // =========================================================

            string name = uppercaseName ? stringName.ToUpper() : currentSpeaker;

            speakerName.SetText(name);

            textColorCode = SceneHandler.GetHexValue(dialogueText.textLayers[0].color);
            highlightColorCode = sceneHandler.GetHexValue(0);

            // =========================================================

            punctuation.Clear();
            punctuationListIndex = 0;

            highlight.Clear();
            highlightListIndex = 0;

            // =========================================================

            combinedString = dialogueSet.dialogue[dialogueValue];

            string currentString = "";
            List<string> newList = new List<string>();

            int omissions = 0;

            for (int i = 0; i < combinedString.Length; i ++)
            {
                char c = combinedString[i];

                if (c == '*') // use stars to manually pause dialogue, even if they are not punctuations (i.e. commas or dashes)
                {
                    int pauseIndex = i - omissions - 1;

                    if (pauseIndex >= 0)
                        punctuation.Add(pauseIndex);

                    omissions ++;
                }

                else if (c != '|')
                {
                    currentString += c;
                }

                else
                {
                    newList.Add(currentString);
                    currentString = "";

                    omissions ++;
                }
            }

            newList.Add(currentString);

            combinedString = string.Join("\n", newList);

            stringLength = combinedString.Length;

            alphaValues = new float[stringLength];
            RecalculateAlphaValues(0);

            finishedString = "";

            charsRevealed = 0;

            coloredString = combinedString.Replace("{", "<color=#" + highlightColorCode + ">");
            coloredString = coloredString.Replace("}", "</color>");

            // =========================================================

            punctuations = punctuations.Replace(" ", "");

            for (int i = 0; i < stringLength; i ++)
            {
                string s = "" + combinedString[i];

                if (punctuations.Contains(s)) // How to deal with ellipsis? We do not want a slight delay between each period of an ellipsis.
                    punctuation.Add(i);

                else if (combinedString[i] == '{' || combinedString[i] == '}')
                    highlight.Add(i);
            }

            punctuation.Sort();

            punctuationCount = punctuation.Count;
            highlightCount = highlight.Count;

            // =========================================================

            dialogueText.SetText("");
        }

        private void ShowText(bool value)
        {
            if (speakerName != null)
            {
                speakerName.gameObject.SetActive(value);

                if (!value)
                    speakerName.SetText("");
            }

            if (dialogueText != null)
            {
                dialogueText.gameObject.SetActive(value);

                if (!value)
                    dialogueText.SetText("");
            }
        }

        public void Open(NPC npc, PlayerController player)
        {
            sceneHandler.SetGCMode(1);

            System.GC.Collect(0);

            // =========================================================

            container.SetActive(true);

            ShowText(true);

            // =========================================================

            speaker = npc;

            speaker.Greeting();

            playerController = player;

            // =========================================================

            timer = 0f;

            charsFinished = 0;

            updateDialogue = true;
        }

        public void Close()
        {
            System.GC.Collect(0);

            sceneHandler.SetGCMode(0);

            // =========================================================

            container.SetActive(false);

            ShowText(false);

            hidingInterface = false;

            updateDialogue = false;

            // =========================================================

            bool usingMenu = inputManager.GetUsingMenu();

            sceneHandler.ShowUILayer(!usingMenu);

            // =========================================================

            var tpc = sceneHandler.thirdPersonCamera;

            if (tpc.UsingCameraAngle())
            {
                sceneHandler.screenFade.FadeTo(revertCameraParent);
            }

            // =========================================================

            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(CloseCoroutine());
            }

            else
            {
                sceneHandler.ShowUILayer(true);
            }
        }

        private IEnumerator CloseCoroutine()
        {
            // inputManager.SetCameraType(0);

            // if (speaker != null)
            // speaker.RevertHeadTrackSmooth();

            yield return null; // wait one frame to allow the third person camera's values to update before starting camera blend transition

            // =========================================================

            CameraTarget cameraTarget = sceneHandler.cameraTarget;

            cameraTarget.SetPlayer(playerController, false);

            yield return SceneHandler.WaitForQuarterSecond;

            // =========================================================

            closeCoroutine = CanvasFadeIn();

            StartCoroutine(closeCoroutine);

            yield return SceneHandler.WaitForQuarterSecond;

            // =========================================================

            bool usingMenu = inputManager.GetUsingMenu();

            playerController.DisableButtonInputs(usingMenu);

            playerController.SetInteractionState(false);

            // =========================================================

            var thirdPersonCamera = sceneHandler.GetThirdPersonCamera();

            thirdPersonCamera.LockMouseY(false);

            thirdPersonCamera.RevertSmoothRotation();

            thirdPersonCamera.ForceTransitionTime(-1.0f);

            cameraTarget.LockSmoothTime(false);

            // cameraTarget.SetSmoothTime(0.05f); // commented out: smooth time will revert by itself once player starts moving again

            sceneHandler.pauseMenu.gameObject.SetActive(true);

            // =========================================================

            PlayerInteraction interaction = playerManager.playerInteraction;

            if (interaction != null)
            {
                int controlType = inputManager.GetControlType();

                interaction.ResetActionPrompt(controlType, true);
            }

            // =========================================================

            currentSet.onDialogueClose.Invoke();

            active = false;
        }

        private IEnumerator CanvasFadeIn()
        {
            targetAlpha = 0f;

            sceneHandler.SetCanvasAlpha(targetAlpha);

            // =========================================================

            while (targetAlpha < 1.0f)
            {
                targetAlpha = Mathf.SmoothDamp(targetAlpha, 1.0f, ref smoothVelocityB, 0.15f);

                if (1.0f - targetAlpha < 0.01f)
                    targetAlpha = 1.0f;

                // =========================================================

                bool gamePaused = sceneHandler.pauseMenu.GetGamePaused();

                if (!inputManager.GetUsingMenu() && !gamePaused)
                    sceneHandler.SetCanvasAlpha(targetAlpha);

                else
                    sceneHandler.SetCanvasAlpha(0f);

                // =========================================================

                yield return null;
            }

            // =========================================================

            gameObject.SetActive(false);
        }

        public float GetTargetAlpha() // called by MenuManager.cs
        {
            return targetAlpha;
        }

        public bool GetHidingInterface() // called by MenuManager.cs
        {
            return hidingInterface;
        }

        public bool GetActive() // called by PlayerInteraction.cs
        {
            return active;
        }

        public void StartDialogue() // called by NPC.cs
        {
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);

                closeCoroutine = null;
            }

            // =========================================================

            active = true;

            hidingInterface = true;

            targetAlpha = 0f;

            // =========================================================

            sceneHandler.SetCanvasAlpha(0f);
            sceneHandler.ShowUILayer(false);

            sceneHandler.pauseMenu.gameObject.SetActive(false);

            // =========================================================

            CanvasFadeIn actionPrompt = playerManager.playerInteraction.actionPrompt;

            if (actionPrompt != null)
            {
                actionPrompt.gameObject.SetActive(false);
            }
        }

        public void ResumeDialogue()
        {
            AudioSource resumeDialogue = sceneHandler.audioManager.interactionSounds.resumeDialogue;

            SceneHandler.PlayAudioSource(resumeDialogue);

            // =========================================================

            isHidden = false;

            container.SetActive(true);

            // =========================================================

            SetDialogue(currentSpeaker, currentSet, dialogueIndex + 1);

            timer = 0f;
            charsFinished = 0;
        }

        public GameEvent GetOpenDialogueEvent() // called by NPC.cs
        {
            return openDialogue;
        }
    }
}









