using System.Collections;
using System.Collections.Generic;
using _Scripts.Managers;
using _Scripts.Scriptables;
using _Scripts.Units.Enemy;
using _Scripts.Units.Player;
using UnityEngine;

/// <summary>
/// This class implements the spear behaviour
/// </summary>
public class Spear : MonoBehaviour
{
    public Side side;
    [SerializeField] private float rotationSpeed = 20f;
    Rigidbody2D _rb;
    public ScriptableWeapon w;
    bool _isInAir = true;
    Player _player;

    int _attackDamage;

    [System.Serializable]
    public enum Side
    {
        Player,
        Enemy,
    }

    void Start()
    {
        AudioManager.instance.Play("Throw Spear");
        Destroy(this.gameObject, 5f);
        _rb = GetComponent<Rigidbody2D>();
        _player = GameObject.Find("Player").GetComponent<Player>();
        _attackDamage = w.attackDamage * 2;

    }
    void FixedUpdate()
    {
        // float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x)* Mathf.Rad2Deg;
        // transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        if (_isInAir)
        {
            ChangeSpriteDirection();
        }


    }
    void ChangeSpriteDirection()
    {
        if (_rb.velocity != Vector2.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, _rb.velocity);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Ground" || other.tag == "Obstacle")
        {
            StopMotion();
            CircleCollider2D arrow = GetComponent<CircleCollider2D>();
            arrow.enabled = false;
            _isInAir = false;
            Invoke("StopMotion", 0.05f);

        }
        if (side == Side.Player)
        {
            if (other.tag == "Enemy")
            {
                AudioManager.instance.Play("Spear");
                Enemy enemy = other.GetComponent<Enemy>();
                if (other.GetComponent<Enemy>().enemyScriptable.enemyType == ScriptableEnemy.EnemyType.SkeletonShield)
                {
                    bool facingOpposite = PlayerDir(_player) != EnemyDir(enemy);
                    // If facing opposite directions → block partially
                    if (!facingOpposite)
                    {
                        int reducedDamage = Mathf.RoundToInt(_attackDamage * 0.25f);
                        Debug.Log($"🛡 SkeletonShield blocked spear! Damage reduced from {_attackDamage} → {reducedDamage}");
                        enemy.TakeDamage(reducedDamage);
                        Destroy(this.gameObject);
                        return;
                    }
                }
                enemy.TakeDamage(_attackDamage);

            }
            else if (other.tag == "Crate")
            {
                AudioManager.instance.Play("Spear");
                other.GetComponent<Crate>().TakeDamage(_attackDamage);
            }
            else if (other.tag == "Lever")
            {
                AudioManager.instance.Play("Spear");
                other.GetComponent<Lever>().SwitchLeverState();
            }
        }
        else if (side == Side.Enemy)
        {
            if (other.tag == "PlayerHitbox")
            {
                _player.TakeDamage(_attackDamage);
                Destroy(this.gameObject);
            }
        }
    }
    void StopMotion()
    {
        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0;
    }

    int PlayerDir(Player p)
    {
        // Player uses _mFacingRight from CharacterController2D
        return p.controller._mFacingRight ? 1 : -1;
    }

    int EnemyDir(Enemy e)
    {
        // Enemy uses localScale
        return e.transform.localScale.x >= 0 ? 1 : -1;
    }
}
