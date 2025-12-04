using UnityEngine;

public class CloseDoorOnTrigger : MonoBehaviour
{
    [Header("Child object to activate on trigger")]
    public GameObject childToActivate; // assign in Inspector, or leave null to auto-find

    private void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player" && childToActivate != null)
        {
            childToActivate.SetActive(true);
            Debug.Log($"{childToActivate.name} activated by trigger!");
        }
    }
}