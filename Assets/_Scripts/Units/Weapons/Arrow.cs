using _Scripts.Managers;
using _Scripts.Scriptables;
using _Scripts.Units.Player;
using UnityEngine;

namespace _Scripts.Units.Weapons
{
    /// <summary>
    /// This class implements arrow behaviour
    /// </summary>
    public class Arrow : MonoBehaviour
    {
        public Side side;
        public ScriptableWeapon w;
        Rigidbody2D _rb;
        bool _isInAir = true;
        Player.Player _player;
        private float _awakeTime;
        private float _soundDiff;

        [System.Serializable]
        public enum Side
        {
            Player,
            Enemy,
        }


        // Start is called before the first frame update
        void Start()
        {
            _awakeTime = Time.time;
            if (side == Side.Player)
            {
                //AudioManager.instance.Play("Bow Release");
            }
            AudioManager.instance.Play("Arrow");
            Destroy(this.gameObject, 5f);
            _rb = GetComponent<Rigidbody2D>();
            _player = GameObject.Find("Player").GetComponent<Player.Player>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            if (_isInAir)
            {
                var velocity = _rb.velocity;
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }


        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Ground") || other.CompareTag("Obstacle"))
            {
                StopMotion();
                PolygonCollider2D arrow = GetComponent<PolygonCollider2D>();
                arrow.enabled = false;
                _isInAir = false;
                Invoke(nameof(StopMotion), 0.05f);

            }
            if (side == Side.Player)
            {
                if (other.CompareTag("Enemy"))
                {
                    _soundDiff = (Time.time - _awakeTime) / -5f;
                    AudioManager.instance.Play("Arrow Impact", _soundDiff);
                    Debug.Log("arrow hit " + other.name + " and dealt " + w.attackDamage + " dmg");
                    Enemy.Enemy enemy = other.GetComponent<Enemy.Enemy>();
                    if (other.GetComponent<Enemy.Enemy>().enemyScriptable.enemyType == ScriptableEnemy.EnemyType.SkeletonShield)
                    {
                        bool facingOpposite = PlayerDir(_player) != EnemyDir(enemy);
                        // If facing opposite directions → block partially
                        if (!facingOpposite)
                        {
                            int reducedDamage = Mathf.RoundToInt(w.attackDamage * 0.25f);
                            Debug.Log($"🛡 SkeletonShield blocked arrow! Damage reduced from {w.attackDamage} → {reducedDamage}");
                            enemy.TakeDamage(reducedDamage);
                            Destroy(this.gameObject);
                            return;
                        }
                    }
                    Debug.Log("enemy took " + w.attackDamage + " dmg from " + w.name);
                    enemy.TakeDamage(w.attackDamage);
                    Destroy(this.gameObject);
                }
                else if (other.CompareTag("Crate"))
                {
                    _soundDiff = (Time.time - _awakeTime) / -5f;
                    AudioManager.instance.Play("Arrow Impact", _soundDiff);
                    Destroy(this.gameObject);
                    other.GetComponent<Crate>().TakeDamage(w.attackDamage);
                }
                else if (other.CompareTag("Lever"))
                {
                    other.GetComponent<Lever>().SwitchLeverState();
                }
            }
            else if (side == Side.Enemy)
            {
                if (other.CompareTag("PlayerHitbox"))
                {
                    // Player player= other.GetComponent<Player>();
                    _player.TakeDamage(w.attackDamage);
                    Destroy(this.gameObject);
                }
            }


        }
        void StopMotion()
        {
            _rb.velocity = Vector2.zero;
            _rb.gravityScale = 0;
        }

        int PlayerDir(Player.Player p)
        {
            // Player uses _mFacingRight from CharacterController2D
            return p.controller._mFacingRight ? 1 : -1;
        }

        int EnemyDir(Enemy.Enemy e)
        {
            // Enemy uses localScale
            return e.transform.localScale.x >= 0 ? 1 : -1;

        }
    }
}
