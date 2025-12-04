using UnityEngine;

namespace _Scripts.Scriptables
{
    [CreateAssetMenu(fileName ="New Weapon", menuName = "ScriptableObjects/Weapon")]
    public class ScriptableWeapon : ScriptableObject
    {
        public int attackDamage;
        public float attackRate;
        public float attackRange;
        public RuntimeAnimatorController controller;
        public Rigidbody2D[] weaponPrefab;
        public float launchForce;
        public float launchArcHeight;
        public float launchDuration;
    

    }
}
