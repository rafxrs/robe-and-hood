using _Scripts.Managers;
using _Scripts.Scriptables;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Units.Player
{
    /// <summary>
    /// This class is the main class for any attacking related code for the player. It deals with input and sets the correct weapons on the animator
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {

        // -------------------------------------------------------------------------------------------------------------------------- //
        // PUBLIC VARIABLES
        // -------------------------------------------------------------------------------------------------------------------------- //

        public ScriptableWeapon[] weapons;
        public ScriptableWeapon weapon;
        public RuntimeAnimatorController defaultController;
        public Transform[] attackPoints;
        public LayerMask enemyLayer, interactableLayer;

        // -------------------------------------------------------------------------------------------------------------------------- //
        Player _player;
        Animator _animator;
        [FormerlySerializedAs("_weaponName")][SerializeField] string weaponName;
        [FormerlySerializedAs("_attackRange")]
        [SerializeField]
        private float attackRange;
        [FormerlySerializedAs("_attackRate")][SerializeField] float attackRate;
        [FormerlySerializedAs("_nextAttackTime")][SerializeField] float nextAttackTime;
        [FormerlySerializedAs("_attackDamage")][SerializeField] int attackDamage;
        float _comboTime;
        readonly float _throwSpearManaCost = 75f;
        int _combo;
        bool _isCombo;
        private static readonly int IsJumping = Animator.StringToHash("isJumping");
        private static readonly int Attack1 = Animator.StringToHash("Attack1");
        private static readonly int Attack3 = Animator.StringToHash("Attack3");
        private bool _queuedFireArrow;
        private float _queuedManaCost;
        private bool _shotQueued;

        // -------------------------------------------------------------------------------------------------------------------------- //
        void Start()
        {
            _animator = GetComponent<Animator>(); NullCheck.CheckNull(_animator);
            _player = GetComponent<Player>();
            _animator.runtimeAnimatorController = defaultController;
        }

        // Update is called once per frame
        void Update()
        {
            if (weapon != null && GameManager.playerControl && Time.time > nextAttackTime && !_player.roll && !(_player.isClimbing && (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))))
            {
                // --- Bow Controls ---
                if (weaponName == "Bow")
                {
                    // NORMAL: Space or LMB
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                        QueueBowShot(fire: false);

                    // FIRE: Shift or RMB
                    else if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1))
                        QueueBowShot(fire: true);
                }


                // --- Spear Controls ---
                else if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1)) && weaponName == "Spear")
                {
                    if (_player.currentMana >= _throwSpearManaCost)
                    {
                        if (!_player.IsGrounded()) _animator.SetBool(IsJumping, false);
                        ThrowSpearAnimation();
                        nextAttackTime = Time.time + attackRate;
                        _player.IsAttacking();
                    }

                    else
                    {
                        ShowMissingMana();
                    }
                }

                // --- Sword / Melee Controls ---
                else if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    if (_combo < 2 && _player.currentMana >= 10f || (_combo >= 2 && _player.currentMana >= 50f))
                    {
                        AttackAnimation();
                        _player.IsAttacking();
                        nextAttackTime = Time.time + attackRate;
                    }
                    else
                    {
                        ShowMissingMana();
                    }
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------- //
        void AttackAnimation()
        {

            int randomAttack = Random.Range(1, 3);
            string attackNumber = "Attack" + randomAttack;
            if (weaponName == "Sword")
            {
                _animator.SetBool(IsJumping, false);
                AudioManager.instance.Play("SwordAttack");

                _combo++; // increment combo count
                Debug.Log("Combo count: " + _combo);

                // Every third attack = SUPER ATTACK
                if (_combo >= 3)
                {
                    Debug.Log("💥 Super Attack triggered!");
                    _animator.SetTrigger("Attack3");
                    Invoke(nameof(FinishAttack3), 0.5f);
                    _combo = 0; // reset combo

                }
                else
                {
                    _isCombo = false;
                    _animator.SetTrigger(attackNumber);
                }
            }
            else
            {
                _animator.SetTrigger(attackNumber);
                _animator.SetBool(IsJumping, false);
            }

        }
        // -------------------------------------------------------------------------------------------------------------------------- //
        void Attack()
        {
            // Debug.Log("Attacking");
            Attack(weaponName, transform, _isCombo);
        }
        public void Attack(string name, Transform player, bool combo)
        {
            switch (name)
            {
                case "Spear":
                    _player.currentMana -= 15f;
                    _player.manaBar.SetMana(_player.currentMana);

                    AudioManager.instance.Play("Spear");
                    //Debug.Log("Spear Attack");
                    Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoints[0].position, new Vector3(weapon.attackRange, 1, 1), 0f, enemyLayer);
                    Collider2D[] hitObjects = Physics2D.OverlapBoxAll(attackPoints[0].position, new Vector3(weapon.attackRange, 1, 1), 0f, interactableLayer);

                    foreach (Collider2D enemy in hitEnemies)
                    {
                        Debug.Log("Spear hit " + enemy.name);
                        enemy.GetComponent<Enemy.Enemy>().TakeDamage(weapon.attackDamage);
                    }
                    foreach (Collider2D obj in hitObjects)
                    {
                        Debug.Log("Spear hit " + obj.name);
                        switch (obj.tag)
                        {
                            case "Crate":
                                obj.GetComponent<Crate>().TakeDamage(weapon.attackDamage);
                                break;
                            case "Lever":
                                obj.GetComponent<Lever>().SwitchLeverState();
                                break;
                        }

                    }
                    break;
                case "Bow":
                    break;
                case "Sword":
                    if (!combo)
                    {
                        _player.currentMana -= 10f;
                        _player.manaBar.SetMana(_player.currentMana);
                    }
                    else
                    {
                        _player.currentMana -= 50f;
                        _player.manaBar.SetMana(_player.currentMana);
                    }


                    // Debug.Log("Sword Attack");
                    hitEnemies = Physics2D.OverlapCircleAll(attackPoints[1].position, weapon.attackRange, enemyLayer);
                    hitObjects = Physics2D.OverlapCircleAll(attackPoints[1].position, weapon.attackRange, interactableLayer);

                    foreach (Collider2D enemy in hitEnemies)
                    {
                        Debug.Log("Sword hit " + enemy.name);
                        if (!combo)
                        {
                            enemy.GetComponent<Enemy.Enemy>().TakeDamage(weapon.attackDamage);
                        }
                        else
                        {
                            Debug.Log("COMBO!");
                            enemy.GetComponent<Enemy.Enemy>().TakeDamage((weapon.attackDamage) * 3);
                        }
                    }
                    foreach (Collider2D obj in hitObjects)
                    {
                        Debug.Log("Sword hit " + obj.name);
                        switch (obj.tag)
                        {
                            case "Crate":
                                obj.GetComponent<Crate>().TakeDamage(!combo ? weapon.attackDamage : weapon.attackDamage * 3);
                                break;
                            case "Lever":
                                obj.GetComponent<Lever>().SwitchLeverState();
                                break;
                        }

                    }
                    break;
            }
        }

        // -------------------------------------------------------------------------------------------------------------------------- //
        void ThrowSpearAnimation()
        {
            _animator.SetBool(Attack3, true);
            Invoke(nameof(FinishAttack3), 0.5f);
        }
        void ThrowSpear()
        {
            _player.currentMana -= _throwSpearManaCost;
            _player.manaBar.SetMana(_player.currentMana);

            Rigidbody2D spearInstance = Instantiate(
                weapon.weaponPrefab[0],
                transform.position,
                Quaternion.identity
            );

            // Facing direction based on player flip
            int facing = _player.controller._mFacingRight ? 1 : -1;

            // Flip spear visually
            Vector3 scale = spearInstance.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facing;
            spearInstance.transform.localScale = scale;

            // Corrected throw direction (independent of transform.right)
            Vector2 throwDirection = new Vector2(facing, 0f);

            // Arc calculation
            float H = weapon.launchForce * weapon.launchDuration;
            float V = weapon.launchArcHeight;

            Vector2 initialVelocity = CalculateInitialVelocity(
                throwDirection,
                H,
                V
            );

            spearInstance.velocity = initialVelocity;
        }
        // -------------------------------------------------------------------------------------------------------------------------- //
        private void TryShootArrow(bool fireArrow)
        {
            float manaCost = fireArrow ? 50f : 25f;  // fire arrow = half cost of total mana
            if (_player.currentMana < manaCost)
            {
                ShowMissingMana();
                return;
            }

            _player.currentMana -= manaCost;
            _player.manaBar.SetMana(_player.currentMana);

            // Audio + animation
            AudioManager.instance.StopPlaying("Bow Load");
            AudioManager.instance.Play("Bow Load");
            _animator.SetTrigger(Attack1);
            _player.IsAttacking();
            nextAttackTime = Time.time + attackRate;

            // Fire the arrow with modified stats
            ShootArrow(fireArrow, manaCost);
        }

        void ShootArrow(bool fireArrow, float manaCost)
        {
            // Select correct projectile prefab
            int arrowIndex = 0;
            Rigidbody2D arrowInstance = Instantiate(weapon.weaponPrefab[arrowIndex], transform.position, Quaternion.identity);

            // Apply velocity
            Vector2 throwDirection = transform.right;
            float horizontalDistance = weapon.launchForce * weapon.launchDuration;
            float verticalDistance = weapon.launchArcHeight;
            Vector2 initialVelocity = CalculateInitialVelocity(throwDirection, horizontalDistance, verticalDistance);
            arrowInstance.velocity = initialVelocity;

            Debug.Log($"{(fireArrow ? "🔥 Fire" : "🏹 Normal")} arrow shot! Mana used: {manaCost}");
        }

        private void QueueBowShot(bool fire)
        {
            float manaCost = fire ? 50f : 25f;

            if (_player.currentMana < manaCost)
            {
                ShowMissingMana();
                return;
            }

            // Deduct now so animation event doesn’t need UI logic
            _player.currentMana -= manaCost;
            _player.manaBar.SetMana(_player.currentMana);

            // Queue shot info for the animation event
            _queuedFireArrow = fire;
            _queuedManaCost = manaCost;
            _shotQueued = true;

            // Play bow draw/fire animation
            AudioManager.instance.StopPlaying("Bow Load");
            AudioManager.instance.Play("Bow Load");
            _animator.SetBool(IsJumping, false); // prevent the cancellation of the bow animation
            _animator.SetTrigger(Attack1);
            _player.IsAttacking();
            nextAttackTime = Time.time + attackRate;

            Debug.Log($"{(fire ? "🔥 Fire" : "🏹 Normal")} arrow queued. Mana used: {manaCost}");
        }

        // <<< ANIMATION EVENT >>>  
        // Add this event on the bow attack animation at the frame the arrow should spawn.
        public void ShootArrowEvent()
        {
            if (!_shotQueued)
            {
                // nothing queued (e.g., event left on another clip) – just ignore
                return;
            }

            // choose which ScriptableWeapon to use
            ScriptableWeapon activeWeapon = _queuedFireArrow ? _player.GetComponent<PlayerCombat>().weapons[3]
                                                            : _player.GetComponent<PlayerCombat>().weapons[1];

            // spawn from that weapon's prefab[0]
            if (activeWeapon.weaponPrefab == null || activeWeapon.weaponPrefab.Length == 0 || activeWeapon.weaponPrefab[0] == null)
            {
                Debug.LogError($"Missing prefab on {activeWeapon.name}");
                _shotQueued = false;
                return;
            }

            Rigidbody2D arrowInstance = Instantiate(
                activeWeapon.weaponPrefab[0],
                transform.position,
                Quaternion.identity
            );


            // Give it velocity (same arc as before)
            int facing = _player.controller._mFacingRight ? 1 : -1;
            Vector2 dir = transform.right * facing;
            float H = weapon.launchForce * weapon.launchDuration;
            float V = weapon.launchArcHeight;
            Vector2 v0 = CalculateInitialVelocity(dir, H, V);
            arrowInstance.velocity = v0;

            Debug.Log($"{(_queuedFireArrow ? "🔥 Fire" : "🏹 Normal")} arrow shot! Mana used: {_queuedManaCost}");

            // Clear the queue
            _shotQueued = false;
        }



        // -------------------------------------------------------------------------------------------------------------------------- //
        private Vector2 CalculateInitialVelocity(Vector2 direction, float horizontalDistance, float verticalDistance)
        {
            // Calculate the initial velocity components for the desired arc
            float horizontalVelocity = horizontalDistance / weapon.launchDuration;
            float verticalVelocity = (verticalDistance - (0.5f * Physics2D.gravity.y * Mathf.Pow(weapon.launchDuration, 2f))) / weapon.launchDuration;

            // Combine the components with the direction vector
            Vector2 initialVelocity = direction * horizontalVelocity;
            initialVelocity.y = verticalVelocity;

            return initialVelocity;
        }
        // -------------------------------------------------------------------------------------------------------------------------- //
        void SwordComboAttack()
        {
            Debug.Log("combo");
            _isCombo = true;
            Attack();
            Invoke(nameof(FinishCombo), 1f);
        }
        void FinishCombo()
        {
            _isCombo = false;
        }
        private void FinishAttack3()
        {
            _animator.SetBool(Attack3, false);
        }
        // -------------------------------------------------------------------------------------------------------------------------- //
        private void SetWeapon(ScriptableWeapon w)
        {
            weaponName = w.name;
            weapon = w;
            attackDamage = w.attackDamage;
            attackRate = w.attackRate;
            attackRange = w.attackRange;
        }
        public void ChooseWeapon(int number) //0 is spear, 1 is bow, 2 is sword
        {
            AudioManager.instance.Play("Equip");
            weapon = weapons[number];
            ChangeController(weapon);
            SetWeapon(weapon);
        }

        public void ChooseWeaponNoSound(int number) //0 is spear, 1 is bow, 2 is sword
        {
            weapon = weapons[number];
            ChangeController(weapon);
            SetWeapon(weapon);
        }
        private void ChangeController(ScriptableWeapon scriptableWeapon)
        {
            _animator.runtimeAnimatorController = scriptableWeapon.controller;
        }
        // -------------------------------------------------------------------------------------------------------------------------- //

        void ResetMissingMana()
        {
            transform.Find("MissingMana").gameObject.SetActive(false);
        }

        private void ShowMissingMana()
        {
            transform.Find("MissingMana").gameObject.SetActive(true);
            Invoke(nameof(ResetMissingMana), 1f);
        }



    }
}
