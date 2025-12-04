using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Managers;
using UnityEngine;
using Random = UnityEngine.Random;


// Attach this to any game object that can spawn prefabs for whatever reason
// (be it coins, gold, explosion particles, etc.).
public class RewardSpawner : MonoBehaviour
{
    public RewardType rewardType;
    [SerializeField] private int amountOfRewards = 1;

    [System.Serializable]
    public enum RewardType
    {
        Coin,
        Key,
        EndOfLevel,
        Weapon,
        Potion,

    }

    [SerializeField] GameObject rewardPrefab;
    void Start()
    {
        if (rewardPrefab == null)
        {
            if (rewardType == RewardType.Coin)
            {
                rewardPrefab = Resources.Load<GameObject>("Prefabs/Collectibles/RewardCoin");
            }
        }

        if (rewardPrefab == null)
        {
            Debug.LogError("No reward");
        }
    }

    public void Reward(Vector2 spawnPosition)
    {
        for (int i = 0; i < amountOfRewards; i++)
        {
            switch (rewardType)
            {
                case RewardType.Coin:
                    float randomOffset = Random.Range(-0.15f, 0.15f);
                    Instantiate(rewardPrefab, new Vector3(spawnPosition.x + randomOffset, spawnPosition.y, 0), Quaternion.identity);
                    break;
                case RewardType.Key:
                    Instantiate(rewardPrefab, new Vector3(spawnPosition.x, spawnPosition.y, 0), Quaternion.identity);
                    break;
                case RewardType.EndOfLevel:
                    GameObject.Find("GameManager").GetComponent<GameManager>().LevelComplete();
                    break;
                case RewardType.Potion:
                    Instantiate(rewardPrefab, new Vector3(spawnPosition.x, spawnPosition.y + 0.3f, 0), Quaternion.identity);
                    break;
                case RewardType.Weapon:
                    break;
                default:
                    break;
            }
        }


    }
}
