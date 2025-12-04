using System.Collections.Generic;
using System.Linq;
using _Scripts.Scriptables;
using UnityEngine;

namespace _Scripts.Systems
{
    /// <summary>
    /// One repository for all scriptable objects. Create your query methods here to keep your business logic clean.
    /// I make this a MonoBehaviour as sometimes I add some debug/development references in the editor.
    /// If you don't feel free to make this a standard class
    /// </summary>
    public class ResourceSystem : StaticInstance<ResourceSystem> {
        private List<ScriptableUnitBase> Units { get; set; }
        private Dictionary<UnitType, ScriptableUnitBase> _unitDict;
//-------------------------------------------------------------------------------------------//
        protected override void Awake() {
            base.Awake();
            AssembleResources();
        }
//-------------------------------------------------------------------------------------------//

        private void AssembleResources() {
            Units = Resources.LoadAll<ScriptableUnitBase>(nameof(_Scripts.Units)).ToList();
            _unitDict = Units.ToDictionary(r => r.unitType, r => r);
        }
//-------------------------------------------------------------------------------------------//

        public ScriptableUnitBase GetExampleHero(UnitType t) => _unitDict[t];
        public ScriptableUnitBase GetRandomHero() => Units[Random.Range(0, Units.Count)];
    }
}   