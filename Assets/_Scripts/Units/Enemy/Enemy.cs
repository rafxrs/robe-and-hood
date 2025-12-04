using _Scripts.Scriptables;
using _Scripts.UI;
using _Scripts.Units.Objects;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Units.Enemy
{
    public class Enemy : MonoBehaviour
    {
        //-------------------------------------------------------------------------------------------//
        public ScriptableEnemy enemyScriptable;
        public bool isDead;
        //-------------------------------------------------------------------------------------------//
        [FormerlySerializedAs("_attackPoint")][SerializeField] private Transform attackPoint;
        [SerializeField] private Transform laserAttackPoint;
        private Player.Player _player;
        private EnemyAI _enemyAI;
        private Animator _animator;
        private Rigidbody2D _rb;
        private RewardSpawner _rewardSpawner;
        private SpriteRenderer _spriteRenderer;
        private FloatingHealthBar _healthBar;
        private int _currentHealth;
        private bool _isAttacking;

        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Death = Animator.StringToHash("Death");

        [SerializeField] private MovingPlatform linkedPlatform;


        //-------------------------------------------------------------------------------------------//
        private void Start()
        {
            _currentHealth = enemyScriptable.BaseStats.maxHealth;
            _player = GameObject.Find("Player").GetComponent<Player.Player>(); NullCheck.CheckNull(_player);
            _enemyAI = GetComponent<EnemyAI>(); NullCheck.CheckNull(_enemyAI);
            _rb = GetComponent<Rigidbody2D>(); NullCheck.CheckNull(_rb);
            _animator = GetComponent<Animator>(); NullCheck.CheckNull(_animator);
            _rewardSpawner = GetComponent<RewardSpawner>(); NullCheck.CheckNull(_rewardSpawner);
            _spriteRenderer = GetComponent<SpriteRenderer>(); NullCheck.CheckNull(_spriteRenderer);
            _healthBar = GetComponentInChildren<FloatingHealthBar>();
            if (_healthBar == null)
            {
                Debug.LogError("health bar is null");
            }

            if (_healthBar != null) _healthBar.SetMax(enemyScriptable.BaseStats.maxHealth);
            // Debug.Log("success");
            enemyScriptable.impactPrefabs[0] = Resources.Load<GameObject>("Prefabs/FX/Impacts/ImpactFX1");
            enemyScriptable.impactPrefabs[1] = Resources.Load<GameObject>("Prefabs/FX/Impacts/ImpactFX2");
            SetEnemy();
        }

        // Update is called once per frame
        void Update()
        {
            if (!isDead)
            {
                if (!_isAttacking && !enemyScriptable.AdvancedStats.isStatic)
                {
                    _animator.SetFloat(Speed, Mathf.Abs(_rb.velocity.x));
                    if (enemyScriptable.AdvancedStats.canFly)
                    {
                        if (_rb.velocity.x > 0.01f)
                        {
                            _animator.SetFloat(Speed, Mathf.Abs(_rb.velocity.x));
                        }
                        else if (_rb.velocity is { x: < 0.02f, y: > 0.01f })
                        {
                            _animator.SetFloat(Speed, Mathf.Abs(_rb.velocity.y));
                        }

                    }

                }

            }

        }

        //-------------------------------------------------------------------------------------------//
        void SetEnemy()
        {
            _animator.runtimeAnimatorController = enemyScriptable.controller;
        }
        //-------------------------------------------------------------------------------------------//

        public void TakeDamage(int damage)
        {
            if (isDead) return;
            // --- Existing FX & damage logic ---
            int rand = Random.Range(0, 2);
            Instantiate(enemyScriptable.impactPrefabs[rand], transform.position, Quaternion.identity);

            _spriteRenderer.color = Color.red;
            Invoke(nameof(ResetSprite), 0.1f);

            // activate health bar
            transform.Find("WorldHealthBar").gameObject.SetActive(true);

            _currentHealth -= damage;
            _healthBar.Set(_currentHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }


        void ResetSprite()
        {
            _spriteRenderer.color = Color.white;
        }
        //-------------------------------------------------------------------------------------------//
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && enemyScriptable.AdvancedStats.isBouncable && !other.GetComponent<CharacterController2D>().mGrounded)
            {
                TakeDamage(enemyScriptable.AdvancedStats.bounceDamage);
                _player.Bounce();
            }
            else if (other.CompareTag("PlayerHitbox") && enemyScriptable.BaseStats.collisionDamage)
            {
                _player.TakeDamage(enemyScriptable.AdvancedStats.attackDamage);
                // player.Knockback(other);
            }
            else if (other.CompareTag("Boundary"))
            {
                if (_enemyAI.isChasing)
                {
                }
                else
                {
                    if (enemyScriptable.enemyType != ScriptableEnemy.EnemyType.Slime) _rb.velocity = Vector3.zero;
                    _enemyAI.FlipPatrol();
                }
            }

        }

        //-------------------------------------------------------------------------------------------//
        void IsAttacking()
        {
            _isAttacking = true;
            Invoke(nameof(ResetIsAttacking), 0.75f);
        }
        void ResetIsAttacking()
        {
            _isAttacking = false;
        }
        public void Attack()
        {
            var hitPlayer = new Collider2D[10];
            Physics2D.OverlapCircleNonAlloc(attackPoint.position, enemyScriptable.AdvancedStats.weaponAttackRange, hitPlayer, enemyScriptable.playerLayer);

            foreach (var hit in hitPlayer)
            {
                if (hit != null)
                {
                    Debug.Log("We hit " + hit.name);
                    if (hit.CompareTag("PlayerHitbox"))
                    {
                        _player.TakeDamage(enemyScriptable.AdvancedStats.attackDamage);
                    }
                }

            }
        }
        public void LaserAttack()
        {
            // 1) compute spawn + direction
            Vector2 spawnPos = (Vector2)laserAttackPoint.position;
            Vector2 toPlayer = (Vector2)_player.transform.position - spawnPos;

            // 2) instantiate & initialize
            GameObject laser = Instantiate(enemyScriptable.laserPrefab, spawnPos, Quaternion.identity);
            laser.GetComponent<LaserBeam>().Initialize(toPlayer);
        }
        public void AttackAnimation()
        {
            IsAttacking();
            int attackNumber = Random.Range(0, 2);
            string attackTrigger = "Attack" + attackNumber;
            _animator.SetTrigger(attackTrigger);
        }
        //-------------------------------------------------------------------------------------------//
        void Die()
        {
            if (isDead) return;  
            isDead = true;

            Debug.Log("enemy died");

            // die animation
            _rb.velocity = Vector2.zero;
            _animator.SetTrigger(Death);

            if (linkedPlatform != null)
            {
                Debug.Log("Platform activated by enemy death");
                linkedPlatform.Activate();
            }

            _rewardSpawner.Reward(transform.position + new Vector3(0, 0.3f, 0));

            GetComponent<BoxCollider2D>().enabled = false;
            GetComponent<EnemyAI>().enabled = false;

            Destroy(gameObject, 1f);
        }

        //-------------------------------------------------------------------------------------------//
        void OnDrawGizmosSelected()
        {
            if (attackPoint == null)
            {
                return;
            }
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, enemyScriptable.AdvancedStats.weaponAttackRange);
        }

        public Transform GetAttackPoint()
        {
            return attackPoint;
        }
    }
}
