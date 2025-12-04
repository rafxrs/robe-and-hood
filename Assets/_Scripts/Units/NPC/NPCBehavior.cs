using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBehavior : MonoBehaviour
{
    Animator _animator;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        if (this.gameObject.name == "NPC_Blacksmith")
        {
            _animator.SetBool("isSmithing",true);
            InvokeRepeating("FlipSmith",5f,Random.Range(5f,10f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FlipSmith()
    {
        if (_animator.GetBool("isSmithing"))
        {
            _animator.SetBool("isSmithing",false);
        }
        else 
        {
            _animator.SetBool("isSmithing",true);
        }
        
    }
}
