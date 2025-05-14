using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class AttackSource : MonoBehaviour
    {
        public int m_hitSound;
        public int m_hitParticle;

        public AudioPlayer audioPlayer;

        protected AudioList audioList;
        protected AudioList targetList;

        protected float startTime;
        protected float previousTime;

        // =========================================================
        //    Weapon Collision Variables
        // =========================================================

        public Transform attackPoints;

        [Tooltip("Additional attack points that may not be connected to the weapon, such as the character's hand.")]
        public List<Renderer> additionalAttackPoints = new List<Renderer>();

        [Range(0f, 1.0f)]
        public float volume = 1.0f;

        [Range(0f, 1.0f)]
        public float hitSoundVolume = 1.0f;

        protected int pointCount;
        protected int attackPositionCount;
        protected int currentAttackPosition;

        protected float attackTimer;
        protected float timeInterval;

        protected Vector3[] m_PreviousPos = null;       

        protected bool inAttack;

        protected bool usePreviousPosition;
        protected bool useAttackPoints;

        protected List<Target> targetsHit = new List<Target>();
        protected List<AttackPoint> validAttackPoints = new List<AttackPoint>();

        protected Vector3[] inputValues = new Vector3[5];

        protected static RaycastHit[] s_RaycastHitCache = new RaycastHit[32];

        protected Attack currentAttack;

        // =========================================================
        //    Component Dependencies
        // =========================================================

        protected Animator animator;

        protected PlayerController playerController;

        protected Attributes attributes;

        protected MeleeAttack meleeAttack;

        protected SceneHandler sceneHandler;

        // =========================================================
        //    Animation Parameters
        // =========================================================

        readonly int hashLastAttack = Animator.StringToHash("LastAttack");

        // =========================================================
        //    Standard Methods
        // =========================================================

        public void Initialize(PlayerController playerController, SceneHandler sceneHandler) // called by MeleeAttack.cs
        {
            if (audioPlayer != null)
                audioList = audioPlayer.GetComponent<AudioList>();

            this.playerController = playerController;

            this.sceneHandler = sceneHandler;

            attributes = playerController.GetAttributes();

            meleeAttack = playerController.GetMeleeAttack();

            animator = playerController.GetAnimator();

            InitializeAttackPoints();
        }

        public void InitializeAttackPoints() // called internally and by ActionStateRecorder.cs
        {
            validAttackPoints.Clear();

            // =========================================================

            if (attackPoints != null)
            {
                int childCount = attackPoints.childCount;

                for (int i = 0; i < childCount; i ++)
                {
                    Renderer meshRenderer = attackPoints.GetChild(i).GetComponent<Renderer>();

                    if (meshRenderer != null)
                    {
                        AttackPoint newPoint = CreateAttackPoint(meshRenderer, i);

                        validAttackPoints.Add(newPoint);
                    }
                }
            }

            // =========================================================

            int listCount = additionalAttackPoints.Count;

            for (int i = 0; i < listCount; i ++)
            {
                Renderer meshRenderer = additionalAttackPoints[i];

                int index = meshRenderer.transform.GetSiblingIndex();

                AttackPoint newPoint = CreateAttackPoint(meshRenderer, index);

                validAttackPoints.Add(newPoint);
            }

            // =========================================================

            pointCount = validAttackPoints.Count;
        }

        private AttackPoint CreateAttackPoint(Renderer m_renderer, int index)
        {
            m_renderer.enabled = false;

            AttackPoint attackPoint = m_renderer.gameObject.AddComponent<AttackPoint>();

            // =========================================================

            Transform attackRoot = attackPoint.transform;

            Transform parent = attackRoot.parent;

            attackRoot.SetParent(null);

            float radius = attackRoot.localScale.x / 2.0f;

            attackPoint.radius = Mathf.Round(radius * 1000.0f) / 1000.0f;

            // =========================================================

            attackRoot.SetParent(parent);

            attackRoot.SetSiblingIndex(index);

            // =========================================================

            return attackPoint;
        }

        protected void BeginAttack(AttackProperties attackProperties, int attackIndex)
        {
            if (attackProperties != null && attackIndex >= 0)
            {
                List<Attack> attacks = attackProperties.attacks;

                int listCount = attacks.Count;

                if (attackIndex < listCount)
                {   
                    Attack attack = attackProperties.attacks[attackIndex];

                    if (attack != null)
                    {
                        currentAttack = attack;

                        currentAttack.onAttackStart.Invoke();

                        // ==============================================================

                        if (playerController.localEvent)
                        {
                            timeInterval = attackProperties.timeInterval;

                            useAttackPoints = false;

                            if (currentAttack.attackPointPositions.Count > 0)
                            {
                                useAttackPoints = true;

                                attackPositionCount = currentAttack.attackPointPositions.Count;

                                currentAttackPosition = 0;

                                attackTimer = 0f;
                            }

                            // ==============================================================

                            int valueA = currentAttack.hitSound;
                            int valueB = currentAttack.hitParticle;

                            int weaponSound = valueA >= 0 ? valueA : this.m_hitSound;
                            int particleIndex = valueB >= 0 ? valueB : this.m_hitParticle;

                            float ignoreHitCache = currentAttack.ignoreHitCache ? 1.0f : 0f;

                            inputValues[0] = new Vector3(weaponSound, hitSoundVolume, particleIndex);

                            inputValues[1] = new Vector3(ignoreHitCache, 0f, 0f);

                            // ==============================================================

                            targetsHit.Clear();

                            Transform playerTransform = playerController.transform;

                            m_PreviousPos = new Vector3[pointCount];

                            for (int i = 0; i < pointCount; ++i)
                            {
                                AttackPoint attackPoint = validAttackPoints[i];

                                attackPoint.previousPositions.Clear();

                                if (useAttackPoints)
                                    m_PreviousPos[i] = playerTransform.TransformPoint(currentAttack.attackPointPositions[i]);

                                else
                                    m_PreviousPos[i] = attackPoint.transform.position;

                                attackPoint.previousPositions.Add(m_PreviousPos[i]);
                            }

                            // ==============================================================

                            usePreviousPosition = currentAttack.usePreviousPosition;

                            startTime = Time.time;
                            previousTime = 0f;

                            inAttack = true;
                        }
                    }
                }
            }
        }

        public void EndAttack(bool animatorEvent) // called by MeleeAttack.cs
        { 
            if (animatorEvent && currentAttack != null)
            {   
                currentAttack.onAttackEnd.Invoke();
            }

            inAttack = false;
        }

        protected void UpdateAttackSource(Transform source, float attackSpeed)
        {
            if (useAttackPoints)
            {   
                attackTimer += Time.deltaTime * attackSpeed;

                if (attackTimer > timeInterval)
                {
                    int points = (int) (attackTimer / timeInterval);

                    for (int i = 0; i < points; i ++)
                    {
                        if (currentAttackPosition < attackPositionCount - pointCount)
                        {
                            currentAttackPosition += pointCount;

                            // CreateAttackVector(source, currentAttackPosition, false, false); // 12/03/2022 - removed method calls that generate garbage 

                            CreateAttackVector(source, currentAttackPosition, false, true);

                            // StartCoroutine(IE_CreateAttackVector(source, currentAttackPosition)); // 12/03/2022 - removed method calls that generate garbage 
                        }

                        else
                        {
                            inAttack = false;
                        }
                    }

                    attackTimer -= timeInterval;
                }

                else if (currentAttackPosition > 0)
                {
                    CreateAttackVector(source, currentAttackPosition, false, false);
                }
            }

            else
            {
                CreateAttackVector(source, 0, true, true);
            }
        }

        // ==============================================================

        // The following method is no longer in use as it generates garbage

        /*protected IEnumerator IE_CreateAttackVector(Transform source, int attackPosition) // let the attack vector be active for two physics updates - just for good measure
        {
            CreateAttackVector(source, attackPosition, false, true);

            yield return new WaitForFixedUpdate();

            CreateAttackVector(source, attackPosition, false, false);
        }*/

        // ==============================================================

        protected void CreateAttackVector(Transform source, int attackPosition, bool fallback, bool savePreviousPosition) // attack collision solution by Unity Technologies - 3D Game Kit
        {
            for (int i = 0; i < pointCount; ++i)
            {
                AttackPoint attackPoint = validAttackPoints[i];

                Vector3 worldPosition = Vector3.zero;
                Vector3 previousPosition = Vector3.zero;

                if (fallback)
                {       
                    worldPosition = attackPoint.transform.position;
                    previousPosition = m_PreviousPos[i];
                }

                else
                {
                    worldPosition = source.TransformPoint(currentAttack.attackPointPositions[attackPosition + i]);
                    previousPosition = source.TransformPoint(currentAttack.attackPointPositions[attackPosition + i - pointCount]);
                }

                Vector3 attackVector = worldPosition - previousPosition;

                // ==============================================================

                if (attackVector.magnitude < 0.001f)
                {    
                    attackVector = Vector3.forward * 0.0001f;
                }

                // ==============================================================

                Ray r = new Ray(worldPosition, attackVector.normalized);

                float radius = attackPoint.radius * currentAttack.radiusMultiplier;

                if (radius < 0f)
                    radius = attackPoint.radius;

                // ==============================================================

                int contacts = Physics.SphereCastNonAlloc(r, radius, s_RaycastHitCache, attackVector.magnitude, ~0, QueryTriggerInteraction.Ignore);

                for (int k = 0; k < contacts; ++k)
                {
                    Collider other = s_RaycastHitCache[k].collider;

                    if (other != null && other.gameObject.layer == sceneHandler.GetTargetLayer())
                    {                     
                        TargetCollider targetCollider = other.GetComponent<TargetCollider>();

                        Target target = targetCollider.GetTarget();

                        Target player = playerController.GetTarget();

                        if (target != null && target.GetGroupIndex() != player.GetGroupIndex() && !targetsHit.Contains(target))
                        {
                            Vector3 collisionPoint = GetCollisionPoint(worldPosition, previousPosition);

                            // ==============================================================

                            float currentTime = Time.unscaledTime;

                            float timeDifference = currentTime - previousTime;

                            previousTime = currentTime;

                            // ==============================================================

                            if (targetsHit.Count < 2)
                            {
                                attributes.UpdateEnergy(currentAttack.energyGain);
                            }

                            // currentAttack.onAttackHit.Invoke();

                            // ==============================================================

                            float elapsedTime = Time.time - startTime;

                            Vector3 sourcePosition = transform.position;

                            inputValues[2] = new Vector3(targetsHit.Count, elapsedTime, timeDifference);

                            inputValues[3] = sourcePosition;
                            inputValues[4] = collisionPoint;

                            playerController.HitTarget(targetCollider, inputValues, currentAttack);

                            targetsHit.Add(target);
                        }
                    }
                }

                // ==============================================================

                m_PreviousPos[i] = worldPosition;

                if (savePreviousPosition)
                    attackPoint.previousPositions.Add(previousPosition);
            }
        }

        protected Vector3 GetCollisionPoint(Vector3 worldPosition, Vector3 previousPosition)
        {
            bool usePreviousPosition = this.usePreviousPosition && targetsHit.Count == 0;

            Vector3 collisionPoint = usePreviousPosition ? previousPosition : worldPosition;

            return collisionPoint;
        }

        public void PlayFromAudioList() // called by MeleeAttack.cs
        {
            if (audioPlayer != null)
            {
                audioPlayer.PlayFromAudioList(audioList, playerController, volume, true);
            }
        }

        public void PlayFromAudioList(int index) // called by MeleeAttack.cs
        {
            if (audioPlayer != null)
            {
                audioPlayer.PlayFromAudioList(audioList, index, playerController, volume, true);
            }
        }

        public void SetTargetList(AudioList audioList)
        {
            targetList = audioList;
        }

        public void PlayFromTargetList(int index)
        {
            if (audioPlayer != null)
            {
                audioPlayer.PlayFromAudioList(targetList, index, playerController, volume, true);
            }
        }

        public void RequestFromTargetList(int index)
        {
            float timeDifference = Time.unscaledTime - playerController.GetHitSoundTime();

            if (timeDifference >= 0.1f)
            {
                PlayFromTargetList(index);

                playerController.SetHitSoundTime();
            }
        }
    }
}









