using System.Collections;
using System.Collections.Generic;
using _Scripts.Units.Player;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    // Start is called before the first frame update
    public Type type;
    Player _player;
    [SerializeField] bool isReward;
    public GameObject destroyFX;
    [SerializeField] float pickupTime = 0.0f;
    float _awakeTime;

    [System.Serializable]
    public enum Type
    {
        Coin,
        Key,
        HealthPotion,
    }

    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
    }

    void Awake()
    {
        _awakeTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("CoinCollider"))
        {
            if (!isReward)
            {
                Collect();
            }
            else if (Time.time > _awakeTime + pickupTime)
            {
                Collect();
            }
        }
    }

    void Collect()
    {
        switch (type)
        {
            case Type.Coin:
                Instantiate(destroyFX, transform.position, Quaternion.identity);
                _player.AddCoin(1);
                break;
            case Type.Key:
                Instantiate(destroyFX, transform.position, Quaternion.identity);
                _player.CollectKey();
                break;
            case Type.HealthPotion:
                _player.HealthPotion();
                break;
        }

        Destroy(gameObject);
    }

}