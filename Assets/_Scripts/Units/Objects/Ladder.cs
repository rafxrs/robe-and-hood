
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    // EdgeCollider2D topOfLadder;
    // Start is called before the first frame update
    void Start()
    {
        // topOfLadder = GameObject.Find("LadderTop").GetComponent<EdgeCollider2D>();
        // topOfLadder.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag=="Player")
        {
            Debug.Log("We reached the top");
            // topOfLadder.enabled = true;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag=="Player")
        {
            Debug.Log("We left the top");
            // topOfLadder.enabled = false;
        }
    }
}
