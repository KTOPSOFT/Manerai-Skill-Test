using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class RootMotionSimulator : MonoBehaviour
    {
        public int actionState;
        public Transform actionList;

        private ActionState currentAction;

        // =========================================================

        private bool useFixedUpdate;

        private float deltaTime;

        private float timer;
        private float timeInterval;

        private int listIndex;
        private int listCount;

        // =========================================================

        private float animatorSpeed;

        private float moveDistance;
        private float scanDistance;

        private float currentDistance;
        private float targetDistance;

        private float currentPoint;
        private float previousPoint;

        private bool simulating;

        private bool pauseSimulation;

        private Vector3 lastGroundedPosition;

        private Transform lastPlatform;

        // =========================================================
        //    Component Dependencies
        // =========================================================

        private Animator animator;
        private CharacterController controller;

        private PlayerController playerController;

        private Rigidbody m_rigidbody;

        private SceneHandler sceneHandler;

        // =========================================================
        //    Animation Parameters
        // =========================================================

        readonly int hashStateType = Animator.StringToHash("StateType");

        readonly int hashIsGrounded = Animator.StringToHash("IsGrounded");

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {
            GetComponents();
        }

        private void GetComponents()
        {
            // Player Components
            animator = GetComponent<Animator>();
            controller = GetComponent<CharacterController>();

            playerController = GetComponent<PlayerController>();
        }

        public void StartTimer() // called by ActionStateHandler.cs
        {
            int stateType = animator.GetInteger(hashStateType);

            int index = actionState - 1;

            if (actionList != null && ValidAction(stateType, index))
            {
                currentAction = actionList.GetChild(stateType).GetChild(index).GetComponent<ActionState>();  

                // =============================================

                if (currentAction.distanceFromOrigin.Count > 0)
                {
                    animator.applyRootMotion = false;

                    animatorSpeed = animator.speed;

                    lastGroundedPosition = transform.localPosition;;

                    lastPlatform = playerController.GetCurrentPlatform();

                    // =============================================

                    timer = 0f;
                    timeInterval = currentAction.timeInterval;

                    listIndex = (int) (currentAction.listCount * currentAction.startTime);
                    listCount = currentAction.listCountOverride;

                    // =============================================

                    int frameCount = currentAction.distanceFromOrigin.Count;

                    if (listCount < 0 || listCount > frameCount)
                        listCount = frameCount;

                    // =============================================

                    currentPoint = currentAction.distanceFromOrigin[listIndex];
                    previousPoint = (listIndex > 0) ? currentAction.distanceFromOrigin[listIndex - 1] : 0f;

                    currentDistance = previousPoint;
                    targetDistance = currentPoint;

                    // =============================================

                    simulating = true;
                }
            }
        }

        private bool ValidAction(int stateType, int index)
        {
            bool validAction = false;

            if (stateType < actionList.childCount)
            {
                Transform stateList = actionList.GetChild(stateType);

                validAction = index < stateList.childCount;
            }

            return validAction;
        }

        private void Update()
        {
            if (simulating && !useFixedUpdate)
            {
                deltaTime = Time.deltaTime;

                Simulate();
            }
        }

        private void FixedUpdate()
        {
            if (simulating && useFixedUpdate)
            {
                deltaTime = Time.fixedDeltaTime;

                Simulate();
            }
        }

        private void Simulate()
        {
            playerController.CheckSlope();

            bool isGrounded = playerController.GetIsGrounded();

            // =============================================

            bool stopTimer = listIndex >= listCount - 1 || !isGrounded;

            if (stopTimer)
            {
                MoveRemainingDistance(isGrounded);

                StopTimer();
            }

            else
            {
                moveDistance = 0f;

                timer += deltaTime * animator.speed;

                if (timer >= timeInterval)
                {
                    int points = (int) (timer / timeInterval);

                    for (int i = 0; i < points; i ++)
                    {
                        MoveTowardsTarget(timeInterval, timeInterval);

                        listIndex ++;

                        if (listIndex > listCount - 1)
                            listIndex = listCount - 1;

                        currentPoint = currentAction.distanceFromOrigin[listIndex];

                        previousPoint = currentAction.distanceFromOrigin[listIndex - 1];

                        timer -= timeInterval;
                    }
                }

                MoveTowardsTarget(timer, timeInterval);

                MoveController(moveDistance);
            }
        }

        public void StopTimer() // called internally and by ActionStateHandler.cs
        {
            if (simulating)
            {
                if (playerController.GetIsGrounded())
                {
                    playerController.CheckForPlatform(transform.position);
                }

                else
                {
                    animator.SetBool(hashIsGrounded, false);

                    playerController.SyncedFall();
                }

                // ==================================================

                playerController.RevertStepOffset();

                simulating = false;
            }
        }

        private void MoveTowardsTarget(float time, float timeInterval)
        {
            float progress = time / timeInterval;

            targetDistance = progress * (currentPoint - previousPoint) + previousPoint;

            float distance = targetDistance - currentDistance;

            moveDistance += distance;

            currentDistance = targetDistance;
        }

        private void MoveController(float distance)
        {   
            float targetAngle = currentAction.moveDirection + playerController.GetTargetAngle();

            Vector3 moveDirection = Quaternion.Euler(Vector3.up * targetAngle) * Vector3.forward;

            MoveController(moveDirection, distance * currentAction.moveMultiplier);
        }

        private void MoveController(Vector3 moveDirection, float distance)
        {
            if (!playerController.GetLockMovement())
            {
                if (!playerController.GetIsGrounded())
                {
                    SetPlayerOnGround(distance);
                }

                OnPlayerGrounded(moveDirection, distance);
            }
        }

        private void OnPlayerGrounded(Vector3 moveDirection, float distance)
        {
            animator.speed = pauseSimulation ? 0f : animatorSpeed;

            lastPlatform = playerController.GetCurrentPlatform(); // must be cached before playerController.CheckForPlatform()

            // =========================================================

            float sign = Mathf.Sign(distance);

            float targetDistance = (scanDistance + 0.1f) * sign;

            if (scanDistance <= 0f)
                targetDistance = distance;

            bool validMove = playerController.ValidControllerMove(transform.position, moveDirection, targetDistance);

            // =========================================================

            if (validMove)
            {
                MoveCharacterController(moveDirection, distance);
            }

            else if (!playerController.GetOnLedge())
            {
                SetPlayerOnGround(distance);
            }
        }

        private void MoveCharacterController(Vector3 moveDirection, float distance)
        {   
            playerController.SetOnLedge(false);

            // =========================================================

            if (useFixedUpdate)
            {
                float moveSpeed = distance / deltaTime;

                float yRotation = playerController.GetTargetAngle() + currentAction.moveDirection;

                playerController.MoveRigidbody(moveDirection, moveSpeed, distance, yRotation);
            }

            else
            {
                Vector3 movement = moveDirection * distance;

                float vertical = playerController.GetVerticalMoveDistance(distance);

                controller.Move(movement + Vector3.down * vertical);

                playerController.RevertStepOffset();
            }

            // =========================================================

            scanDistance = -1.0f;

            lastGroundedPosition =  transform.localPosition;
        }

        private void SetPlayerOnGround(float distance)
        {
            if (useFixedUpdate)
            {
                playerController.StopRigidbody();
            }

            scanDistance = Mathf.Abs(distance);

            // =========================================================

            playerController.SetPlatform(lastPlatform); // must be set before calling playerController.SetTransform()

            playerController.SetOnLedge(true);

            // =========================================================

            bool setLocalPosition = true;

            playerController.SetTransform(lastGroundedPosition, transform.eulerAngles, setLocalPosition);

            // =========================================================

            bool meleeAction = animator.GetInteger(hashStateType) == 1;

            bool dodgeRoll = !meleeAction && actionState == 1;
            bool sidestep = !meleeAction && (actionState == 2 || actionState == 3);

            if (dodgeRoll || sidestep)
                animator.speed = animatorSpeed * 1.5f;
        }

        private void MoveRemainingDistance(bool isGrounded)
        {
            if (isGrounded)
            {
                targetDistance = currentAction.distanceFromOrigin[listCount - 1];

                MoveController(targetDistance - currentDistance);

                if (useFixedUpdate && gameObject.activeInHierarchy)
                {
                    StartCoroutine(StopRigidbody());
                }
            }
        }

        private IEnumerator StopRigidbody()
        {
            yield return SceneHandler.WaitForFixedUpdate;

            playerController.StopRigidbody();
        }

        public void PauseSimulation(bool value)
        {
            pauseSimulation = value;
        }

        public void SetComponents(SceneHandler sceneHandler, Rigidbody rb) // called by PlayerController.cs
        {
            this.sceneHandler = sceneHandler;

            m_rigidbody = rb;
        }

        public void UseFixedUpdate(bool value) // called by PlayerController.cs
        {
            useFixedUpdate = value;
        }

        public void ResetScanDistance() // called by ActionStateHandler.cs
        {
            scanDistance = -1.0f;
        }

        // =============================================================
        //    Get Methods - Variables
        // =============================================================

        public bool GetSimulating() // called by PlayerController.cs
        {
            return simulating;
        }
    }
}




