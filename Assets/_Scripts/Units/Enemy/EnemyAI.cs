using System;
using _Scripts.Scriptables;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _Scripts.Units.Enemy
{
    public class EnemyAI : MonoBehaviour
    {

        //-------------------------------------------------------------------------------------------//
        // PUBLIC GLOBAL VARIABLES
        public Transform leftBoundary;
        public Transform rightBoundary;
        public Transform bottomBoundary;
        public Transform upBoundary;
        public bool isChasing;
        [FormerlySerializedAs("TOLERANCE")] public float tolerance = 0.01f;
        //-------------------------------------------------------------------------------------------//
        // PRIVATE PRIMITIVES
        //-------------------------------------------------------------------------------------------//
        private int _currentWaypoint;
        private float _speed;
        private float _distToPlayer;
        private float _chaseSpeed;
        private float _patrolSpeed;
        private float _nextAttackTime = -1f;
        private float _dir = -1f; // direction of patrolling
        private float _nextWayPointDist;
        private float _nextTp;
        private bool _hasAttacks;
        private bool _isStatic;
        private bool _isTeleporting;
        [FormerlySerializedAs("_isInBounds")][SerializeField] private bool isInBounds;
        private bool _wasInBounds;

        //-------------------------------------------------------------------------------------------//
        // PRIVATE NON PRIMITIVES
        //-------------------------------------------------------------------------------------------//
        private Path _path;
        private Enemy _enemy;
        private Seeker _seeker;
        private Rigidbody2D _rb;
        private ScriptableEnemy _enemyScriptable;
        private Transform _playerTransform;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        private static readonly int Attack2 = Animator.StringToHash("Attack2");
        private static readonly int Teleport1 = Animator.StringToHash("Teleport");

        //-------------------------------------------------------------------------------------------//
        // START AND UPDATE
        //-------------------------------------------------------------------------------------------//
        // Start is called before the first frame update
        void Start()
        {
            _speed = _chaseSpeed;
            _seeker = GetComponent<Seeker>();
            _rb = GetComponent<Rigidbody2D>();
            _enemy = GetComponent<Enemy>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _playerTransform = GameObject.Find("Player").GetComponent<Transform>();

            _enemyScriptable = _enemy.enemyScriptable;
            _chaseSpeed = _enemyScriptable.AdvancedStats.chaseSpeed;
            _patrolSpeed = _enemyScriptable.AdvancedStats.patrolSpeed;
            _nextWayPointDist = _enemyScriptable.AdvancedStats.nextWayPointDist;
            _hasAttacks = _enemyScriptable.AdvancedStats.hasAttacks;
            _isStatic = _enemyScriptable.AdvancedStats.isStatic;

            if (!_isStatic)
            {
                InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
            }
        }
        void Update()
        {
            _distToPlayer = Mathf.Abs(transform.position.x - _playerTransform.position.x);
            if (Math.Abs(_playerTransform.position.x - transform.position.x) < tolerance)
            {
                _rb.velocity = Vector2.zero;
            }
            if ((_playerTransform.position.x > leftBoundary.position.x) && (_playerTransform.position.x < rightBoundary.position.x)
                                                                        && (_playerTransform.position.y > bottomBoundary.position.y)
                                                                        && (_playerTransform.position.y < upBoundary.position.y))
            {
                isInBounds = true;
            }
            else
            {
                isInBounds = false;
            }

            if (_isStatic && _hasAttacks && Time.timeSinceLevelLoad > 2f)
            {
                if (_playerTransform.position.x > transform.position.x)
                {
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                }
                else if (_playerTransform.position.x < transform.position.x)
                {
                    transform.localScale = new Vector3(1f, 1f, 1f);
                }

                if (_distToPlayer < _enemyScriptable.AdvancedStats.attackDistance && Time.time > _nextAttackTime && isInBounds)
                {
                    // Debug.Log("Static attack");
                    _nextAttackTime = Time.time + _enemyScriptable.AdvancedStats.attackRate;
                    _enemy.AttackAnimation();
                }
            }
            else if (_hasAttacks)
            {
                if (!_isStatic)
                {
                    if (!isInBounds)
                    {
                        if (_wasInBounds && _enemyScriptable.AdvancedStats.canFly)
                        {
                            ChaseAndAttack();
                        }
                        else
                        {
                            Patrol();
                        }

                    }
                    else
                    {
                        ChaseAndAttack();
                    }
                }
            }
            else
            {
                if (!_isStatic)
                {
                    Patrol();
                }

            }



        }
        //-------------------------------------------------------------------------------------------//
        // PATH FUNCTIONS
        //-------------------------------------------------------------------------------------------//
        void UpdatePath()
        {
            if (_seeker.IsDone())
            {
                _seeker.StartPath(_rb.position, _playerTransform.position, OnPathComplete);
            }

        }
        void OnPathComplete(Path p)
        {
            if (!p.error)
            {
                _path = p;
                _currentWaypoint = 0;
            }
        }
        //-------------------------------------------------------------------------------------------//
        // PATROL FUNCTIONS
        //-------------------------------------------------------------------------------------------//
        private void Patrol()
        {
            _speed = _patrolSpeed;
            isChasing = false;
            Vector2 direction = new Vector2(_dir, 0f);
            Vector2 force = direction * (_speed * Time.deltaTime);
            _rb.AddForce(force);
            if (_enemyScriptable.enemyType == ScriptableEnemy.EnemyType.Spider)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
                _spriteRenderer.flipY = !_wasInBounds;

                if (force.x >= 0.01)
                {
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                }
                else if (force.x <= 0.01)
                {
                    transform.localScale = new Vector3(1f, 1f, 1f);
                }
            }
            else
            {
                if (force.x >= 0.01)
                {
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                }
                else if (force.x <= 0.01)
                {
                    transform.localScale = new Vector3(1f, 1f, 1f);
                }
            }

        }
        public void FlipPatrol()
        {
            if (_enemyScriptable.AdvancedStats.hasAttacks)
            {
                _rb.velocity = Vector2.zero;
            }
            _dir *= -1f;
        }

        //-------------------------------------------------------------------------------------------//

        private void Chase()
        {
            if (_enemyScriptable.enemyType == ScriptableEnemy.EnemyType.Spider)
            {
                _spriteRenderer.flipY = false;
                _rb.gravityScale = 2;
            }

            if (_enemyScriptable.enemyType == ScriptableEnemy.EnemyType.SkeletonMage)
            {
                // randomly (50% chance) fire a laser OR teleport every x to y seconds
                float choice = Random.Range(-1, 1);

                if (choice < 0f && Time.time > _nextTp && !_isTeleporting)
                {
                    _speed = 0f;
                    _isTeleporting = true;
                    _nextTp = Time.time + Random.Range(2f, 4f);
                    Teleport();
                }
                else if (choice < 0f && Time.time > _nextTp && !_isTeleporting)
                {
                    _animator.SetTrigger(Attack2);
                }
            }
            _speed = _chaseSpeed;
            isChasing = true;
            if (_path == null)
            {
                return;
            }

            if (_currentWaypoint >= _path.vectorPath.Count)
            {
                return;
            }

            Vector2 direction = ((Vector2)_path.vectorPath[_currentWaypoint] - _rb.position).normalized;
            Vector2 force = direction * (_speed * Time.deltaTime);
            _rb.AddForce(force);
            float distance = Vector2.Distance(_rb.position, _path.vectorPath[_currentWaypoint]);

            if (distance < _nextWayPointDist)
            {
                _currentWaypoint++;
            }

            // NEW: face the player instead of using A* force
            float dxToPlayer = _playerTransform.position.x - transform.position.x;

            // small deadzone so we don't flicker when almost aligned
            if (dxToPlayer > 0.1f)
            {
                // player is to the right → enemy looks right (in your art, that’s scale.x = -1)
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else if (dxToPlayer < -0.1f)
            {
                // player is to the left → enemy looks left
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }

        void ChaseAndAttack()
        {
            _wasInBounds = true;
            if (_distToPlayer < _enemyScriptable.AdvancedStats.attackDistance && Time.time < _nextAttackTime)
            {
                // Debug.Log("Cant attack");
                Chase();
            }
            else if (_distToPlayer < _enemyScriptable.AdvancedStats.attackDistance && Time.time > _nextAttackTime && !_isTeleporting)
            {
                _nextAttackTime = Time.time + _enemyScriptable.AdvancedStats.attackRate;
                _enemy.AttackAnimation();
            }

            else
            {
                Chase();
            }
        }

        void Teleport()
        {
            // For skeleton mage only: teleport close to the player within a specific range
            Debug.Log("Teleport now");
            _animator.SetTrigger(Teleport1);
            Invoke(nameof(PerformTeleport), 1f);
        }
        private void PerformTeleport()
        {
            var position = _playerTransform.position;
            transform.position = new Vector2(
                position.x + Random.Range(-3f, 3f),
                position.y + 0.5f
            );
            _isTeleporting = false;
        }

        //-------------------------------------------------------------------------------------------//
        public void Jump()

        {
            _rb.AddForce(new Vector2(0f, _enemyScriptable.AdvancedStats.jumpForce));
        }


    }
}
