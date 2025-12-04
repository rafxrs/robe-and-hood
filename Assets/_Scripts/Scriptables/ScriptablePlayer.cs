using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Scriptables
{
    [CreateAssetMenu(fileName = "New Player", menuName = "ScriptableObjects/Player")]
    public class ScriptablePlayer : ScriptableUnitBase
    {
        [FormerlySerializedAs("_advancedStats")] [SerializeField] private Stats advancedStats;
        public Stats AdvancedStatistics => advancedStats;
        public GameObject[] impactPrefabs;

        /// <summary>
        /// Keeping base stats as a struct on the scriptable keeps it flexible and easily editable.
        /// We can pass this struct to the spawned prefab unit and alter them depending on conditions.
        /// </summary>
        [Serializable]
        public struct Stats
        {
            public int speed;
            public float climbSpeed;
            public int maxMana;
            public int rollManaCost;
            public float bounceForce;
        }
    }
}
