using System.Collections;
using System.Collections.Generic;
using _Scripts.Scriptables;
using UnityEngine;

public class Crate : MonoBehaviour
{
    public int maxHealth = 50;
    int _currentHealth;
    [SerializeField] private GameObject[] impactPrefabs;
    Animator _animator;
    RewardSpawner _rewardSpawner;
    [SerializeField] int amountOfRewards = 1;


    // Start is called before the first frame update
    void Start()
    {
        _currentHealth = maxHealth;
        _animator = GetComponent<Animator>();
        _rewardSpawner = GetComponent<RewardSpawner>();
        impactPrefabs[0] = Resources.Load<GameObject>("Prefabs/FX/Impacts/ImpactFX1");
        impactPrefabs[1] = Resources.Load<GameObject>("Prefabs/FX/Impacts/ImpactFX2");
    }


    void SpawnRewards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            _rewardSpawner.Reward(transform.position + new Vector3(0, 0.3f, 0));
        }

    }

    void OntriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Weapon")
        {
            ScriptableWeapon w = other.GetComponent<ScriptableWeapon>();
            TakeDamage(w.attackDamage);
        }
    }
    public void TakeDamage(int attackDamage)
    {
        _currentHealth -= attackDamage;
        int rand = Random.Range(0, 2);
        Instantiate(impactPrefabs[rand], transform.position, Quaternion.identity);
        if (_currentHealth <= 0)
        {
            GetComponent<BoxCollider2D>().enabled = false;
            Debug.Log("Destroy");
            _animator.SetTrigger("Destroy");
            SpawnRewards(amountOfRewards);
            Destroy(this.gameObject, 1.5f);

        }
    }




}
