using UnityEngine;

using System;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    [Serializable]
    public class Attack
    {
        [Header("Attack Properties")]

        public float damageMultiplier = 1.0f;
        public float hitstopDuration;

        [Tooltip("Energy points added to meter if attack lands.")]
        public float energyGain;
        
        // =================================================
        
        [Header("Launch Properties")]
        
        [Tooltip("If true, this attack will launch smaller enemies into the air.")]
        public bool strongAttack;
        
        [Tooltip("The launch value index set in the Melee Attack component of the player object.")]
        public int launchValueIndex;

        // =================================================

        [Header("Overrides")]

        public float radiusMultiplier = 1.0f;

        public int hitSound = -1;
        public int hitParticle = -1;

        // =================================================

        [Header("Collision")]

        [Tooltip("When an attack is too fast, the point of collision might be too far off the intended position. Set this to true to use the attack point's previous position as the collision point.")]
        public bool usePreviousPosition;

        [Tooltip("If true, the collision's target will not be cached for auto targeting. Applicable to additional hits and projectiles that are not part of the main attack sequence.")]
        public bool ignoreHitCache;
        
        [Tooltip("If true, then damage indicators and hit particles will appear on the center of the target's collider.")]
        public bool centeredHit;

        // =================================================
        
        [Space(10)]
        
        public CameraShake cameraShake;

        [Serializable]
        public class CameraShake
        {
            public float scale;

            public int cycles;

            public bool isLocal = true;
        }

        // =================================================

        [Header("Game Events")]

        // public GameEvent onAttackHit;

        public GameEvent onAttackStart;
        public GameEvent onAttackEnd;

        // =================================================

        [Header("Collision Testing")]

        public List<Vector3> attackPointPositions = new List<Vector3>();
    }
}




