using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class AttackProperties : MonoBehaviour
    {
        public List<Attack> attacks = new List<Attack>();
        
        public float timeInterval = 0.01f;
        
        private void Reset()
        {
            attacks.Clear();
            
            // =========================================================
            
            Attack attack = new Attack();
            
            attack.damageMultiplier = 1.0f;
            
            attack.radiusMultiplier = 1.0f;
            
            attack.hitSound = -1;
            
            // =========================================================
            
            attacks.Add(attack);
        }
    }
}




