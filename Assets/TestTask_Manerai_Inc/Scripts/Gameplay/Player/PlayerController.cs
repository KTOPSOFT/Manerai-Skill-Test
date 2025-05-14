using UnityEngine;
using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

using TMPro;

namespace YukiOno.SkillTest
{
    public class PlayerController : MonoBehaviour
    {
        private float deltaTime;

        // =========================================================
        //    Player Variables
        // =========================================================

        public string m_name = "Player";

        private bool activePlayer;

        private int playerIndex;

        private Vector3 spawnPosition;
        private Vector3 spawnRotation;

        // =========================================================
        //    Component Dependencies
        // =========================================================

        private Animator animator;

        private CharacterController controller;

        private Rigidbody m_rigidbody;
        private CapsuleCollider capsuleCollider;

        private Target target;
        private Attributes attributes;

        private MeleeAttack meleeAttack;
        private RootMotionSimulator rms;

        private Footsteps footsteps;

        private Transform cameraTransform;

        private SceneHandler sceneHandler;
        private InputManager inputManager;

        private PlayerManager playerManager;

        // =========================================================
        //    Input Variables
        // =========================================================

        private Vector2 moveInput;

        private Vector3 direction;

        private float inputMagnitude;

        private float currentInput;
        private float targetInput;

        private float currentVelocity;
        private float targetVelocity;

        private float inputReference;
        private float velocityReference;

        private bool inputDetected;

        private float inputSmooth;

        private bool sprintInput;

        private bool gamePaused;

        private float groundedTargetInput;

        // =========================================================
        //    Movement Variables
        // =========================================================

        public float walkSpeed = 2.0f;
        public float runSpeed = 4.0f;
        public float sprintSpeed = 6.0f;

        public bool canSprint = true;

        private bool isCrouching;
        private bool isSprinting;

        private float walkDelay;

        private static float defaultTurnSmoothTime = 0.1f;

        private float turnSmoothTime;
        private float turnSmoothReference;

        private float moveDistance;

        private Vector3 moveDirection;
        private Vector3 externalForces;

        private float currentAngle;
        private float targetAngle;

        private bool disableMoveInput;
        private bool disableButtonInputs;

        private bool lockMovement;
        private bool lockRotation;

        private bool inSimulationState;

        private bool validMove;
        private bool crouchingMovement;

        private Vector3 previousPosition;

        private float stepOffset;

        private float stepOffsetScale = 1.0f;

        private Transform currentPlatform;

        // =========================================================
        //    Vertical Variables
        // =========================================================

        public float jumpHeight = 1.2f;

        private float fallSpeed;

        private bool isJumping;
        private bool isGrounded;

        private bool onLedge;
        private bool airborne;

        private bool onPlatform;

        private bool skipLandingTrigger;

        private Transform defaultParent;

        private static float gravity = -21.25f;

        private static float rayLength = 0.2f;

        private static float groundedFallSpeed = -2.0f;
        private static float terminalVelocity = -50.0f;

        [Range(0f, 1.0f)]
        public float jumpVoiceChance = 0.33f;

        // =========================================================
        //    Rigidbody Variables
        // =========================================================

        private bool useRigidbody;

        private bool isColliding;

        private PhysicMaterial physicMaterial;

        private float capsuleRadius;
        private float capsuleHeight;

        private RaycastHit rayHit;

        private bool groundHit;
        private bool platformFall;

        private float groundSlope;

        private Vector3 groundNormal;

        private Collider[] colliders = new Collider[10];

        public bool usingRigidbody { get { return useRigidbody; } }

        private Transform freeTransform;

        // =========================================================
        //    State Handler Variables
        // =========================================================

        private bool movementEnabled;
        private bool rotationEnabled;

        private bool autoRotate;

        private bool movementState;

        private int eventState = -1;

        // =========================================================
        //    Stamina Variables
        // =========================================================

        private bool isTired;

        public float maxStamina = 100.0f;

        private static float defaultStaminaRecovery = 40.0f;
        private static float sprintStaminaCost = 8.0f;

        private float staminaRecovery;

        private float currentStamina;
        private float currentStaminaRecoveryDelay;

        // =========================================================
        //    Combat Variables
        // =========================================================

        private Collider[] targets = new Collider[10];

        private float hitSoundTime;

        private float actionModifier = 1.0f;

        // =========================================================
        //    Other Components
        // =========================================================

        [Header("Components")]

        public AudioPlayer voiceAudioPlayer;

        private AudioList voiceAudioList;

        public GameObject interactionTriggers;

        public CollisionTrigger collisionTrigger;

        public CanvasGroup playerHUD;

        public Meter staminaMeter;

        // =========================================================
        //    Animation Parameters
        // =========================================================

        readonly int hashStateTime = Animator.StringToHash("StateTime");
        readonly int hashInputMagnitude = Animator.StringToHash("InputMagnitude");

        readonly int hashFallSpeed = Animator.StringToHash("FallSpeed");

        readonly int hashNetworkState = Animator.StringToHash("NetworkState");

        readonly int hashIsGrounded = Animator.StringToHash("IsGrounded");
        readonly int hashInputDetected = Animator.StringToHash("InputDetected");

        readonly int hashJump = Animator.StringToHash("Jump");
        readonly int hashLand = Animator.StringToHash("Land");

        readonly int hashNetworkTrigger = Animator.StringToHash("NetworkTrigger");

        // =========================================================
        //    Arrays and Lists
        // =========================================================

        private Vector3[] hitValues = new Vector3[8];

        private float[] jumpValues = new float[3];
        private float[] damageValues = new float[3];

        private List<EventCollider> eventColliders = new List<EventCollider>();

        // =========================================================
        //    Layer Cache
        // =========================================================

        private int playerLayer;

        private int enemyLayer;

        private bool isPlayerObject;

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {
            GetComponents();

            InitializeComponents();

            InitializeLayers();

            // =========================================================

            isGrounded = true;

            stepOffset = controller.stepOffset;

            defaultParent = transform.parent;

            turnSmoothTime = defaultTurnSmoothTime;

            staminaRecovery = defaultStaminaRecovery;

            currentStamina = maxStamina;

            currentPlatform  = defaultParent;

            actionModifier = 1.0f;

            // =========================================================

            currentAngle = transform.eulerAngles.y;

            targetAngle = currentAngle;

            // =========================================================

            InitializeRigidbody();
        }

        private void GetComponents()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            // Player Components
            animator = GetComponent<Animator>();

            controller = GetComponent<CharacterController>();

            m_rigidbody = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();

            target = GetComponent<Target>();
            attributes = GetComponent<Attributes>();

            meleeAttack = GetComponent<MeleeAttack>();
            rms = GetComponent<RootMotionSimulator>();

            footsteps = GetComponent<Footsteps>();

            // Scene Components
            cameraTransform = Camera.main.transform;

            sceneHandler = elements.GetComponent<SceneHandler>();
            inputManager = elements.GetComponent<InputManager>();

            playerManager = elements.GetComponent<PlayerManager>();

            networkManager = sceneHandler.networkManager;
        }

        private void InitializeComponents()
        {
            meleeAttack.Initialize();

            if (voiceAudioPlayer != null)
                voiceAudioList = voiceAudioPlayer.GetComponent<AudioList>();

            if (footsteps != null)
                footsteps.Initialize(this);

            if (collisionTrigger != null)
                collisionTrigger.Initialize(this, controller);
        }

        private void InitializeLayers()
        {
            playerLayer = sceneHandler.GetPlayerLayer();

            enemyLayer = sceneHandler.GetEnemyLayer();

            isPlayerObject = (gameObject.layer == playerLayer);
        }

        private void OnEnable() // public override void OnStartClient()
        {
            // base.OnStartClient();

            m_soloInstance = networkManager == null || networkManager.maxConnections == 1;

            AddPlayer();
        }

        private void OnDisable()
        {
            RemovePlayer();
        }

        private void OnAnimatorMove()
        {
            // Even with no lines in this method, having it exist in this class allows the animator to use Root Transform Position (XZ)
        }

        private void Update()
        {
            if (!gamePaused)
            {
                deltaTime = Time.deltaTime;

                // =========================================================

                if (activePlayer)
                {
                    HandleInput();
                }

                StaminaUpdate();

                // =========================================================

                CheckCollisions();

                MovementUpdate();

                MoveController();

                AnimatorUpdate();
            }
        }

        private void StaminaUpdate()   
        {
            if (currentStaminaRecoveryDelay > 0f)
            {
                if (isGrounded && !isSprinting)
                {
                    currentStaminaRecoveryDelay -= deltaTime;
                }
            }

            else if (currentStamina < maxStamina)
            {
                currentStamina += staminaRecovery * deltaTime;

                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;

                    if (isTired)
                    {
                        staminaRecovery = defaultStaminaRecovery;

                        isTired = false;
                    }
                }
            }

            // =========================================================

            if (!isGrounded && groundedTargetInput > 1.01f)
            {
                UseStamina(sprintStaminaCost * deltaTime, 0.5f);
            }

            // =========================================================

            if (staminaMeter != null)
            {
                float value = 1.0f;

                if (currentStamina < maxStamina)
                    value = currentStamina / maxStamina;

                staminaMeter.SetValue(value);
            }
        }

        private void HandleInput()
        {
            if (activePlayer)
            {
                var keyboard = Keyboard.current;

                // ==========================================================

                moveInput = Vector2.zero;

                if (!disableMoveInput)
                {
                    bool mouseInUse = inputManager.GetMouseInUse();

                    if (mouseInUse)
                        moveInput = GetMoveInput(keyboard);

                    else if (Gamepad.current != null)
                        moveInput = new Vector2(Gamepad.current.leftStick.x.ReadValue(), Gamepad.current.leftStick.y.ReadValue());
                }

                inputDetected = moveInput.sqrMagnitude > 0.01f;

                // ==========================================================

                if (inputDetected != previousInputDetected)
                {
                    previousInputDetected = inputDetected;

                    if (isLocalPlayer)
                    {
                        CmdSetInputDetected(inputDetected);
                    }
                }

                // ==========================================================

                sprintInput = false;

                isSprinting = false;

                if (!disableButtonInputs)
                {   
                    bool spaceKeyDown = keyboard.spaceKey.wasPressedThisFrame;

                    bool shiftKeyHeld = keyboard.leftShiftKey.isPressed;

                    // ==========================================================

                    if (spaceKeyDown || GamepadButtonDown.South())
                    {
                        JumpInput();
                    }

                    if (shiftKeyHeld || GamepadButtonHeld.RightShoulder())
                    {
                        sprintInput = true;

                        SprintInput();
                    }
                }
            }
        }

        private static Vector2 GetMoveInput(Keyboard keyboard)
        {
            float x = 0f;
            float y = 0f;

            if (keyboard.wKey.isPressed)
                y ++;

            if (keyboard.aKey.isPressed)
                x --;

            if (keyboard.sKey.isPressed)
                y --;

            if (keyboard.dKey.isPressed)
                x ++;

            return new Vector2(x, y);
        }

        private void JumpInput()
        {
            bool ceilingHit = CeilingHit();

            if (ceilingHit)
            {
                airborne = true;

                Land(true);
            }

            else
            {
                meleeAttack.SetTrigger(0);

                bool validInput = meleeAttack.ValidInput();

                if (validInput && !isCrouching && !airborne)
                {
                    animator.ResetTrigger(hashJump);

                    Jump();
                }
            }
        }

        private void SprintInput()
        {
            isSprinting = canSprint && movementState && !isTired;

            if (isSprinting)
            {
                walkDelay = 0.5f;

                UseStamina(sprintStaminaCost * deltaTime, 0.5f);
            }
        }

        public void UseStamina(float value)
        {
            if (staminaMeter != null)
            {
                staminaMeter.FlashSubBar(value, maxStamina);
            }

            UseStamina(value, 2.0f);
        }

        public void UseStamina(float value, float recoveryDelay)
        {
            currentStamina -= value;

            currentStaminaRecoveryDelay = recoveryDelay;

            if (currentStamina <= 0f && isGrounded)
            {
                currentStamina = 0f;

                staminaRecovery = defaultStaminaRecovery / 2.0f;

                isTired = true;

                if (staminaMeter != null)
                {
                    staminaMeter.ShowRegenBar(true);
                }
            }
        }

        public void ToggleRigidbody(bool value) // called internally and by PlayerManager.cs
        {
            if (value != useRigidbody)
            {
                useRigidbody = value;

                // ==========================================================

                m_rigidbody.isKinematic = !value;

                controller.enabled = !value;

                capsuleCollider.enabled = value;

                rms.UseFixedUpdate(value);

                // ==========================================================

                if (!value) // move character controller to update controller.isGrounded
                {
                    controller.Move(Vector3.zero);

                    if (!controller.isGrounded && !airborne)
                    {
                        SnapToGround(-1.0f);
                    }
                }

                // ==========================================================

                if (activePlayer)
                {
                    sceneHandler.thirdPersonCamera.UseFixedUpdate(value);

                    sceneHandler.cameraTarget.UseFixedUpdate(value);
                }
            }
        }

        private void CheckCollisions()
        {
            float padding = 0.2f;

            float radius = capsuleRadius + padding;

            Vector3 end = transform.position + Vector3.up * (capsuleHeight - radius + padding);

            // ==========================================================

            bool onPhysicsLayer = false;

            if (isPlayerObject)
            {
                Vector3 start = transform.position + Vector3.up * (radius + 0.02f);

                onPhysicsLayer = Physics.CheckCapsule(start, end, radius, sceneHandler.physicsLayerMask);

                isColliding = onPhysicsLayer;

                // ==========================================================

                CheckStepPrevention(start, radius);
            }

            else
            {
                onPhysicsLayer = isGrounded;

                if (!onPhysicsLayer)
                {
                    Vector3 start = transform.position + Vector3.up * radius;

                    LayerMask layerMask = sceneHandler.physicsLayerMask | (1 << playerLayer);

                    int arrayLength = Physics.OverlapCapsuleNonAlloc(start, end, radius, colliders, layerMask);

                    for (int i = 0; i < arrayLength; i ++)
                    {
                        GameObject target = colliders[i].gameObject;

                        if (target.layer != enemyLayer || target.transform != transform)
                        {
                            onPhysicsLayer = true;

                            break;
                        }
                    }
                }
            }

            // ==========================================================

            ToggleRigidbody(onPhysicsLayer || platformFall);
        }

        private void CheckStepPrevention(Vector3 start, float radius)
        {
            bool stepPrevention = Physics.CheckSphere(start, radius, sceneHandler.stepPrevention);

            float newScale = stepPrevention ? 0f : 1.0f;

            if (newScale != stepOffsetScale)
            {
                stepOffsetScale = newScale;

                controller.stepOffset = stepOffset * stepOffsetScale;
            }
        }

        private void MovementUpdate()
        {
            direction = new Vector3(moveInput.x, 0f, moveInput.y);

            inputMagnitude = direction.magnitude;

            if (inputMagnitude > 1.0f)
            {
                direction = direction.normalized;

                inputMagnitude = 1.0f;
            }

            // ==========================================================

            if (activePlayer && !autoRotate)
            {
                if (!lockRotation && rotationEnabled)
                {
                    if (inputDetected)
                        targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;

                    else
                        targetAngle = currentAngle;

                    // ==========================================================

                    if (movementState && inputDetected)
                    {
                        if (targetAngle != previousTargetAngle)
                        {
                            previousTargetAngle = targetAngle;

                            if (isLocalPlayer)
                            {
                                CmdSetTargetAngle(targetAngle);
                            }
                        }
                    }

                }
            }

            autoRotate = false;

            // ==========================================================

            float turnSmooth = isGrounded ? turnSmoothTime : 0.05f;

            if (lockRotation)
            {
                UpdateRotation(turnSmooth);
            }

            else
            {
                if (movementEnabled)
                {
                    if (inputDetected)
                    {   
                        UpdateRotation(turnSmooth);
                    }
                }

                else if (rotationEnabled)
                {
                    rotationEnabled = false;
                }

                else
                {                    
                    UpdateRotation(turnSmooth);
                }
            }

            // ==========================================================

            if (walkDelay > 0f && !isSprinting)
            {
                walkDelay -= deltaTime;

                if (walkDelay < 0f)
                    walkDelay = 0f;
            }

            // ==========================================================

            DetermineMoveSpeed();

            HorizontalUpdate();
            VerticalUpdate();
        }

        private void UpdateRotation(float turnSmooth)
        {
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref turnSmoothReference, turnSmooth);

            Quaternion rotation = Quaternion.Euler(0f, currentAngle, 0f);

            // ==========================================================

            if (useRigidbody)
            {
                m_rigidbody.rotation = rotation;
            }

            else
            {
                transform.rotation = rotation;
            }
        }

        private void DetermineMoveSpeed()
        {
            if (!lockMovement && inputDetected)
            {
                if (localEvent)
                {
                    if (isCrouching)
                    {
                        targetInput = 1.0f;
                        targetVelocity = walkSpeed * 0.75f;
                    }

                    else if (isSprinting)
                    {
                        targetInput = 1.5f;
                        targetVelocity = sprintSpeed;
                    }

                    else if (walkDelay <= 0f && inputMagnitude < 0.625f)
                    {
                        targetInput = 0.5f;
                        targetVelocity = walkSpeed;
                    }

                    else
                    {
                        targetInput = 1.0f;
                        targetVelocity = runSpeed;
                    }
                }
            }

            else
            {
                currentVelocity = isGrounded ? 0f : currentVelocity;

                targetInput = 0f;
                targetVelocity = 0f;
            }

            // =====================================================================

            if (targetInput != previousTargetInput)
            {
                previousTargetInput = targetInput;

                if (isLocalPlayer)
                {
                    CmdSetTargetInput(targetInput);
                }
            }

            if (targetVelocity != previousTargetVelocity)
            {
                previousTargetVelocity = targetVelocity;

                if (isLocalPlayer)
                {
                    CmdSetTargetVelocity(targetVelocity);
                }
            }

            // =====================================================================

            if (inputDetected && currentInput > targetInput)
                inputSmooth = 0.30f;

            else
                inputSmooth = 0.15f;

            currentInput = Mathf.SmoothDamp(currentInput, targetInput, ref inputReference, inputSmooth);

            currentVelocity = isGrounded ? Mathf.SmoothDamp(currentVelocity, targetVelocity, ref velocityReference, 0.15f) : currentVelocity;
        }

        private void HorizontalUpdate()
        {
            if (!inSimulationState)
            {
                if (isGrounded)
                {
                    moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                }

                // =====================================================================

                validMove = true;

                crouchingMovement = isCrouching && inputDetected;

                float targetMoveDistance = currentVelocity * deltaTime;

                if (crouchingMovement)
                {   
                    validMove = ValidControllerMove(transform.position, moveDirection, targetMoveDistance);
                }

                // =====================================================================

                if (validMove)
                {
                    moveDistance = targetMoveDistance;

                    previousPosition = transform.position;

                    onLedge = false;
                }
            }
        }

        private void VerticalUpdate()
        {
            bool previouslyGrounded = isGrounded;

            // ==================================================

            CheckForGround();

            if (useRigidbody && previouslyGrounded && !isGrounded && isPlayerObject)
            {   
                SnapToGround(-1.0f);
            }

            CheckSlope();

            // ==================================================

            if (!inSimulationState)
            {
                if (!isGrounded)
                {
                    CheckForCeiling();

                    if (fallSpeed > terminalVelocity)
                        fallSpeed += gravity * deltaTime;

                    else
                        fallSpeed = terminalVelocity;

                    // ==================================================

                    if (!airborne)
                    {
                        groundedTargetInput = targetInput;

                        SyncedFall();

                        // ==================================================

                        if (inSimulationState)
                            rms.StopTimer();
                    }
                }
            }
        }

        private bool IsGrounded(float multiplier)
        {
            return IsGrounded(transform.position, multiplier, false);
        }

        public bool IsGrounded(Vector3 startPosition, float multiplier, bool forceCheck)
        {
            bool isGrounded = false;

            bool previouslyGrounded = this.isGrounded;

            if (previouslyGrounded || (fallSpeed <= 0f && !onPlatform) || forceCheck)
            {
                float radius = capsuleRadius * multiplier;

                float offset = useRigidbody ? 0.05f : 0.025f;

                Vector3 spherePosition = startPosition + Vector3.up * (radius - offset);

                isGrounded = OnGroundLayer(spherePosition, radius, sceneHandler.groundLayerMask);
            }

            return isGrounded;
        }

        private void CheckForGround()
        {
            bool grounded = false;

            if (isGrounded)
            {
                grounded = IsGrounded(1.0f);
            }

            else if (!onPlatform)
            {
                if (useRigidbody)
                    grounded = IsGrounded(0.99f); // must be smaller than capsule radius to avoid landing on internal geometry

                else
                    grounded = controller.isGrounded && IsGrounded(1.0f);
            }

            // ==================================================

            if (isJumping)
            {
                isJumping = grounded;
            }

            else
            {
                bool useRayHit = isGrounded;

                SetIsGrounded(grounded, useRayHit);
            }

            // ==================================================

            // if (!rms.GetSimulating() && !crouchingMovement)

            if (!rms.GetSimulating())
            {
                CheckForPlatform(transform.position);
            }
        }

        private static bool OnGroundLayer(Vector3 position, float radius, LayerMask layerMask)
        {
            bool onGroundLayer = Physics.CheckSphere(position, radius, layerMask, QueryTriggerInteraction.Ignore);

            return onGroundLayer;
        }

        private void CheckForCeiling()
        {
            if (fallSpeed > 0f)
            {
                bool ceilingHit = CeilingHit();

                if (ceilingHit)
                {
                    fallSpeed = 0f;
                }
            }
        }

        private bool CeilingHit()
        {
            bool ceilingHit = false;

            // ==================================================

            float radius = controller.radius * 0.9f;

            float controllerHeight = controller.center.y * 2.0f;

            Vector3 spherePosition = transform.position + Vector3.up * controllerHeight - Vector3.up * (radius - 0.01f);

            // ==================================================

            ceilingHit = Physics.CheckSphere(spherePosition, radius, sceneHandler.groundLayerMask, QueryTriggerInteraction.Ignore);

            return ceilingHit;
        }

        public bool CheckForPlatform(Vector3 origin) // called internally and by RootMotionSimulator.cs
        {
            bool groundCheck = false;

            if (isGrounded && !isJumping)
            {
                RaycastHit hit;

                Ray ray = new Ray(origin + Vector3.up * 1.0f, Vector3.down);

                groundCheck = Physics.Raycast(ray, out hit, rayLength + 1.0f, sceneHandler.groundLayerMask);

                // ==================================================

                if (groundCheck)
                {
                    Transform platform = hit.transform;

                    int layer = platform.gameObject.layer;

                    if (layer == sceneHandler.GetPlatformLayer())
                    {
                        if (currentPlatform != platform)
                        {   
                            Land(origin, true);
                        }
                    }

                    else if (currentPlatform != defaultParent)
                    {   
                        Land(origin, true);
                    }
                }
            }

            return groundCheck;
        }

        private void SetIsGrounded(bool value, bool useRayHit)
        {
            isGrounded = value;

            if (value)
            {
                if (airborne)
                {
                    Land(useRayHit); // must be called before modifying "airborne" to determine whether or not to use landing trigger

                    onLedge = false;

                    airborne = false;
                }

                fallSpeed = groundedFallSpeed;
            }
        }

        public bool ValidControllerMove(Vector3 startPosition, Vector3 moveDirection, float distance) // called internally and by RootMotionSimulator.cs
        {   
            Vector3 targetPosition = startPosition + moveDirection * distance;

            bool groundCheck = CheckForPlatform(targetPosition);

            if (!groundCheck) // moveDirection must already be normalized for this to work correctly
            {
                targetPosition = targetPosition + moveDirection * (controller.radius + 0.1f);

                groundCheck = CheckForPlatform(targetPosition);

                // ==================================================

                if (groundCheck && stepOffset < 0.3f)
                {
                    controller.stepOffset = 0.3f * stepOffsetScale;
                }

            }

            return groundCheck;
        }

        private void MoveController()
        {
            bool validConditions = inputDetected || !isGrounded;

            if (validConditions && validMove && !rms.GetSimulating())
            {
                if (useRigidbody)
                {
                    float moveSpeed = inSimulationState ? 0f : currentVelocity;

                    MoveRigidbody(moveDirection, moveSpeed, moveDistance, targetAngle);
                }

                else
                {
                    Vector3 horizontal = inSimulationState ? Vector3.zero : moveDirection * moveDistance;

                    Vector3 vertical = Vector3.up * fallSpeed * deltaTime + Vector3.down * GetVerticalMoveDistance(moveDistance);

                    Vector3 external = externalForces * deltaTime;

                    // ==================================================

                    bool previouslyGrounded = isGrounded && !airborne;

                    controller.Move(horizontal + vertical + external);

                    if (previouslyGrounded && !controller.isGrounded)
                    {
                        float offset = previousPosition.y - transform.position.y;

                        transform.position = transform.position + Vector3.up * offset; // compensate for large drop on the same frame
                    }
                }

                // ==================================================

                if (crouchingMovement)
                {
                    RevertStepOffset();
                }
            }

            else if (useRigidbody && !inSimulationState)
            {
                StopRigidbody();
            }
        }

        private void SnapToGround(float multiplier)
        {   
            RaycastHit hit;

            Vector3 position = transform.position;

            if (multiplier > 0f)
            {
                float radius = controller.radius * multiplier;

                Vector3 spherePosition = position + Vector3.up * (radius + 1.0f);

                isGrounded = Physics.SphereCast(spherePosition, radius, Vector3.down, out hit, rayLength + 1.0f, sceneHandler.groundLayerMask);
            }

            else
            {
                Vector3 rayStart = position + Vector3.up * 1.0f;

                isGrounded = Physics.Raycast(rayStart, Vector3.down, out hit, rayLength + 1.0f, sceneHandler.groundLayerMask);
            }

            // ==================================================

            if (isGrounded)
            {
                float x = position.x;
                float z = position.z;

                Vector3 newPosition = new Vector3(x, hit.point.y, z);

                transform.position = newPosition;
            }
        }

        private void AnimatorUpdate()
        {
            float stateTime = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1.0f);

            animator.SetFloat(hashStateTime, stateTime);
            animator.SetFloat(hashInputMagnitude, currentInput);
            animator.SetFloat(hashFallSpeed, fallSpeed);

            animator.SetBool(hashInputDetected, inputDetected);

            if (!inSimulationState)
            {
                animator.SetBool(hashIsGrounded, isGrounded);
            }
        }

        public float GetVerticalMoveDistance(float moveDistance) // called internally and by RootMotionSimulator.cs
        {
            float distance = 0f; // this distance is used to keep the character controller snapped to the ground

            if (isGrounded)
            {
                distance = Mathf.Abs(moveDistance); // assuming a max slope of 45 degrees, the maximum vertical distance is equal to the move distance

                distance *= 1.05f; // multiply by a small value just for good measure

                if (distance < 0.15f)
                    distance = 0.15f;
            }

            return distance;
        }

        private void Jump()
        {
            groundedTargetInput = targetInput;

            // ==================================================

            Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);

            float targetAngle = transform.eulerAngles.y;

            if (inputDetected)
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;

            // ==================================================

            float moveSpeed = targetVelocity;

            float jumpAngle = targetAngle;

            Jump(moveSpeed, jumpAngle, targetAngle);
        }

        private void Jump(float moveSpeed, float jumpAngle)
        {
            float targetAngle = jumpAngle;

            Jump(moveSpeed, jumpAngle, targetAngle);
        }

        private void Jump(float moveSpeed, float jumpAngle, float targetAngle)
        {
            SyncedJump(moveSpeed, jumpAngle, targetAngle);
        }

        public void JumpEvent(Vector3 position, Vector3 rotation, float[] values, bool useTrigger)
        {
            float targetAngle = values[0];
            float moveSpeed = values[1];

            float jumpAngle = values[2];

            // ==================================================

            rms.StopTimer();

            inSimulationState = false;

            // jump.Play();

            // ==================================================

            airborne = true;
            onLedge = false;

            isJumping = true;
            isGrounded = false;

            currentVelocity = moveSpeed;
            targetVelocity = moveSpeed;

            onPlatform = false;

            skipLandingTrigger = false;

            currentPlatform = defaultParent;

            // ==================================================

            animator.ResetTrigger(hashLand);

            if (useTrigger)
                animator.SetTrigger(hashJump);

            // ==================================================

            Vector3 moveDirection = Quaternion.Euler(0f, jumpAngle, 0f) * Vector3.forward;

            float fallSpeed = Mathf.Sqrt(jumpHeight * -2.0f * gravity); // velocity needed to reach desired height

            // ==================================================

            bool useLocalPosition = false;

            SetPlayerTransform(position, rotation, targetAngle, moveSpeed, moveDirection, fallSpeed, useLocalPosition);

            SetPlatform(defaultParent);

            // ==================================================

            if (voiceAudioList != null)
            {
                float randomChance = Random.Range(0, 1.0f);

                if (randomChance <= jumpVoiceChance)
                {
                    bool playOneShot = false;

                    voiceAudioPlayer.PlayFromAudioList(voiceAudioList, this, playOneShot);
                }
            }
        }

        public void Land(bool useRayHit) // called by PlayerManager.cs
        {
            Land(transform.position, useRayHit);
        }

        private void Land(Vector3 startPosition, bool useRayHit)
        {
            Transform newPlatform = defaultParent;

            Vector3 worldPosition = startPosition;

            // ==================================================

            RaycastHit sphereHit;

            Ray ray = new Ray(startPosition + Vector3.up * 1.0f, Vector3.down);

            bool grounded = Physics.SphereCast(ray, controller.radius, out sphereHit, rayLength + 1.0f, sceneHandler.groundLayerMask);

            if (grounded)
            {
                int colliderLayer = sphereHit.collider.gameObject.layer;

                if (colliderLayer == sceneHandler.GetPlatformLayer())
                {
                    newPlatform = sphereHit.transform;
                }

                // ==================================================

                if (useRayHit)
                {
                    RaycastHit rayHit;

                    if (Physics.Raycast(ray, out rayHit, rayLength + 1.0f, sceneHandler.groundLayerMask))
                        worldPosition = rayHit.point;

                    else
                        worldPosition = sphereHit.point;
                }
            }

            // ==================================================

            float targetVelocity = this.targetVelocity;

            float targetInput = this.targetInput;

            if (sprintInput)
            {
                targetVelocity = sprintSpeed;

                targetInput = 1.5f;
            }

            // ==================================================

            if (newPlatform == defaultParent)
            {
                SyncedLand(worldPosition, transform.eulerAngles, targetVelocity, targetInput, airborne);
            }

            else
            {
                Vector3 newPosition = transform.localPosition;

                if (currentPlatform != newPlatform)
                {
                    newPosition = RelativePosition(newPlatform, worldPosition);
                }

                Platform platform = newPlatform.parent.GetComponent<Platform>();

                int siblingIndex = newPlatform.GetSiblingIndex();

                SyncedPlatformLand(newPosition, transform.eulerAngles, targetVelocity, targetInput, airborne, platform, siblingIndex);
            }
        }

        private void LandEvent(Vector3 newPosition, Vector3 newRotation, float moveSpeed, float moveInput, bool isAirborne)
        {   
            onPlatform = false;

            platformFall = false;

            // ==================================================

            LandEvent(newPosition, newRotation, moveSpeed, moveInput, isAirborne, true, false);
        }

        private void PlatformLandEvent(Vector3 newPosition, Vector3 newRotation, float moveSpeed, float moveInput, bool isAirborne, Platform platform, int siblingIndex)
        {   
            onPlatform = true;

            platformFall = false;

            // ==================================================

            Transform newPlatform = platform.transform.GetChild(siblingIndex);

            SetPlatform(newPlatform);

            // ==================================================

            bool useLocalPosition = true;

            LandEvent(newPosition, newRotation, moveSpeed, moveInput, isAirborne, false, useLocalPosition);
        }

        private void LandEvent(Vector3 newPosition, Vector3 newRotation, float moveSpeed, float moveInput, bool isAirborne, bool revertPlatform, bool useLocalPosition)
        {
            isGrounded = true;

            animator.SetBool(hashIsGrounded, isGrounded);

            // ==================================================

            currentVelocity = moveSpeed;

            currentInput = moveInput;

            animator.SetFloat(hashInputMagnitude, currentInput);

            // ==================================================

            if (isAirborne)
            {
                if (skipLandingTrigger)
                    animator.CrossFade("Idle", 0.01f);

                else
                    animator.SetTrigger(hashLand);
            }

            walkDelay = 0f;

            skipLandingTrigger = false;

            // ==================================================

            if (useLocalPosition)
                SetLocalTransform(newPosition, newRotation);

            else
                SetTransform(newPosition, newRotation);

            // ==================================================

            if (revertPlatform)
            {
                RevertPlatform();
            }

            // ==================================================

            if (useRigidbody && groundHit)
            {
                SnapToGround(capsuleRadius * 0.99f);
            }
        }

        private void FallEvent(Vector3 localPosition, Vector3 newRotation)
        {
            airborne = true;

            // ==================================================

            onPlatform = onPlatform && IsOnPlatform(currentPlatform, 1.1f);

            platformFall = onPlatform;

            // ==================================================

            SetLocalTransform(localPosition, newRotation);

            if (gameObject.activeInHierarchy && currentPlatform != defaultParent)
            {
                StartCoroutine(PlatformFallEvent(currentPlatform));
            }
        }

        private IEnumerator PlatformFallEvent(Transform platform)
        {
            while (onPlatform)
            {
                onPlatform = IsOnPlatform(platform, 1.1f);

                yield return null;
            }

            onPlatform = false;

            RevertPlatform();
        }

        private bool IsOnPlatform(Transform platform, float multiplier)
        {
            bool isOnPlatform = false;

            // ==================================================

            float radius = capsuleRadius * multiplier;

            Vector3 position = transform.position + Vector3.up * radius;

            LayerMask layerMask = 1 << sceneHandler.GetPlatformLayer();

            // ==================================================

            int platformsHit = Physics.OverlapSphereNonAlloc(position, radius, colliders, layerMask);

            for (int i = 0; i < platformsHit; i ++)
            {
                if (colliders[i].transform == platform)
                {
                    isOnPlatform = true;

                    break;
                }
            }

            // ==================================================

            return isOnPlatform;
        }

        private Vector3 RelativePosition(Transform target, Vector3 worldPosition)
        {
            freeTransform.SetParent(target);

            freeTransform.position = worldPosition;

            return freeTransform.localPosition;
        }

        public void SetTarget(Transform target) // called by NPC.cs
        {
            float angle = AngleFromTarget(target);

            SetTargetAngle(angle);
        }

        private float AngleFromTarget(Transform target)
        {
            Vector3 difference = target.position - transform.position;

            float angle = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;

            return angle;
        }

        public void AutoRotate(bool autoTarget) // called by ActionStateHandler.cs
        {
            if (inputDetected)
            {
                targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            }

            else
            {
                int targetsHit = meleeAttack.GetTargetsHit().Count;

                bool useAutoTarget = meleeAttack.forceAutoTarget || autoTarget;

                if (useAutoTarget && targetsHit == 0)
                {
                    Transform nearestTarget = GetNearestTarget();

                    if (nearestTarget != null)
                    {
                        targetAngle = AngleFromTarget(nearestTarget);
                    }
                }

                else if (targetsHit == 1)
                {
                    TargetCollider currentTarget = meleeAttack.GetCurrentTarget();

                    if (currentTarget != null)
                    {
                        Transform target = currentTarget.transform;

                        Transform targetTransform = currentTarget.targetTransform;

                        if (targetTransform != null)
                        {
                            target = targetTransform;
                        }

                        targetAngle = AngleFromTarget(target);
                    }
                }
            }
        }

        public void SetSimulationState(bool value) // called by ActionStateHandler.cs
        {
            inSimulationState = value;

            onLedge = value ? onLedge : false;
        }

        public void SetStateValues(bool[] values) // called by StateHandler.cs
        {
            movementEnabled = values[0];
            rotationEnabled = values[1];

            autoRotate = values[2];

            movementState = values[3];
        }

        public void ResetValues()
        {
            ResetTransform();

            attributes.ResetHealth(true);
        }

        public void ResetTransform()
        {
            Teleport(spawnPosition, spawnRotation, true);
        }

        public void Teleport(Transform target, bool resetToIdle)
        {
            Teleport(target.position, target.eulerAngles, resetToIdle);
        }

        public void Teleport(Vector3 position, Vector3 rotation, bool resetToIdle)
        {
            RevertPlatform();

            // ==================================================

            bool previouslyGrounded = isGrounded;

            transform.position = position;
            transform.eulerAngles = rotation;

            currentAngle = rotation.y;
            targetAngle = currentAngle;

            // ==================================================

            bool forceCheck = true;

            isGrounded = IsGrounded(position, capsuleRadius, forceCheck);

            skipLandingTrigger = resetToIdle && isGrounded;

            if (isGrounded && !previouslyGrounded)
            {
                airborne = false;

                LandEvent(position, rotation, currentVelocity, currentInput, airborne);

                if (!resetToIdle)
                {
                    animator.SetTrigger(hashLand);
                }
            }

            else
            {
                moveDirection = Quaternion.Euler(Vector3.up * rotation.y) * Vector3.forward;
            }

            // ==================================================

            if (resetToIdle)
            {
                animator.CrossFade("Idle", 0.01f);
            }

            // ==================================================

            if (activePlayer)
            {
                sceneHandler.cameraTarget.SetToTargetPosition();
            }
        }

        public void SetPlayerTransform(Vector3 newPosition, Vector3 newRotation) // use this method to move player to specified position
        {
            float moveSpeed = currentVelocity;

            SetPlayerTransform(newPosition, newRotation, newRotation.y, moveSpeed, moveDirection, fallSpeed, false);
        }

        public float GetVolumeModifier()
        {
            float volumeModifier = 1.0f;

            if (gameObject.layer == sceneHandler.GetPlayerLayer() && !activePlayer)
            {
                volumeModifier = 0.33f;
            }

            return volumeModifier;
        }

        public void SetPlatform(Transform newPlatform) // called internally and by PlayerController.cs
        {
            if (newPlatform != null && newPlatform.gameObject.activeInHierarchy)
            {
                currentPlatform = newPlatform;

                // =============================================================

                transform.SetParent(newPlatform);

                transform.SetSiblingIndex(playerIndex);

                // =============================================================

                if (activePlayer)
                {
                    var cameraTarget = sceneHandler.cameraTarget;

                    cameraTarget.SetParent(newPlatform, airborne);
                }
            }
        }

        public void RevertPlatform()
        {
            SetPlatform(defaultParent);
        }

        private void AddPlayer()
        {
            if (sceneHandler != null)
            {
                if (gameObject.layer == sceneHandler.GetPlayerLayer())
                {
                    playerManager.AddPlayer(this);
                }

                // =============================================================

                if (interactionTriggers != null)
                {
                    InitializeInteractionTriggers();
                }

                if (playerHUD != null)
                {
                    List<CanvasGroup> canvasGroups = sceneHandler.canvasGroups;

                    if (!canvasGroups.Contains(playerHUD))
                    {
                        canvasGroups.Add(playerHUD);
                    }
                }
            }
        }

        private void RemovePlayer()
        {
            if (sceneHandler != null)
            {
                if (gameObject.layer == sceneHandler.GetPlayerLayer())
                {
                    playerManager.RemovePlayer(this);
                }

                // =============================================================

                if (playerHUD != null)
                {
                    List<CanvasGroup> canvasGroups = sceneHandler.canvasGroups;

                    canvasGroups.Remove(playerHUD);
                }
            }
        }

        public void SetGamePaused(bool value) // called by PauseMenu.cs
        {
            gamePaused = value;

            disableMoveInput = value;
        }

        public void DisableButtonInputs(bool value) // called by MenuManager.cs
        {
            disableButtonInputs = value;

            meleeAttack.DisableAllInputs(value);
        }

        private void SetInteractionState(bool value, int throwing)
        {
            disableMoveInput = value;

            lockMovement = value;
            lockRotation = value;
        }

        public void RevertStepOffset() // called internally and by RootMotionSimulator.cs
        {
            controller.stepOffset = stepOffset * stepOffsetScale;
        }

        private void InitializeInteractionTriggers()
        {
            Transform triggers = interactionTriggers.transform;

            int childCount = triggers.childCount;

            for (int i = 0; i < childCount; i ++)
            {
                InteractionTrigger trigger = triggers.GetChild(i).GetComponent<InteractionTrigger>();

                if (trigger != null)
                {
                    trigger.Initialize(this);
                }
            }
        }

        public void ResetTurnSmoothTime()
        {
            turnSmoothTime = defaultTurnSmoothTime;
        }

        public void ResetAllTriggers() // called by StateHandler.cs
        {
            meleeAttack.ResetAllTriggers();

            animator.ResetTrigger(hashJump);

            animator.ResetTrigger(hashLand);
        }

        public void PlayLandingAudio() // called by StateHandler.cs
        {
            if (footsteps != null)
            {
                footsteps.Land();
            }
        }

        private void ActionEvent(AnimationEvent currentEvent) // called by animation event
        {
            int actionState = rms.actionState;

            if (actionState == eventState)
            {
                Transform meleeStates = rms.actionList.GetChild(1);

                ActionState currentState = meleeStates.GetChild(actionState - 1).GetComponent<ActionState>();

                if (currentState != null)
                {
                    List<GameEvent> actionEvents = currentState.actionEvents;

                    int index = currentEvent.intParameter;

                    if (index >= 0 && index < actionEvents.Count)
                    {
                        actionEvents[index].Invoke();
                    }
                }
            }
        }

        public void EventState() // called by animation event and by MeleeAttack.cs
        {
            eventState = rms.actionState;
        }

        public void ResetEventState() // called by ActionStateHandler.cs
        {
            eventState = -1;
        }

        public void AddExternalForce(Vector3 force)
        {
            externalForces += force;
        }

        public void SetExternalForce(Vector3 force)
        {
            externalForces = force;
        }

        private void SetTransform(Vector3 newPosition, Vector3 newRotation, float targetRotation, float moveSpeed, Vector3 moveDirection, float fallSpeed, bool setLocalPosition)
        {
            SetTransform(newPosition, newRotation, setLocalPosition);

            SetMovement(newRotation, targetRotation, moveDirection, moveSpeed, fallSpeed);
        }

        private void SetTransform(Vector3 newPosition, Vector3 newRotation)
        {
            SetTransform(newPosition, newRotation, false);
        }

        private void SetLocalTransform(Vector3 newPosition, Vector3 newRotation)   
        {
            SetTransform(newPosition, newRotation, true);
        }

        public void SetTransform(Vector3 newPosition, Vector3 newRotation, bool setLocalPosition) // called by internally and by RootMotionSimulator.cs
        {   
            if (useRigidbody)
            {
                if (setLocalPosition)
                {
                    freeTransform.parent = currentPlatform;

                    freeTransform.localPosition = newPosition;

                    newPosition = freeTransform.position;
                }

                // =============================================================

                transform.position = newPosition;

                transform.eulerAngles = newRotation;
            }

            else
            {
                // Disable character controller before setting new position, then re-enable

                controller.enabled = false;

                if (setLocalPosition)
                    transform.localPosition = newPosition;

                else
                    transform.position = newPosition;

                controller.enabled = true;

                // =============================================================

                // Setting rotation does not require character controller to be disabled

                transform.eulerAngles = newRotation;
            }
        }

        private void SetMovement(Vector3 newRotation, float targetRotation, Vector3 moveDirection, float moveSpeed, float fallSpeed)
        {
            currentAngle = newRotation.y;
            targetAngle = targetRotation;

            // =============================================================

            this.fallSpeed = fallSpeed;

            this.moveDirection = moveDirection;

            currentVelocity = moveSpeed;
            targetVelocity = moveSpeed;
        }

        public int GetPlayerCount() // called by ActionStateHandler.cs
        {
            int playerCount = playerManager.playerCount;

            return playerCount;
        }

        public void SetAngle(float newAngle) // called by Target.cs
        {
            Vector3 newRotation = Vector3.up * newAngle;

            currentAngle = newAngle;
            targetAngle = newAngle;

            if (useRigidbody)
            {
                m_rigidbody.rotation = Quaternion.Euler(newRotation);
            }

            else
            {
                transform.eulerAngles = newRotation;
            }
        }

        public void AddEventCollider(EventCollider eventCollider) // called by EventCollider.cs
        {
            if (!eventColliders.Contains(eventCollider))
            {
                eventColliders.Add(eventCollider);
            }
        }

        public void RemoveEventCollider(EventCollider eventCollider) // called by EventCollider.cs
        {
            eventColliders.Remove(eventCollider);
        }

        public void ClearEventColliders() // called by PlayerManager.cs
        {
            int listCount = eventColliders.Count;

            for (int i = 0; i < listCount; i ++)
            {
                EventCollider eventCollider = eventColliders[i];

                eventCollider.onExit.Invoke();
            }

            eventColliders.Clear();
        }

        public void SetSpawnPoint(Vector3 position, Vector3 rotation) // called by PlayerManager.cs
        {
            spawnPosition = position;

            spawnRotation = rotation;
        }

        public void ResetInput() // called by PlayerManager.cs
        {
            moveInput = Vector2.zero;

            sprintInput = false;

            isSprinting = false;

            inputDetected = false;
        }

        // =============================================================
        //    Combat Methods
        // =============================================================

        public void HitTarget(TargetCollider targetCollider, Vector3[] inputValues, Attack attack) // called by AttackSource.cs
        {
            Target target = targetCollider.GetTarget();

            Attributes targetAttributes = target.GetComponent<Attributes>();

            if (targetAttributes.GetEvadeTimer() > 0f)
            {
            }

            else
            {
                float weaponSound = inputValues[0].x;
                float hitVolume = inputValues[0].y;
                float particleIndex = inputValues[0].z;

                bool ignoreHitCache = inputValues[1].x > 0f;

                float targetsHit = inputValues[2].x;
                float elapsedTime = inputValues[2].y;
                float timeDifference = inputValues[2].z;

                Vector3 sourcePosition = inputValues[3];
                Vector3 collisionPoint = inputValues[4];

                // ===========================================================

                if (!ignoreHitCache)
                {
                    this.meleeAttack.AddTarget(target, targetCollider);
                }

                float[] damageValues = GetDamageValues(targetCollider, target, attack.damageMultiplier);

                // ===========================================================

                Collider currentCollider = targetCollider.GetComponent<Collider>();

                Collider visualBounds = targetCollider.visualBounds;

                Vector3 contactPoint = target.GetContactPoint(currentCollider, visualBounds, collisionPoint, false);

                // ===========================================================

                if (meleeAttack)
                {
                    Collider collider = currentCollider;

                    if (visualBounds != null)
                        collider = visualBounds;

                    // to prevent visual feedback from falling outside the visual bounds, we will push the contact point toward the center

                    contactPoint = (3.0f * contactPoint + collider.bounds.center) / 4.0f;
                }

                // ===========================================================

                bool useHitstopValue = (targetsHit < 1.0f) && (elapsedTime <= 0.10f);
                bool shakeCameraValue = timeDifference >= 0.1f;

                float criticalHit = damageValues[1];
                float weakHit = damageValues[2];

                float targetSound = targetCollider.hitSound;
                float useHitstop = useHitstopValue ? 1.0f : 0f;

                float strongAttack = attack.strongAttack ? 1.0f : 0f;
                float centeredHit = attack.centeredHit ? 1.0f : 0f;
                float shakeCamera = shakeCameraValue ? 1.0f : 0;

                float shakeScale = attack.cameraShake.scale;
                float shakeCycles = attack.cameraShake.cycles;
                float shakeLocal = attack.cameraShake.isLocal ? 1.0f : 0f;

                Vector3 contactDifference = contactPoint - target.transform.position;

                // ===========================================================

                int damage = (int) damageValues[0];

                hitValues[0] = sourcePosition;
                hitValues[1] = contactDifference;

                hitValues[2] = new Vector3(damage, criticalHit, weakHit);
                hitValues[3] = new Vector3(targetSound, weaponSound, hitVolume);
                hitValues[4] = new Vector3(particleIndex, attack.hitstopDuration, useHitstop);
                hitValues[5] = new Vector3(strongAttack, centeredHit, shakeCamera);
                hitValues[6] = new Vector3(shakeScale, shakeCycles, shakeLocal);
                hitValues[7] = new Vector3(attack.launchValueIndex, 0f, 0f);

                // ===========================================================

                SendCollisionFeedback(target, hitValues);
            }
        }

        public float[] GetDamageValues(TargetCollider targetCollider, Target target, float damageMultiplier) // called by WeaponColliderGroup.cs and ProjectileCollider.cs
        {        
            bool weakHit = false;
            bool criticalHit = false;

            float baseDamage = attributes.baseAttack * damageMultiplier;
            float damage = baseDamage * attributes.attackModifier * actionModifier;

            float criticalChance = Mathf.Round(Random.Range(0f, 100.0f));
            float criticalRate = attributes.criticalRate;

            weakHit = damage < baseDamage;

            if (!weakHit && criticalChance < criticalRate)
            {
                damage *= attributes.criticalModifier;

                criticalHit = true;
            }

            if (!attributes.GetIgnoreDamageModifiers())
            {
                damage *= targetCollider.damageModifier;
            }

            if (damage < 1.0f)
            {
                damage = 1.0f;
            }

            // =============================================================

            damageValues[0] = damage;
            damageValues[1] = criticalHit ? 1.0f : 0f;
            damageValues[2] = weakHit ? 1.0f : 0f;

            return damageValues;
        }

        private Transform GetNearestTarget()
        {
            GameObject targetColliders = target.targetColliders.gameObject;

            if (targetColliders != null)
            {
                targetColliders.SetActive(false);
            }

            // =============================================================

            Transform nearestTarget = null;

            float radius = meleeAttack.autoTargetRange;

            LayerMask targetLayerMask = 1 << sceneHandler.GetTargetLayer();

            // =============================================================

            int targetCount = Physics.OverlapSphereNonAlloc(transform.position, radius, targets, targetLayerMask);

            if (targetCount > 0)
            {
                nearestTarget = targets[0].transform;

                Vector3 difference = transform.position - nearestTarget.position;

                float nearestDistance = difference.sqrMagnitude;

                for (int i = 1; i < targetCount; i ++)
                {
                    Transform currentTarget = targets[i].transform;

                    difference = transform.position - currentTarget.position;

                    float distance = difference.sqrMagnitude;

                    if (distance < nearestDistance)
                    {
                        nearestTarget = currentTarget;

                        nearestDistance = distance;
                    }
                }
            }

            // =============================================================

            if (targetColliders != null)
            {
                targetColliders.SetActive(true);
            }

            // =============================================================

            if (nearestTarget != null)
            {
                TargetCollider targetCollider = nearestTarget.GetComponent<TargetCollider>();

                if (targetCollider != null)
                {
                    Transform targetTransform = targetCollider.targetTransform;

                    if (targetTransform != null)
                    {
                        nearestTarget = targetTransform;
                    }
                }
            }

            return nearestTarget;
        }

        // =============================================================
        //    Rigidbody Methods
        // =============================================================

        private void InitializeRigidbody()
        {
            useRigidbody = false;

            InitializeRigidbody(0);

            // =========================================================

            InitializeCapsule();

            CreatePhysicMaterial();

            // =========================================================

            freeTransform = sceneHandler.GetFreeTransform();
        }

        private void InitializeRigidbody(int throwing)
        {
            if (m_rigidbody == null)
                m_rigidbody = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;

            if (capsuleCollider == null)
                capsuleCollider = gameObject.AddComponent(typeof(CapsuleCollider)) as CapsuleCollider;

            rms.SetComponents(sceneHandler, m_rigidbody);

            // =========================================================

            m_rigidbody.useGravity = false;

            m_rigidbody.isKinematic = true;

            m_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // this mode prevents the player from stuttering when moving towards other moving objects

            m_rigidbody.constraints =  RigidbodyConstraints.None |  RigidbodyConstraints.FreezeRotationX |  RigidbodyConstraints.FreezeRotationY |  RigidbodyConstraints.FreezeRotationZ;
        }

        private void InitializeCapsule()
        {
            float skinWidth = controller.skinWidth;

            capsuleRadius = controller.radius + skinWidth;
            capsuleHeight = controller.height + skinWidth * 2.0f;

            // =========================================================

            capsuleCollider.center = controller.center;

            capsuleCollider.radius = capsuleRadius;
            capsuleCollider.height = capsuleHeight;
        }

        private void CreatePhysicMaterial()
        {
            physicMaterial = new PhysicMaterial();

            physicMaterial.dynamicFriction = 0f;
            physicMaterial.staticFriction = 0f;

            physicMaterial.bounciness = 0f;

            physicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
            physicMaterial.bounceCombine = PhysicMaterialCombine.Average;

            // =========================================================

            capsuleCollider.material = physicMaterial;
        }

        public void CheckSlope() // called internally and by RootMotionSimulator.cs
        {
            if (useRigidbody)
            {
                float rayLength = 0.2f;

                Ray ray = new Ray(transform.position + Vector3.up * 1.0f, Vector3.down);

                groundHit = Physics.Raycast(ray, out rayHit, rayLength + 1.0f, sceneHandler.groundLayerMask);

                // ==================================================

                groundSlope = -1.0f;

                if (groundHit)
                {   
                    groundNormal = rayHit.normal;

                    groundSlope = Vector3.Angle(groundNormal, Vector3.up);
                }
            }
        }

        public void StopRigidbody()
        {
            if (useRigidbody)
            {
                m_rigidbody.velocity = Vector3.zero;
            }
        }

        public void MoveRigidbody(Vector3 moveDirection, float moveSpeed, float moveDistance, float yRotation)
        {
            if (moveSpeed > 0f && groundHit)
            {
                Vector3 directionRight = Vector3.Cross(moveDirection, Vector3.up);

                moveDirection = Vector3.Cross(rayHit.normal, directionRight).normalized;
            }

            // =============================================================

            float fallSpeed = isGrounded ? GetGroundedFallSpeed() : this.fallSpeed;

            Vector3 horizontal = moveDirection * moveSpeed;

            Vector3 vertical = Vector3.up * fallSpeed;

            // =============================================================

            Vector3 velocity = horizontal + vertical;

            /*moveSpeed = Mathf.Abs(moveSpeed);

            if (isGrounded && velocity.magnitude > moveSpeed)
            {
                velocity = velocity.normalized * moveSpeed;
            }*/

            // =============================================================

            m_rigidbody.velocity = velocity;
        }

        private float GetGroundedFallSpeed()
        {
            float moveSlope = -1.0f * (90.0f - Vector3.Angle(groundNormal, moveDirection));

            // =============================================================

            bool steepSlope = groundSlope > 15.0f;

            bool steepDrop = moveSlope <= -25.0f;

            // =============================================================

            bool useFallSpeed = !steepSlope || steepDrop;

            float fallSpeed = useFallSpeed ? groundedFallSpeed : 0f;

            // =============================================================

            return fallSpeed;
        }

        // =============================================================
        //    Get Methods - Variables
        // =============================================================

        public bool GetActivePlayer() // called by MeleeAttack.cs
        {
            return activePlayer;
        }

        public Vector3 GetSpawnRotation()
        {
            return spawnRotation;
        }

        public float GetTargetInput() // called by Footsteps.cs
        {
            return targetInput;
        }

        public bool GetIsCrouching() // called by RootMotionSimulator.cs
        {
            return isCrouching;
        }

        public float GetCurrentAngle() // called by RootMotionSimulator.cs
        {
            return currentAngle;
        }

        public float GetTargetAngle() // called by RootMotionSimulator.cs and ActionStateHandler.cs
        {
            return targetAngle;
        }

        public bool GetDisableMoveInput() // called by Menu.cs
        {
            return disableMoveInput;
        }

        public bool GetLockMovement() // called by RootMotionSimulator.cs
        {
            return lockMovement;
        }

        public bool GetSimulationState() // called by RootMotionSimulator.cs
        {
            return inSimulationState;
        }

        public Transform GetCurrentPlatform() // called by RootMotionSimulator.cs
        {
            return currentPlatform;
        }

        public bool GetIsGrounded() // called by RootMotionSimulator.cs
        {
            return isGrounded;
        }

        public bool GetOnLedge() // called by RootMotionSimulator.cs
        {
            return onLedge;
        }

        public bool GetIsTired()
        {
            return isTired;
        }

        public bool GetMovementState() // called by ActionStateHandler.cs
        {
            return movementState;
        }

        public float GetHitSoundTime() // called by AttackSource.cs
        {
            return hitSoundTime;
        }

        public float GetActionModifier()
        {
            return actionModifier;
        }

        // =============================================================
        //    Set Methods - Variables
        // =============================================================

        public void SetPlayerIndex(int value) // called by PlayerManager.cs
        {
            playerIndex = value;
        }

        public void SetActivePlayer(bool value) // called by PlayerManager.cs
        {
            activePlayer = value;
        }

        public void SetTurnSmoothTime(float value) // called by ActionStateHandler.cs
        {
            float smoothTime = (value >= 0f) ? value : defaultTurnSmoothTime;

            turnSmoothTime = smoothTime;
        }

        public void SetOnLedge(bool value) // called by RootMotionSimulator.cs
        {
            onLedge = value;
        }

        public void SetHitSoundTime() // called by Target.cs
        {
            hitSoundTime = Time.unscaledTime;
        }

        public void SetDefaultParent(Transform target) // called by PlayerManager.cs
        {
            defaultParent = target;
        }

        public void SetActionModifier(float value)
        {
            actionModifier = value;
        }

        // =============================================================
        //    Get Methods - Components
        // =============================================================

        public Animator GetAnimator()
        {
            return animator;
        }

        public CharacterController GetCharacterController()
        {
            return controller;
        }

        public Attributes GetAttributes()
        {
            return attributes;
        }

        public MeleeAttack GetMeleeAttack()
        {
            return meleeAttack;
        }

        public RootMotionSimulator GetRootMotionSimulator()
        {
            return rms;
        }

        public Target GetTarget()
        {
            return target;
        }

        public SceneHandler GetSceneHandler()
        {
            return sceneHandler;
        }

        public PlayerManager GetPlayerManager()
        {
            return playerManager;
        }

        public AudioList GetVoiceAudioList()
        {
            return voiceAudioList;
        }

        public Rigidbody GetRigidbody()
        {
            return m_rigidbody;
        }

        // =============================================================
        //    Network Variables
        // =============================================================

        private CustomNetworkManager networkManager;

        private bool m_soloInstance;

        public bool soloInstance { get { return m_soloInstance; } }
        public bool localEvent { get { return (soloInstance || activePlayer); } }

        public bool isLocalPlayer { get { return false; } }

        private bool previousInputDetected;

        private float previousTargetInput;
        private float previousTargetVelocity;

        private float previousTargetAngle;

        // =============================================================
        //    Network Methods
        // =============================================================

        public void SetTargetAngle(float angle)
        {
            if (localEvent)
            {
                targetAngle = angle;

                if (isLocalPlayer)
                    CmdSetTargetAngle(angle);
            }
        }

        public void SyncedJump(float moveSpeed, float jumpAngle, float targetAngle)
        {   
            if (localEvent)
            {
                jumpValues[0] = targetAngle;
                jumpValues[1] = moveSpeed;

                jumpValues[2] = jumpAngle;

                Vector3 position = transform.position;
                Vector3 rotation = transform.eulerAngles;

                // =============================================================

                JumpEvent(position, rotation, jumpValues, true);

                if (isLocalPlayer)
                    CmdSyncedJump(position, rotation, jumpValues, true);
            }
        }

        public void SyncedFall() // called by RootMotionSimulator.cs
        {
            if (localEvent)
            {
                Vector3 localPosition = transform.localPosition;

                FallEvent(localPosition, transform.eulerAngles);

                if (isLocalPlayer)
                    CmdSyncedFall(localPosition, transform.eulerAngles);
            }
        }

        private void SyncedLand(Vector3 newPosition, Vector3 newRotation, float moveSpeed, float moveInput, bool isAirborne)
        {
            if (localEvent)
            {
                LandEvent(newPosition, newRotation, moveSpeed, moveInput, isAirborne);

                if (isLocalPlayer)
                    CmdSyncedLand(newPosition, newRotation, moveSpeed, moveInput, isAirborne);
            }
        }

        private void SyncedPlatformLand(Vector3 newPosition, Vector3 newRotation, float moveSpeed, float moveInput, bool isAirborne, Platform platform, int siblingIndex)
        {
            if (localEvent)
            {
                PlatformLandEvent(newPosition, newRotation, moveSpeed, moveInput, isAirborne, platform, siblingIndex);

                if (isLocalPlayer)
                    CmdSyncedPlatformLand(newPosition, newRotation, moveSpeed, moveInput, isAirborne, platform, siblingIndex);
            }
        }

        private void SetPlayerTransform(Vector3 newPosition, Vector3 newRotation, float targetRotation, float moveSpeed, Vector3 moveDirection, float fallSpeed, bool setLocalPosition)
        {
            if (localEvent)
            {
                SetTransform(newPosition, newRotation, targetRotation, moveSpeed, moveDirection, fallSpeed, setLocalPosition);

                if (isLocalPlayer)
                    CmdSetPlayerTransform(newPosition, newRotation, targetRotation, moveSpeed, moveDirection, fallSpeed, setLocalPosition);
            }
        }

        public void SetToCurrentTransform() // called by RootMotionSimulator.cs
        {
            Vector3 currentPosition = transform.localPosition;
            Vector3 currentRotation = transform.eulerAngles;

            float moveSpeed = currentVelocity;

            bool setLocalTransform = true;

            SetPlayerTransform(currentPosition, currentRotation, targetAngle, moveSpeed, moveDirection, fallSpeed, setLocalTransform);
        }

        public void SyncTransform() // called by ActionStateHandler.cs
        {
            if (isLocalPlayer)
            {
                Vector3 currentPosition = transform.localPosition;
                Vector3 currentRotation = transform.eulerAngles;

                float moveSpeed = currentVelocity;

                bool setLocalTransform = true;

                CmdSetPlayerTransform(currentPosition, currentRotation, targetAngle, moveSpeed, moveDirection, fallSpeed, setLocalTransform);
            }
        }

        private void SendCollisionFeedback(Target target, Vector3[] values)
        {
            if (localEvent)
            {
                target.ReceiveCollisionFeedback(this, values);

                if (isLocalPlayer)
                    CmdSendCollisionFeedback(target, values);
            }
        }

        public void SendNetworkTrigger(int actionState, float angle) // called by ActionStateHandler.cs
        {
            if (isLocalPlayer)
            {
                CmdSendNetworkTrigger(actionState, angle);
            }
        }

        public void SetInteractionState(bool value) // called by NPC.cs and DialogueBox.cs
        {
            if (localEvent)
            {
                SetInteractionState(value, 0);

                if (isLocalPlayer)
                    CmdSetInteractionState(value);
            }
        }

        // =============================================================
        //    Network Commands
        // =============================================================

        // [Command]
        private void CmdSetTargetAngle(float angle)
        {
            networkManager.SetTargetAngle(this, angle);
        }

        // [Command]
        private void CmdSyncedJump(Vector3 position, Vector3 rotation, float[] values, bool useTrigger)
        {
            networkManager.SyncedJump(this, position, rotation, values, useTrigger);
        }

        // [Command]
        private void CmdSyncedFall(Vector3 position, Vector3 rotation)
        {
            networkManager.SyncedFall(this, position, rotation);
        }

        // [Command]
        private void CmdSyncedLand(Vector3 newPosition, Vector3 newRotation, float moveSpeed, float moveInput, bool isAirborne)
        {
            networkManager.SyncedLand(this, newPosition, newRotation, moveSpeed, moveInput, isAirborne);
        }

        // [Command]
        private void CmdSyncedPlatformLand(Vector3 newPosition, Vector3 newRotation, float moveSpeed, float moveInput, bool isAirborne, Platform platform, int siblingIndex)
        {
            networkManager.SyncedLand(this, newPosition, newRotation, moveSpeed, moveInput, isAirborne, platform, siblingIndex);
        }

        // [Command]
        private void CmdSetPlayerTransform(Vector3 newPosition, Vector3 newRotation, float targetRotation, float moveSpeed, Vector3 moveDirection, float fallSpeed, bool setLocalPosition)
        {
            networkManager.SetPlayerTransform(this, newPosition, newRotation, targetRotation, moveSpeed, moveDirection, fallSpeed, setLocalPosition);
        }

        // [Command]
        private void CmdSendCollisionFeedback(Target target, Vector3[] values)
        {
            networkManager.SendCollisionFeedback(this, target, values);
        }

        // [Command]
        private void CmdSendNetworkTrigger(int actionState, float angle)
        {
            networkManager.SendNetworkTrigger(this, actionState, angle);
        }

        // [Command]
        private void CmdSetInteractionState(bool value)
        {
            networkManager.SetInteractionState(this, value);
        }

        // [Command]
        private void CmdSetInputDetected(bool value)
        {
            networkManager.SetInputDetected(this, value);
        }

        // [Command]
        private void CmdSetTargetInput(float value)
        {
            networkManager.SetTargetInput(this, value);
        }

        // [Command]
        private void CmdSetTargetVelocity(float value)
        {
            networkManager.SetTargetVelocity(this, value);
        }

        // =============================================================
        //    Client RPCs
        // =============================================================

        // [ClientRpc]
        public void RpcSetTargetAngle(float angle)
        {
            if (!isLocalPlayer)
            {
                targetAngle = angle;
            }
        }

        // [ClientRpc]
        public void RpcSyncedJump(Vector3 position, Vector3 rotation, float[] values, bool useTrigger)
        {
            if (!isLocalPlayer)
            {
                JumpEvent(position, rotation, values, true);
            }
        }

        // [ClientRpc]
        public void RpcSyncedFall(Vector3 position, Vector3 rotation)
        {
            if (!isLocalPlayer)
            {
                FallEvent(position, rotation);
            }
        }

        // [ClientRpc]
        public void RpcSyncedLand(Vector3 newPosition, Vector3 newRotation, float moveSpeed, float moveInput, bool isAirborne)
        {
            if (!isLocalPlayer)
            {
                LandEvent(newPosition, newRotation, moveSpeed, moveInput, isAirborne);
            }
        }

        // [ClientRpc]
        public void RpcSyncedPlatformLand(Vector3 newPosition, Vector3 newRotation, float moveSpeed, float moveInput, bool isAirborne, Platform platform, int siblingIndex)
        {
            if (!isLocalPlayer)
            {
                PlatformLandEvent(newPosition, newRotation, moveSpeed, moveInput, isAirborne, platform, siblingIndex);
            }
        }

        // [ClientRpc]
        public void RpcSetPlayerTransform(Vector3 newPosition, Vector3 newRotation, float targetRotation, float moveSpeed, Vector3 moveDirection, float fallSpeed, bool setLocalPosition)
        {
            if (!isLocalPlayer)
            {
                SetTransform(newPosition, newRotation, targetRotation, moveSpeed, moveDirection, fallSpeed, setLocalPosition);
            }
        }

        // [ClientRpc]
        public void RpcSendCollisionFeedback(Target target, Vector3[] values)
        {
            if (!isLocalPlayer)
            {
                target.ReceiveCollisionFeedback(this, values);
            }
        }

        // [ClientRpc]
        public void RpcSendNetworkTrigger(int actionState, float angle)
        {
            if (!isLocalPlayer)
            {
                targetAngle = angle;

                animator.SetInteger(hashNetworkState, actionState);
                animator.SetTrigger(hashNetworkTrigger);
            }
        }

        // [ClientRpc]
        public void RpcSetInteractionState(bool value)
        {
            if (!isLocalPlayer)
            {
                SetInteractionState(value, 0);
            }
        }

        // [ClientRpc]
        public void RpcSetInputDetected(bool value)
        {
            if (!isLocalPlayer)
            {
                inputDetected = value;
            }
        }

        // [ClientRpc]
        public void RpcSetTargetInput(float value)
        {
            if (!isLocalPlayer)
            {
                targetInput = value;
            }
        }

        // [ClientRpc]
        public void RpcSetTargetVelocity(float value)
        {
            if (!isLocalPlayer)
            {
                targetVelocity = value;
            }
        }
    }
}




