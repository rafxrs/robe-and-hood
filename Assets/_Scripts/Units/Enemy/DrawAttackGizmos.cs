using UnityEngine;

namespace _Scripts.Units.Enemy
{
    public class DrawAttackGizmos : MonoBehaviour
    {
        void OnDrawGizmosSelected()
        {
        
            if (CompareTag("Player"))
            {
                if (name == "SwordAttackPoint"){
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(transform.position,0.75f);
                }
                else if (name == "SpearAttackPoint")
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(transform.position, new Vector3(2,1,1));
                }
            
            }
            else if (CompareTag("Enemy") && GetComponentInParent<Enemy>().enemyScriptable.AdvancedStats.hasAttacks)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, GetComponentInParent<Enemy>().enemyScriptable.AdvancedStats.weaponAttackRange);
            }
        
        }
    }
}
