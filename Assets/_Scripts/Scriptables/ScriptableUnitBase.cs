using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Scriptables
{
    /// <summary>
    /// Keeping all relevant information about a unit on a scriptable means we can gather and show
    /// info on the menu screen, without instantiating the unit prefab.
    /// </summary>
    public abstract class ScriptableUnitBase : ScriptableObject {
        public UnitType unitType;

        [FormerlySerializedAs("_stats")] [SerializeField] private Stats stats;
        public Stats BaseStats => stats;

        // Used in game
        public GameObject prefab;
    
        // Used in menus
        // public string description;
        // public Sprite menuSprite;
    }

    /// <summary>
    /// Keeping base stats as a struct on the scriptable keeps it flexible and easily editable.
    /// We can pass this struct to the spawned prefab unit and alter them depending on conditions.
    /// </summary>
    [Serializable]
    public struct Stats {
        public int maxHealth;
        public bool collisionDamage;
    }

    [Serializable]
    public enum UnitType {
        Player = 0,
        Enemy = 1,
        Npc = 2
    }
}