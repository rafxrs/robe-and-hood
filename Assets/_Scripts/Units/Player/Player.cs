using System.Collections;
using _Scripts.Managers;
using _Scripts.Scriptables;
using _Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace _Scripts.Units.Player
{
    /// <summary>
    /// Main class for the player
    /// This class deals with health, movement, mana, coins and player related UI
    ///
    /// Input System bindings (see Awake):
    ///   Move  - Arrow keys / gamepad left stick
    ///   Jump  - Z / gamepad south button
    ///   Roll  - C / gamepad east button
    ///
    /// Attack input (PrimaryAttack = X, SecondaryAttack = Left Shift) lives in PlayerCombat.
    /// </summary>

    //-------------------------------------------------------------------------------------------//
    public class Player : MonoBehaviour
    {
        //-------------------------------------------------------------------------------------------//
        // GLOBAL VARIABLES
        //-------------------------------------------------------------------------------------------//
        // PUBLIC
        [FormerlySerializedAs("_platformLayerMask")] public LayerMask platformLayerMask;
        public CharacterController2D controller;
        public HealthBar healthBar;
        public ManaBar manaBar;
        //-------------------------------------------------------------------------------------------//
        public float currentMana;
        public int coins;
        public bool isClimbing;
        public bool roll;
        public bool tookDamage;

        /// <summary>Current raw Move action value, exposed so other scripts (e.g. PlayerCombat) can
        /// read directional input without touching the legacy Input class.</summary>
        public Vector2 MoveInput => _moveAction.ReadValue<Vector2>();

        //-------------------------------------------------------------------------------------------//
        // PRIVATE VARIABLES
        //-------------------------------------------------------------------------------------------//
        ScriptablePlayer _playerScriptable;
        private Animator _animator;
        Rigidbody2D _rb;
        UIManager _uiManager;
        SpriteRenderer _spriteRenderer;
        Color _hurtColor;
        CapsuleCollider2D _hitbox;
        GameObject[] _impactPrefabs;
        PlayerCombat _playerCombat;

        //-------------------------------------------------------------------------------------------//
        // PRIMITIVES
        float _horizontalInput;
        float _nextClimb = -1f;
        [FormerlySerializedAs("_nextStepsTime")] public float nextStepsTime;
        int _currentHealth;
        bool _isDead;
        private bool _isAttacking;
        int _numKeys = 0;
        float _nextLadderSound;

        // Jump buffer
        const float JumpBufferTime = 0.12f;
        float _jumpPressedTime = -1f;

        // Roll input latch (mirrors the old GetButtonDown one-frame pulse)
        bool _rollQueued;

        private static readonly int IsClimbing = Animator.StringToHash("isClimbing");

        private static readonly int Speed = Animator.StringToHash("Speed");
        //-------------------------------------------------------------------------------------------//

        //-------------------------------------------------------------------------------------------//
        // INPUT SYSTEM ACTIONS
        //-------------------------------------------------------------------------------------------//
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _rollAction;

        private void Awake()
        {
            // Move - Arrow keys / gamepad left stick, returns a Vector2
            _moveAction = new InputAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            _moveAction.AddBinding("<Gamepad>/leftStick");

            // Jump - Z / gamepad south button
            _jumpAction = new InputAction("Jump", InputActionType.Button);
            _jumpAction.AddBinding("<Keyboard>/z");
            _jumpAction.AddBinding("<Gamepad>/buttonSouth");

            // Roll - C / gamepad east button
            _rollAction = new InputAction("Roll", InputActionType.Button);
            _rollAction.AddBinding("<Keyboard>/c");
            _rollAction.AddBinding("<Gamepad>/buttonEast");
        }

        private void OnEnable()
        {
            _moveAction.Enable();

            _jumpAction.performed += OnJumpPerformed;
            _jumpAction.canceled += OnJumpCanceled;
            _jumpAction.Enable();

            _rollAction.performed += OnRollPerformed;
            _rollAction.Enable();
        }

        private void OnDisable()
        {
            _moveAction.Disable();

            _jumpAction.performed -= OnJumpPerformed;
            _jumpAction.canceled -= OnJumpCanceled;
            _jumpAction.Disable();

            _rollAction.performed -= OnRollPerformed;
            _rollAction.Disable();
        }

        private void OnDestroy()
        {
            _moveAction?.Dispose();
            _jumpAction?.Dispose();
            _rollAction?.Dispose();
        }

        // ---- Input callbacks ----

        private void OnJumpPerformed(InputAction.CallbackContext ctx)
        {
            if (!GameManager.playerControl || _isDead) return;
            _jumpPressedTime = Time.time;
        }

        private void OnJumpCanceled(InputAction.CallbackContext ctx)
        {
            // Button released - cut the jump short if still rising (variable jump height)
            if (controller != null)
                controller.EndJump();
        }

        private void OnRollPerformed(InputAction.CallbackContext ctx)
        {
            if (!GameManager.playerControl || _isDead) return;
            _rollQueued = true;
        }

        //-------------------------------------------------------------------------------------------//
        // START
        void Start()
        {
            _playerScriptable = Resources.Load<ScriptablePlayer>("ScriptableObjects/Player");
            if (_playerScriptable == null)
            {
                Debug.LogError("PlayerScriptable is null");
            }

            _currentHealth = _playerScriptable.BaseStats.maxHealth;
            currentMana = _playerScriptable.AdvancedStatistics.maxMana;
            _hurtColor = Color.red;

            manaBar.SetMaxMana(currentMana);
            manaBar.SetMana(currentMana);
            healthBar.SetMax(_playerScriptable.BaseStats.maxHealth);
            healthBar.Set(_playerScriptable.BaseStats.maxHealth);

            _uiManager = GameObject.Find("Main Canvas").GetComponent<UIManager>();
            if (_playerScriptable == null)
            {
                Debug.LogError("UI Manager is null");
            }
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _hitbox = GameObject.Find("PlayerHitbox").GetComponent<CapsuleCollider2D>();
            _playerCombat = GetComponent<PlayerCombat>();
            if (_hitbox == null)
            {
                Debug.LogError("Hitbox is null");
            }

            InvokeRepeating("RegainMana", 0f, 0.01f);
        }

        //-------------------------------------------------------------------------------------------//
        // UPDATE
        void Update()
        {
            if (GameManager.playerControl && !_isDead)
            {
                Vector2 moveValue = _moveAction.ReadValue<Vector2>();

                _horizontalInput = moveValue.x * _playerScriptable.AdvancedStatistics.speed;
                if (Mathf.Abs(_horizontalInput) > 0.001f && Time.time > nextStepsTime && IsGrounded())
                {
                    nextStepsTime = Time.time + 0.3f;
                    //Debug.Log("Playing Steps sound");
                    AudioManager.instance.Play("Steps");
                }

                _animator.SetFloat(Speed, Mathf.Abs(_horizontalInput));
                if (IsGrounded())
                {
                    _animator.SetBool(IsClimbing, false);
                }

                if (isClimbing && (Time.time > _nextClimb) && moveValue.y > 0.1f)
                {
                    if (Time.time > _nextLadderSound)
                    {
                        AudioManager.instance.Play("Ladder");
                        _nextLadderSound = Time.time + 0.4f;
                    }
                    else
                    {

                    }
                    Climb(moveValue.y);
                    AudioManager.instance.StopPlaying("Steps");
                }

                // Roll - consume the queued press exactly once, same semantics as the old GetButtonDown check
                if (_rollQueued)
                {
                    _rollQueued = false;

                    if (!roll && (currentMana >= _playerScriptable.AdvancedStatistics.rollManaCost) && IsGrounded() && !isClimbing && Mathf.Abs(_horizontalInput) > 0.01)
                    {
                        //Debug.Log("Rolling");
                        AudioManager.instance.StopPlaying("Steps");
                        Roll();
                    }
                    else if ((currentMana < _playerScriptable.AdvancedStatistics.rollManaCost) && IsGrounded() && !isClimbing && Mathf.Abs(_horizontalInput) > 0.01)
                    {
                        transform.Find("MissingMana").gameObject.SetActive(true);
                        Invoke("ResetMissingMana", 0.75f);
                    }
                }
            }
            else
            {
                _horizontalInput = 0f;
            }


        }

        void FixedUpdate()
        {
            if (GameManager.playerControl)
            {
                // Decide if we want to attempt a jump this physics tick
                bool wantJump = (_jumpPressedTime > 0) &&
                                (Time.time - _jumpPressedTime <= JumpBufferTime) &&
                                !roll && !isClimbing;

                // Pass the attempt to the controller; it returns whether it actually jumped
                bool didJump = controller.Move(_horizontalInput * Time.fixedDeltaTime, roll, wantJump);

                if (didJump)
                {
                    _jumpPressedTime = -1f; // clear buffer only when jump actually applied
                    if (!_isAttacking)
                        _animator.SetBool("isJumping", true);
                    AudioManager.instance.Play("Jump");
                }
            }


        }

        //-------------------------------------------------------------------------------------------//

        /*
    MOVEMENT FUNCTIONS
    */
        public void OnLanding()
        {
            _animator.SetBool("isJumping", false);
        }
        public void OnRoll()
        {
            _animator.SetBool("isRolling", false);
        }

        public bool IsGrounded()
        {
            // Use the controller's unified grounded state
            return controller != null && controller.Grounded;
        }
        public void Jump()
        {
            // Kept for compatibility if used elsewhere, but input path no longer calls this.
            AudioManager.instance.Play("Jump");
            if (!_isAttacking)
            {
                _animator.SetBool("isJumping", true);
            }
        }
        public void Roll()
        {
            roll = true;
            AudioManager.instance.Play("Roll");
            _animator.SetBool("isRolling", true);
            currentMana -= _playerScriptable.AdvancedStatistics.rollManaCost;
            manaBar.SetMana(currentMana);
            StartCoroutine(RollDownRoutine());
        }
        public void Climb(float verticalInput)
        {
            _animator.SetBool("isClimbing", true);
            Vector2 climbVelocity = new Vector2(_rb.linearVelocity.x, verticalInput * _playerScriptable.AdvancedStatistics.climbSpeed);
            _rb.linearVelocity = climbVelocity;
        }

        public void Bounce()
        {
            AudioManager.instance.Play("SlimeDeath");
            Debug.Log("Boucing off");
            _hitbox.enabled = false;
            Invoke("EnableHitbox", 0.2f);
            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(new Vector2(0f, _playerScriptable.AdvancedStatistics.bounceForce));
            // animator.SetBool("isJumping", true);
        }

        void EnableHitbox()
        {
            _hitbox.enabled = true;
        }

        public void StopMotion()
        {
            _animator.SetFloat("Speed", 0f);
            _rb.linearVelocity = Vector2.zero;
        }
        //-------------------------------------------------------------------------------------------//

        /*
    COLLIDER FUNCTIONS
    */
        void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("DeathFloor"))
            {

                TakeDamage(_playerScriptable.BaseStats.maxHealth);
            }
            else if (other.gameObject.CompareTag("Spikes"))
            {
                TakeDamage(_playerScriptable.BaseStats.maxHealth / 4);
            }
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Climbable")
            {
                //Debug.Log("We can climb");
                isClimbing = true;
            }

        }
        void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "Climbable")
            {
                _nextClimb = Time.time + 0.5f;
                isClimbing = false;
                _animator.SetBool("isClimbing", false);
            }
        }

        //-------------------------------------------------------------------------------------------//
        // ROUTINES
        //-------------------------------------------------------------------------------------------//

        IEnumerator RollDownRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            roll = false;
        }

        //-------------------------------------------------------------------------------------------//
        //HEALTH & COMBAT FUNCTIONS
        //-------------------------------------------------------------------------------------------//

        public void TakeDamage(int damage)
        {
            if (roll)
            {
            }
            else
            {
                tookDamage = true;
                int randomImpact = Random.Range(0, 2);
                Instantiate(_playerScriptable.impactPrefabs[randomImpact], transform.position, Quaternion.identity);
                _spriteRenderer.color = _hurtColor;
                Invoke("ResetColor", 0.25f);
                _currentHealth -= damage;
                healthBar.Set(_currentHealth);
                if (_currentHealth <= 0)
                {
                    Die();
                }
            }

        }
        void ResetColor()
        {
            _spriteRenderer.color = new Color(255, 255, 255, 255);
        }
        public void Knockback(Collider2D other)
        {
            _rb.linearVelocity = Vector2.zero;
            GameManager.playerControl = false;
            // animator.SetTrigger("Stun");
            Invoke("EnablePlayerControl", .5f);
            Vector2 knockbackDirection = (transform.position - other.transform.position).normalized;

            // Apply the knockback force to the player

            _rb.AddForce(knockbackDirection * 100f, ForceMode2D.Impulse);
        }
        public void IsAttacking()
        {
            _isAttacking = true;
            Invoke("ResetIsAttacking", 0.8f);
        }
        void ResetIsAttacking()
        {
            _isAttacking = false;
        }
        void RegainMana()
        {
            if (currentMana >= _playerScriptable.AdvancedStatistics.maxMana)
            {
                currentMana = 100f;
                manaBar.SetMana(currentMana);
            }
            else
            {
                currentMana += 0.2f;
                manaBar.SetMana(currentMana);
            }
        }
        //-------------------------------------------------------------------------------------------//
        void Die()
        {
            Invoke("GameOverSequence", 1.5f);
            _isDead = true;
            _animator.SetBool("isJumping", false);
            _animator.SetBool("isClimbing", false);
            _animator.SetTrigger("Death");
            if (_playerCombat != null) _playerCombat.enabled = false;
            GetComponent<CharacterController2D>().enabled = false;
            // GetComponent<BoxCollider2D>().enabled = false;
            // GetComponent<CircleCollider2D>().enabled = false;
            Destroy(this.gameObject, 4f);
        }
        //-------------------------------------------------------------------------------------------//
        // UI AND SCORE FUNCTIONS
        //-------------------------------------------------------------------------------------------//
        void GameOverSequence()
        {
            _uiManager.GameOverSequence();
        }
        public void LevelComplete()
        {
            _uiManager.LevelComplete();
        }
        public void ResetMissingMana()
        {
            transform.Find("MissingMana").gameObject.SetActive(false);
        }
        //-------------------------------------------------------------------------------------------//
        // INTERACTION FUNTIONS
        //
        public void AddCoin(int coin)
        {
            coins += coin;
            _uiManager.UpdateCoin(coins);
        }
        public void CollectKey()
        {
            Debug.Log("Collected a key");
            _numKeys++;
        }
        public bool UnlockChest()
        {
            if (_numKeys > 0)
            {
                Debug.Log("Unlocking chest");
                // unlock
                _numKeys--;
                return true;

            }
            else
            {
                Debug.Log("No key");
                // show missing key on top of player
                return false;
            }
        }

        public void HealthPotion()
        {
            _currentHealth += 50;
            if (_currentHealth >= _playerScriptable.BaseStats.maxHealth)
            {
                _currentHealth = _playerScriptable.BaseStats.maxHealth;
            }
            healthBar.Set(_currentHealth);

        }

    }
}