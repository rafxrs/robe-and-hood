using System.Collections;
using _Scripts.Units.Objects;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public Action action;
    public bool leverState = false;
    public GameObject[] doActionOn;

    private float _timeBetweenSwitch = 1f;
    private float _nextSwitch = -1f;
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)  // <-- fixed capitalization (was OntriggerEnter2D)
    {
        if (other.CompareTag("Weapon"))
        {
            SwitchLeverState();
        }
    }

    public void SwitchLeverState()
    {
        if (Time.time > _nextSwitch)
        {
            leverState = !leverState;
            _animator.SetBool("State", leverState);
            DoAction();
            _nextSwitch = Time.time + _timeBetweenSwitch;
        }
    }

    [System.Serializable]
    public enum Action
    {
        Door,
        Platform,
        MovingPlatform,
    }

    private void DoAction()
    {
        switch (action)
        {
            case Action.Door:
                foreach (GameObject gobj in doActionOn)
                    gobj.SetActive(!leverState);
                break;

            case Action.Platform:
                foreach (GameObject gobj in doActionOn)
                    gobj.SetActive(leverState);
                break;

            case Action.MovingPlatform:
                foreach (GameObject gobj in doActionOn)
                {
                    var mp = gobj.GetComponent<MovingPlatform>();
                    if (mp != null)
                    {
                        mp.Activate();
                    }
                }
                break;

        }
    }
}
