#if UNITY_EDITOR

    using UnityEngine;
using UnityEngine.Audio;

using UnityEditor;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class PlayerControllerGenerator
    {
        public static void CreatePlayerController()
        {
            int layer = LayerMask.NameToLayer("Player");

            GameObject playerController = CreateGameObject("New Player Controller", layer);

            // =========================================================

            Transform selection = Selection.activeTransform;

            if (selection != null)
            {
                SetParent(playerController, selection.gameObject);
            }

            // =========================================================

            playerController.AddComponent<Animator>();
            playerController.AddComponent<CharacterController>();
            playerController.AddComponent<PlayerController>();
            playerController.AddComponent<Target>();
            playerController.AddComponent<Attributes>();
            playerController.AddComponent<MeleeAttack>();
            playerController.AddComponent<RootMotionSimulator>();
            playerController.AddComponent<Footsteps>();

            AudioMixer audioMixer = GetAudioMixer();

            CreateMesh(playerController, layer);
            CreateWeapons(playerController, layer, audioMixer);
            CreateActions(playerController, layer);
            CreateAudio(playerController, layer, audioMixer);
            CreateColliders(playerController, layer);

            // =========================================================

            CharacterController controller = playerController.GetComponent<CharacterController>();

            controller.stepOffset = 0.15f;

            controller.center = Vector3.up * 0.9f;

            controller.radius = 0.32f;
            controller.height = 1.64f;
        }

        private static void CreateMesh(GameObject playerController, int layer)
        {
            GameObject mesh = CreateGameObject("Mesh", layer);

            SetParent(mesh, playerController);
        }

        private static void CreateWeapons(GameObject playerController, int layer, AudioMixer audioMixer)
        {
            GameObject weapons = CreateGameObject("Weapons", layer);

            SetParent(weapons, playerController);

            // =========================================================

            GameObject weapon = CreateGameObject("Weapon", layer);

            weapon.AddComponent<Weapon>();

            SetParent(weapon, weapons);

            // =========================================================

            GameObject audioPlayer = CreateGameObject("Audio Player", layer);

            audioPlayer.AddComponent<AudioPlayer>();
            audioPlayer.AddComponent<AudioList>();

            SetParent(audioPlayer, weapon);

            // =========================================================

            AudioSource audioSource = audioPlayer.GetComponent<AudioSource>();

            InitializeAudioSource(audioSource, audioMixer, 4);

            // =========================================================

            Weapon m_weapon = weapon.GetComponent<Weapon>();

            m_weapon.audioPlayer = audioPlayer.GetComponent<AudioPlayer>();

            // =========================================================

            MeleeAttack meleeAttack = playerController.GetComponent<MeleeAttack>();

            meleeAttack.weapons = weapons.transform;
        }

        private static void CreateActions(GameObject playerController, int layer)
        {
            GameObject actions = CreateGameObject("Actions", layer);

            SetParent(actions, playerController);

            // =========================================================

            GameObject actionStates = CreateGameObject("Action States", layer);

            SetParent(actionStates, actions);

            GameObject meleeStates = CreateGameObject("Melee States", layer);

            SetParent(meleeStates, actions);

            GameObject jumpStates = CreateGameObject("Jump States", layer);

            SetParent(jumpStates, actions);

            // =========================================================

            GameObject attack = CreateGameObject("Normal Attack", layer);

            attack.AddComponent<ActionState>();
            attack.AddComponent<AttackProperties>();

            SetParent(attack, meleeStates);

            // =========================================================

            RootMotionSimulator rms = playerController.GetComponent<RootMotionSimulator>();

            rms.actionList = actions.transform;
        }

        private static void CreateAudio(GameObject playerController, int layer, AudioMixer audioMixer)
        {
            GameObject audio = CreateGameObject("Audio", layer);

            SetParent(audio, playerController);

            // =========================================================

            GameObject voice = CreateGameObject("Voice", layer);

            voice.AddComponent<AudioPlayer>();
            voice.AddComponent<AudioList>();

            SetParent(voice, audio);

            AudioSource audioSourceA = voice.GetComponent<AudioSource>();

            InitializeAudioSource(audioSourceA, audioMixer, 1);

            // =========================================================

            GameObject footsteps = CreateGameObject("Footsteps", layer);

            footsteps.AddComponent<AudioPlayer>();

            SetParent(footsteps, audio);

            AudioSource audioSourceB = footsteps.GetComponent<AudioSource>();

            InitializeAudioSource(audioSourceB, audioMixer, 4);

            Footsteps playerFootsteps = playerController.GetComponent<Footsteps>();

            playerFootsteps.audioPlayer = footsteps.GetComponent<AudioPlayer>();

            // =========================================================

            PlayerController player = playerController.GetComponent<PlayerController>();

            player.voiceAudioPlayer = voice.GetComponent<AudioPlayer>();
        }

        private static void CreateColliders(GameObject playerController, int layer)
        {
            GameObject colliders = CreateGameObject("Colliders", layer);

            SetParent(colliders, playerController);

            // =========================================================

            GameObject targetColliders = CreateGameObject("Target Colliders", layer);

            SetParent(targetColliders, colliders);

            // =========================================================

            GameObject body = CreateGameObject("Body", layer);

            body.AddComponent<CapsuleCollider>();
            body.AddComponent<TargetCollider>();

            SetParent(body, targetColliders);

            // =========================================================

            Target target = playerController.GetComponent<Target>();

            target.targetColliders = targetColliders.transform;

            // =========================================================

            CapsuleCollider colliderA = body.GetComponent<CapsuleCollider>();

            colliderA.center = Vector3.up;

            colliderA.radius = 0.25f;
            colliderA.height = 2.0f;

            // =========================================================

            GameObject interactionTriggers = CreateGameObject("Interaction Triggers", layer);

            SetParent(interactionTriggers, colliders);

            interactionTriggers.SetActive(false);

            // =========================================================

            PlayerController player = playerController.GetComponent<PlayerController>();

            player.interactionTriggers = interactionTriggers;

            // =========================================================

            LayerMask itemLayerMask = 1 << LayerMask.NameToLayer("Item");

            GameObject itemChecker = CreateNewSphereTrigger("Item Checker", layer, 2.0f, itemLayerMask);

            SetParent(itemChecker, interactionTriggers);

            // =========================================================

            LayerMask npcLayerMask = 1 << LayerMask.NameToLayer("NPC");

            GameObject npcChecker = CreateNewSphereTrigger("NPC Checker", layer, 3.0f, npcLayerMask);

            SetParent(npcChecker, interactionTriggers);

            // =========================================================

            GameObject collisionTrigger = CreateNewCollisionTrigger("Collision Trigger", layer, player);

            SetParent(collisionTrigger, colliders);

            collisionTrigger.SetActive(false);
        }

        private static void InitializeAudioSource(AudioSource audioSource, AudioMixer audioMixer, int mixerGroup)
        {
            audioSource.outputAudioMixerGroup = GetMixerGroup(audioMixer, mixerGroup);

            audioSource.playOnAwake = false;

            audioSource.spatialBlend = 1.0f;

            audioSource.rolloffMode = AudioRolloffMode.Linear;

            audioSource.minDistance = 5.0f;
            audioSource.maxDistance = 20.0f;
        }

        private static GameObject CreateNewSphereTrigger(string name, int layer, float radius, LayerMask layerMask)
        {
            GameObject sphereTrigger = CreateGameObject(name, layer);

            // =========================================================

            SphereCollider collider = sphereTrigger.AddComponent<SphereCollider>();

            collider.isTrigger = true;

            collider.radius = radius;

            // =========================================================

            Rigidbody m_rigidbody = sphereTrigger.AddComponent<Rigidbody>();

            m_rigidbody.useGravity = false;

            m_rigidbody.isKinematic = true;

            m_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            // =========================================================

            InteractionTrigger interactionTrigger = sphereTrigger.AddComponent<InteractionTrigger>();

            interactionTrigger.layerMask = layerMask;

            // =========================================================

            return sphereTrigger;
        }

        private static GameObject CreateNewCollisionTrigger(string name, int layer, PlayerController player)
        {
            GameObject collisionTrigger = CreateGameObject(name, layer);

            CollisionTrigger trigger = collisionTrigger.AddComponent<CollisionTrigger>();

            player.collisionTrigger = trigger;

            // =========================================================

            CapsuleCollider collider = trigger.GetComponent<CapsuleCollider>();

            collider.center = Vector3.up * 0.9f;

            collider.radius = 0.4f;
            collider.height = 1.8f;

            // =========================================================

            return collisionTrigger;
        }

        private static GameObject CreateGameObject(string name, int layer)
        {
            GameObject newObject = GameObjectGenerator.CreateGameObject(name, layer);

            return newObject;
        }

        private static void SetParent(GameObject childObject, GameObject parentObject)
        {
            GameObjectGenerator.SetParent(childObject, parentObject);
        }

        private static AudioMixer GetAudioMixer()
        {
            AudioMixer audioMixer = null;

            string[] mixerAssets = AssetDatabase.FindAssets("Audio Mixer 01");

            if (mixerAssets.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(mixerAssets[0]);

                audioMixer = AssetDatabase.LoadAssetAtPath(assetPath, typeof(AudioMixer)) as AudioMixer;
            }

            return audioMixer;
        }

        private static AudioMixerGroup GetMixerGroup(AudioMixer audioMixer, int index)
        {
            AudioMixerGroup mixerGroup = null;

            if (audioMixer != null)
            {
                AudioMixerGroup[] mixerGroups = null;

                // =========================================================

                if (index == 0)
                    mixerGroups =  audioMixer.FindMatchingGroups("Master");

                else if (index == 1)
                    mixerGroups = audioMixer.FindMatchingGroups("Voice");

                else if (index == 2)
                    mixerGroups = audioMixer.FindMatchingGroups("Music");

                else if (index == 3)
                    mixerGroups = audioMixer.FindMatchingGroups("Environment");

                else if (index == 4)
                    mixerGroups = audioMixer.FindMatchingGroups("Sound FX");

                else if (index == 5)
                    mixerGroups = audioMixer.FindMatchingGroups("System");

                // =========================================================

                if (mixerGroups != null)
                {
                    mixerGroup = mixerGroups[0];
                }
            }

            return mixerGroup;
        }
    }
}

#endif







