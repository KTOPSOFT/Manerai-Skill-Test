using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class Target : MonoBehaviour
    {
        public Transform targetColliders;

        public InteractionCollider interactionCollider;

        public AudioPlayer audioPlayer;

        // =================================================

        [Space(10)]

        public GameEvent onTargetHit;

        public CustomAnimation customAnimation;

        // =================================================

        [Space(10)]

        public bool useHitTrigger;

        public bool faceAttackSource;

        // =================================================

        private int groupIndex;

        private PlayerController playerController;

        private Attributes attributes;

        private CapsuleCollider mainCollider;

        private SceneHandler sceneHandler;

        private ObjectPoolManager objectPoolManager;

        private float[] jumpValues = new float[3];

        // =================================================

        private DamageIndicator centeredIndicator;

        private List<DamageIndicator> activeIndicators = new List<DamageIndicator>();

        private List<TargetCollider> colliders = new List<TargetCollider>();

        // =========================================================
        //    Animation Parameters
        // =========================================================

        readonly int hashHit = Animator.StringToHash("Hit");

        readonly int hashStrongHit = Animator.StringToHash("StrongHit");

        // =========================================================
        //    Standard Methods
        // =========================================================

        private void Awake()
        {
            Transform elements = GameObject.FindWithTag("GameplayElements").transform;

            sceneHandler = elements.GetComponent<SceneHandler>();

            objectPoolManager = sceneHandler.objectPools.GetComponent<ObjectPoolManager>();

            // =================================================

            attributes = GetComponent<Attributes>();

            if (useHitTrigger || faceAttackSource)
            {
                playerController = GetComponent<PlayerController>();
            }

            // =================================================

            groupIndex = gameObject.layer;

            mainCollider = GetComponent<CapsuleCollider>();

            // =================================================

            if (interactionCollider != null)
            {
                int layer = sceneHandler.GetInteractionLayer();

                interactionCollider.Initialize(attributes, layer);
            }

            // =================================================

            InitializeTargetColliders();
        }

        private void InitializeTargetColliders()
        {
            int targetLayer = sceneHandler.GetTargetLayer();

            int childCount = targetColliders.childCount;

            // =================================================

            colliders.Clear();

            for (int i = 0; i < childCount; i ++)
            {
                TargetCollider collider = targetColliders.GetChild(i).GetComponent<TargetCollider>();

                colliders.Add(collider);
            }

            // =================================================

            for (int i = 0; i < childCount; i ++)
            {
                TargetCollider targetCollider = colliders[i];

                targetCollider.gameObject.layer = targetLayer;

                targetCollider.Initialize(this);
            }
        }

        public Vector3 GetContactPoint(Collider targetCollider, Collider visualBounds, Vector3 collisionPoint, bool randomScale) // called by PlayerController.cs
        {  
            Collider bounds = targetCollider;

            if (visualBounds != null)
                bounds = visualBounds;

            // ================================================================

            float scaleModifier = 0f;

            if (randomScale)
                scaleModifier = Random.Range(0f, 0.1f);

            // ================================================================

            Vector3 offset = Random.insideUnitCircle * scaleModifier;

            Vector3 contactPoint = bounds.ClosestPointOnBounds(collisionPoint) + offset;

            return contactPoint;
        }

        public void ReceiveCollisionFeedback(PlayerController sender, Vector3[] values) // called by PlayerController.cs
        {
            Vector3 sourcePosition = values[0];
            Vector3 contactDifference = values[1];

            int damage = (int) values[2].x;
            bool criticalHit = values[2].y > 0f;
            bool weakHit = values[2].z > 0f;

            int targetSound = (int) values[3].x;
            int weaponSound = (int) values[3].y;
            float hitVolume = values[3].z;

            int particleIndex = (int) values[4].x;
            float hitstopDuration = values[4].y;
            bool useHitstop = values[4].z > 0f;

            bool strongAttack = values[5].x > 0f;
            bool centeredHit = values[5].y > 0f;
            bool shakeCamera = values[5].z > 0f;

            float shakeScale = values[6].x;
            int shakeCycles = (int) values[6].y;
            bool shakeLocal = values[6].z > 0f;

            int launchValueIndex = (int) values[7].x;

            // =================================================

            attributes.UpdateHealth(-damage);

            if (audioPlayer != null)
            {
                AudioList targetHitSounds = GetTargetHitSounds(targetSound);

                audioPlayer.PlayFromAudioList(targetHitSounds, sender, hitVolume, true);

                // =============================================

                AudioList weaponHitSounds = GetWeaponHitSounds(weaponSound);

                audioPlayer.PlayFromAudioList(weaponHitSounds, sender, hitVolume, true);

                // =============================================

                sender.SetHitSoundTime();
            }

            // =================================================

            Vector3 direction = sourcePosition - transform.position;

            float newAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            if (faceAttackSource)
            {
                playerController.SetAngle(newAngle);
            }

            // =================================================

            if (useHitTrigger)
            {
                SendFlying(sender, strongAttack, launchValueIndex, newAngle);
            }

            // =================================================

            if (shakeCamera && !weakHit)
            {
                bool activePlayer = sender.GetActivePlayer();

                CameraShake.Shake(shakeScale, shakeCycles, shakeLocal, activePlayer, sourcePosition);
            }

            // =================================================

            if (useHitstop && !weakHit)
            {
                MeleeAttack meleeAttack = sender.GetMeleeAttack();

                meleeAttack.StartHitstop(hitstopDuration);
            }

            // =================================================

            Vector3 relativePoint = transform.position + contactDifference;

            Vector3 feedbackPosition = relativePoint;

            if (centeredHit)
            {
                float height = mainCollider.height;

                feedbackPosition = transform.position + Vector3.up * height * 3.25f / 4.0f;
            }

            onTargetHit.Invoke();

            // =================================================

            if (customAnimation != null)
            {
                Vector3 difference = sender.transform.position - transform.position;

                float yRotation = transform.eulerAngles.y * Mathf.Deg2Rad;

                customAnimation.radianOffset = Mathf.Atan2(difference.x, difference.z) - Mathf.PI - yRotation;

                customAnimation.PlayAnimation();
            }

            // =================================================

            Vector3 offset = transform.position - Camera.main.transform.position;

            if (offset.sqrMagnitude <= 400.0f)
            {
                int hitValue = weakHit ? 2 : (criticalHit ? 1 : 0);

                Transform particlePool = objectPoolManager.hitParticles.GetChild(particleIndex);
                Transform indicatorPool = objectPoolManager.damageIndicators.GetChild(hitValue);

                Transform particleTransform = ObjectPoolManager.GetFromPool(particlePool, feedbackPosition);
                Transform damageTransform = ObjectPoolManager.GetFromPool(indicatorPool, feedbackPosition);

                if (particleTransform != null)
                {
                    Vector3 difference = sender.transform.position - transform.position;

                    float angle = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;

                    // =================================================

                    particleTransform.eulerAngles = Vector3.up * angle;

                    particleTransform.gameObject.SetActive(true);
                }

                if (damageTransform != null)
                {
                    DamageIndicator indicator = damageTransform.GetComponent<DamageIndicator>();

                    indicator.SetSender(sender);
                    indicator.SetReceiver(this);

                    activeIndicators.Add(indicator);

                    if (centeredHit)
                    {
                        if (centeredIndicator != null && centeredIndicator.transform.position == feedbackPosition)
                            centeredIndicator.ForceExit();

                        centeredIndicator = indicator;
                    }

                    // =================================================

                    indicator.SetText(damage);

                    damageTransform.position = feedbackPosition;

                    indicator.EnableCanvas();
                }
            }
        }

        private void SendFlying(PlayerController sender, bool strongAttack, int index, float newAngle)
        {
            Animator animator = playerController.GetAnimator();

            if (strongAttack)
            {
                var launchValues = sender.GetMeleeAttack().launchValues;

                if (index >= 0 && index < launchValues.Length)
                {
                    var launchValue = launchValues[index];
                    
                    float launchSpeed = launchValue.launchSpeed;
                    float launchHeight = launchValue.launchHeight;

                    SendFlying(newAngle, launchSpeed, launchHeight);
                }

                else
                {
                    SendFlying(newAngle, 7.0f, 1.2f);
                }

                animator.SetTrigger(hashStrongHit);
            }

            else
            {
                bool isGrounded = playerController.GetIsGrounded();

                if (!isGrounded)
                {
                    SendFlying(newAngle, 2.0f, 0.4f);
                }

                animator.SetTrigger(hashHit);
            }
        }

        private void SendFlying(float newAngle, float moveSpeed, float height)
        {
            jumpValues[0] = newAngle;
            jumpValues[1] = moveSpeed;

            jumpValues[2] = newAngle + 180.0f;

            // =================================================

            bool useTrigger = false;

            float jumpHeight = playerController.jumpHeight;

            playerController.jumpHeight = height;

            // =================================================

            playerController.JumpEvent(transform.position, transform.eulerAngles, jumpValues, useTrigger);

            playerController.jumpHeight = jumpHeight;
        }

        private AudioList GetTargetHitSounds(int index)
        {
            AudioList audioList = null;

            List<AudioList> targetHitSounds = sceneHandler.audioLists.targetHitSounds;

            if (index < targetHitSounds.Count)
            {
                audioList = targetHitSounds[index];
            }

            return audioList;
        }

        private AudioList GetWeaponHitSounds(int index)
        {
            AudioList audioList = null;

            List<AudioList> weaponHitSounds = sceneHandler.audioLists.weaponHitSounds;

            if (index < weaponHitSounds.Count)
            {
                audioList = weaponHitSounds[index];
            }

            return audioList;
        }

        // =============================================================
        //    Get Methods - Variables
        // =============================================================

        public int GetGroupIndex() // called by Weapon.cs
        {
            return groupIndex;
        }

        public DamageIndicator GetCenteredIndicator() // called by DamageIndicator.cs
        {
            return centeredIndicator;
        }

        public List<DamageIndicator> GetActiveIndicators() // called by DamageIndicator.cs
        {
            return activeIndicators;
        }

        // =============================================================
        //    Set Methods - Variables
        // =============================================================

        public void SetGroupIndex(int value)
        {
            groupIndex = value;
        }

        public void SetCenteredIndicator(DamageIndicator newIndicator) // called by DamageIndicator.cs
        {
            centeredIndicator = newIndicator;
        }
    }
}








