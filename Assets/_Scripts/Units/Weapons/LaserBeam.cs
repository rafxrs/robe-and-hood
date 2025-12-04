using System;
using _Scripts.Units.Player;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [Tooltip("Speed of the laser in units/sec")]
    public float speed = 10f;
    [Tooltip("Seconds before auto-destroy")]
    public float lifeTime = 2f;

    private Vector2 _direction;

    void Start()
    {
        Debug.Log("Created a laser");
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(Vector2 direction)
    {
        _direction = direction.normalized;
        // rotate sprite to face movement
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        transform.Translate(_direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Hit player with laser");
            other.gameObject.GetComponent<Player>().TakeDamage(50);
            Destroy(gameObject);
        }
    }
}