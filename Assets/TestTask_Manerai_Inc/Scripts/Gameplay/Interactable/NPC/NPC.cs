using UnityEngine;

using UnityEngine.Events;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class NPC : Interactable
    {
        public DialogueSet currentDialogueSet;

        public Transform cameraAngle;

        private IEnumerator currentCoroutine;

        protected override void Awake()
        {
            interactionType = 0;

            interactionRange = 3.0f;

            viewDependent = true;

            // =========================================================

            base.Awake();

            sphereCollider.center = Vector3.up * 0.1f;
        }

        protected override void Reset()
        {
            base.Reset();

            sphereCollider.center = Vector3.up * 0.1f;
        }

        protected override void OnValidate()
        {
            interactionType = 0;

            viewDependent = true;

            base.OnValidate();
        }

        public override void StartInteraction()
        {
            PlayerController player = interaction.GetActivePlayer().GetComponent<PlayerController>();

            if (player.GetIsGrounded() && currentDialogueSet != null)
            {
                base.StartInteraction();

                if (cameraAngle != null)
                {
                    StartDialogue();
                }

                else
                {
                    StartCoroutine(IE_StartDialogue(player));
                }
            }
        }

        private void StartDialogue()
        {
            PlayerController player = interaction.GetActivePlayer().GetComponent<PlayerController>();

            SceneHandler sceneHandler = player.GetSceneHandler();

            DialogueBox dialogueBox = GetDialogue(player, sceneHandler);

            // =========================================================

            var thirdPersonCamera = sceneHandler.GetThirdPersonCamera();

            thirdPersonCamera.LockMouseX(true);
            thirdPersonCamera.LockMouseY(true);

            // =========================================================

            GameEvent openDialogue = dialogueBox.GetOpenDialogueEvent();

            openDialogue.RemoveAllListeners();

            openDialogue.AddListener(delegate{OpenDialogue(sceneHandler);});

            openDialogue.AddListener(delegate{thirdPersonCamera.SetCameraParent(cameraAngle);});

            sceneHandler.screenFade.FadeTo(openDialogue);
        }

        private void OpenDialogue(SceneHandler sceneHandler)
        {
            PlayerController player = interaction.GetActivePlayer().GetComponent<PlayerController>();

            LockMouseY(sceneHandler);

            // =========================================================

            DialogueBox dialogueBox = sceneHandler.dialogueBox;

            dialogueBox.SetDialogue(m_name, currentDialogueSet, 0);

            dialogueBox.Open(this, player);
        }

        private DialogueBox GetDialogue(PlayerController player, SceneHandler sceneHandler)
        {
            player.DisableButtonInputs(true);

            player.SetInteractionState(true);

            player.ResetTurnSmoothTime();

            // =========================================================

            if (player.usingRigidbody)
            {
                player.StopRigidbody();
            }

            // =========================================================

            DialogueBox dialogueBox = sceneHandler.dialogueBox;

            dialogueBox.gameObject.SetActive(true);

            dialogueBox.StartDialogue();

            // =========================================================

            AudioSource startDialogue = sceneHandler.audioManager.interactionSounds.startDialogue;

            SceneHandler.PlayAudioSource(startDialogue);

            return dialogueBox;
        }

        private IEnumerator IE_StartDialogue(PlayerController player)
        {
            player.SetTarget(transform);

            FaceTowards(player.transform);

            // =========================================================

            SceneHandler sceneHandler = player.GetSceneHandler();

            DialogueBox dialogueBox = GetDialogue(player, sceneHandler);

            // =========================================================

            var thirdPersonCamera = sceneHandler.GetThirdPersonCamera();

            thirdPersonCamera.ForceTransitionTime(0.2f);

            thirdPersonCamera.SetCameraState(0);

            // =========================================================

            CameraTarget cameraTarget = sceneHandler.cameraTarget;

            cameraTarget.SetSmoothTime(0.2f);
            cameraTarget.LockSmoothTime(true);

            Vector3 targetPosition = (transform.position + player.transform.position) / 2.0f;

            cameraTarget.SetTargetPosition(targetPosition, false);

            LockMouseY(sceneHandler);

            // =========================================================

            yield return SceneHandler.WaitForThirdSecond;

            RootMotionSimulator rms = player.GetRootMotionSimulator();

            while (rms.actionState != 0)
            {
                yield return null;
            }

            // =========================================================

            dialogueBox.SetDialogue(m_name, currentDialogueSet, 0);

            dialogueBox.Open(this, player);
        }

        private void LockMouseY(SceneHandler sceneHandler)
        {
            var thirdPersonCamera = sceneHandler.GetThirdPersonCamera();

            thirdPersonCamera.SetSmoothRotationY(10.0f);

            thirdPersonCamera.LockMouseX(false);
            thirdPersonCamera.LockMouseY(true);

            thirdPersonCamera.SetMouseY(0f);
        }

        private IEnumerator RotateTowards(float targetAngle)
        {
            bool rotating = true;

            float currentAngle = transform.eulerAngles.y;

            float smoothVelocity = 0f;

            while (rotating)
            {
                currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref smoothVelocity, 0.1f);

                if (Mathf.Abs(targetAngle - currentAngle) < 0.1f)
                {
                    currentAngle = targetAngle;

                    rotating = false;
                }

                transform.rotation = Quaternion.Euler(0f, currentAngle, 0f);

                yield return null;
            }
        }

        public void FaceTowards(Transform target)
        {
            Vector3 difference = target.position - transform.position;

            float newAngle = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;

            // =========================================================

            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);

                currentCoroutine = null;
            }

            currentCoroutine = RotateTowards(newAngle);

            StartCoroutine(currentCoroutine);
        }

        public void Greeting()
        {
        }
    }
}









