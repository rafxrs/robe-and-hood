using _Scripts.Scriptables;
using _Scripts.Systems;
using UnityEngine;

namespace _Scripts.Managers
{
    /// <summary>
    /// An example of a scene-specific manager grabbing resources from the resource system
    /// Scene-specific managers are things like grid managers, unit managers, environment managers etc
    /// </summary>
    public class UnitManager : StaticInstance<UnitManager> {

        public void SpawnEnemies() {
            // SpawnUnit(.Tarodev, new Vector3(1, 0, 0));
        }

        private void SpawnUnit(UnitType t, Vector3 pos) {
            var scriptableUnit = ResourceSystem.Instance.GetExampleHero(t);

            var spawned = Instantiate(scriptableUnit.prefab, pos, Quaternion.identity,transform);

            // Apply possible modifications here such as potion boosts, team synergies, etc
            var stats = scriptableUnit.BaseStats;
            stats.maxHealth += 20;

            spawned.GetComponent<UnitBase>().SetStats(stats);
        }
    }
}
