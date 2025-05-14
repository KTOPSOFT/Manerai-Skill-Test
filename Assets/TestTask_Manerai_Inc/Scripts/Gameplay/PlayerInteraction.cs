using UnityEngine;
using UnityEngine.InputSystem;

using System;

using System.Collections;
using System.Collections.Generic;

using TMPro;

namespace YukiOno.SkillTest
{
    public class PlayerInteraction : MonoBehaviour
    {
        public CanvasFadeIn actionPrompt;

        public TextMeshProUGUI promptText;

        public MessageList confirmAnimation;

        public List<string> interactionTypes = new List<string>();

        public List<string> inputTags = new List<string>();

        private bool actionConfirmed;

        // =========================================================

        private SceneHandler sceneHandler;

        private InputManager inputManager;

        private DialogueBox dialogueBox;

        private Transform activePlayer;

        private Interactable currentTarget;
        private Interactable previousTarget;

        private int currentType = -1;

        private List<Interactable> validTargets = new List<Interactable>();

        // =========================================================

        private int targetListCount;

        private List<Interactable> targetList = new List<Interactable>();

        // =========================================================

        private int targetsAdded;

        private List<Interactable> newTargets = new List<Interactable>();

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            sceneHandler = elements.GetComponent<SceneHandler>();

            inputManager = elements.GetComponent<InputManager>();

            dialogueBox = elements.GetComponent<SceneHandler>().dialogueBox;

            // =========================================================

            if (actionPrompt != null)
            {
                GameObject prompt = actionPrompt.gameObject;

                prompt.SetActive(true);

                prompt.SetActive(false);
            }
        }

        private void Update()
        {
            HandleInput();

            HandleNewTargets();

            UpdateTargets();
        }

        private void Reset()
        {
            interactionTypes.Clear();

            interactionTypes.Add("Interact");
            interactionTypes.Add("Take Item");
        }

        private void HandleInput()
        {
            var keyboard = Keyboard.current;

            // ==========================================================

            bool interactionInput = keyboard.fKey.wasPressedThisFrame || GamepadButtonDown.West();

            if (interactionInput)
            {
                bool usingMenus = inputManager.GetUsingMenu() || dialogueBox.GetActive();

                if (!usingMenus && currentTarget != null)
                {
                    currentTarget.StartInteraction();
                }
            }
        }

        private void HandleNewTargets()
        {
            if (targetsAdded > 0)
            {
                int index = newTargets.Count - 1;

                Interactable newTarget = newTargets[index];

                previousTarget = targetsAdded > 1 ? newTargets[index - 1] : previousTarget;

                HideNewTargetNames();

                SwitchTarget(newTarget);

                // =========================================================

                newTargets.Clear();

                targetsAdded = 0;
            }
        }

        private void UpdateTargets()
        {
            if (activePlayer != null)
            {
                int listCount = targetListCount;

                for (int i = 0; i < listCount; i ++)
                {
                    Interactable target = targetList[i];

                    float interactionRange = target.GetInteractionRange();

                    if (interactionRange > 0f)
                    {
                        Transform targetTransform = target.transform;

                        Vector3 offset = targetTransform.position - activePlayer.position;

                        // =========================================================

                        float distance = offset.sqrMagnitude;

                        if (distance <= target.GetValidTargetRange())
                        {
                            if (target.viewDependent)
                            {
                                Vector3 targetDirection = targetTransform.position - activePlayer.position;

                                float viewAngle = Vector3.Angle(targetDirection, activePlayer.forward);

                                // =========================================================

                                if (viewAngle <= 90.0f)
                                {
                                    AddValidTarget(target);
                                }

                                else
                                {
                                    RemoveValidTarget(target);
                                }
                            }

                            else
                            {
                                AddValidTarget(target);
                            }
                        }

                        else if (distance < target.GetTargetRange())
                        {
                            RemoveValidTarget(target);
                        }

                        else
                        {
                            RemoveTarget(target);

                            listCount --;

                            i --;
                        }
                    }
                }
            }
        }

        public void AddTarget(Interactable target) // called by InteractionTrigger.cs
        {
            if (!targetList.Contains(target))
            {
                targetList.Add(target);

                targetListCount ++;
            }
        }

        public void RemoveTarget(Interactable target) // called by Interactable.cs
        {
            if (targetList.Contains(target))
            {   
                targetList.Remove(target);

                targetListCount --;

                // =========================================================

                RemoveValidTarget(target);
            }
        }

        private void AddValidTarget(Interactable target)
        {
            if (!validTargets.Contains(target))
            {
                targetsAdded ++;

                newTargets.Add(target);

                validTargets.Add(target);
            }
        }

        private void RemoveValidTarget(Interactable target)
        {
            if (validTargets.Contains(target))
            {
                validTargets.Remove(target);

                // =========================================================

                int validTargetCount = validTargets.Count;

                if (validTargetCount > 0)
                {
                    Interactable newTarget = validTargets[validTargetCount - 1];

                    SwitchTarget(newTarget);
                }

                else
                {
                    ShowTargetName(target, false);

                    RemoveTargets();
                }
            }
        }

        private void SwitchTarget(Interactable target)
        {
            if (target != currentTarget)
            {
                ShowTargetName(currentTarget, false);

                previousTarget = currentTarget;

                SetCurrentTarget(target);

                ShowTargetName(currentTarget, true);

                // =============================================================

                if (currentTarget != previousTarget && !inputManager.GetUsingMenu())
                {
                    AudioSource targetAudio = sceneHandler.audioManager.interactionSounds.target;

                    SceneHandler.PlayAudioSource(targetAudio);
                }
            }
        }

        private void SetCurrentTarget(Interactable target)
        {
            int type = target.interactionType;

            ShowActionPrompt(true, type);

            currentTarget = target;

            currentType = type;
        }

        private void RemoveTargets()
        {
            ShowActionPrompt(false, -1);

            currentTarget = null;

            previousTarget = null;

            currentType = -1;
        }

        private void ShowTargetName(Interactable target, bool value)
        {
            if (target != null && target.gameObject.activeInHierarchy)
            {
                target.ShowTargetName(value);
            }
        }

        private void HideNewTargetNames()
        {
            int listCount = newTargets.Count;

            for (int i = 0; i < listCount; i ++)
            {
                GameObject canvas = newTargets[i].canvas;

                if (canvas != null)
                {
                    canvas.SetActive(false);
                }
            }
        }

        public void ClearLists() // called internally and by PlayerManager.cs
        {
            newTargets.Clear();

            targetList.Clear();

            validTargets.Clear();

            // =============================================================

            targetsAdded = 0;

            targetListCount = 0;

            // =============================================================

            ShowTargetName(currentTarget, false);

            RemoveTargets();
        }

        private void ShowActionPrompt(bool value, int type)
        {
            if (actionPrompt != null)
            {
                GameObject prompt = actionPrompt.gameObject;

                if (value)
                {
                    if (type != currentType || actionConfirmed)
                    {
                        SetPromptText(prompt, type);
                    }
                }

                else if (prompt.activeInHierarchy)
                {
                    if (actionConfirmed)
                    {
                        prompt.SetActive(false);
                    }

                    else
                    {
                        actionPrompt.FadeIn(false);
                    }
                }
            }

            actionConfirmed = false;
        }

        private void SetPromptText(GameObject prompt, int type)
        {
            prompt.SetActive(false);

            if (promptText != null)
            {
                string action = "Interact";

                if (type < interactionTypes.Count)
                {
                    action = interactionTypes[type];
                }

                // =============================================================

                int controlType = inputManager.GetControlType();

                string buttonTag = "";

                if (controlType < inputTags.Count)
                {
                    buttonTag = inputTags[controlType];
                }

                // =============================================================

                string text = buttonTag + " " + action;

                promptText.SetText(text);
            }

            prompt.SetActive(true);
        }

        public void ResetActionPrompt(int controlType, bool forceReset) // called by InputManager.cs and DialogueBox.cs
        {
            if (currentType >= 0 && actionPrompt != null)
            {
                GameObject prompt = actionPrompt.gameObject;

                if (prompt.activeInHierarchy || forceReset)
                {
                    SetPromptText(prompt, currentType);
                }
            }
        }

        public void ConfirmAction() // called by Item.cs
        {
            actionConfirmed = true;

            if (confirmAnimation != null && promptText != null)
            {
                confirmAnimation.AddMessage(promptText.text);
            }
        }

        // =============================================================
        //    Get Methods
        // =============================================================

        public Transform GetActivePlayer() // called by NPC.cs
        {
            return activePlayer;
        }

        public List<Interactable> GetTargetList() // called by InteractionTrigger.cs
        {
            return targetList;
        }

        // =============================================================
        //    Set Methods
        // =============================================================

        public void SetActivePlayer(Transform player) // called by PlayerManager.cs
        {
            activePlayer = player;

            ClearLists();
        }

        // =============================================================
        //    Get Methods - Components
        // =============================================================

        public SceneHandler GetSceneHandler()
        {
            return sceneHandler;
        }
    }
}




