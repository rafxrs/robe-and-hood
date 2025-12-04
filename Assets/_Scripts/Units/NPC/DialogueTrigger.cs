using System.Collections;
using System.Collections.Generic;
using _Scripts.Managers;
using _Scripts.Units.Player;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    public void TriggerDialogue(GameObject doActionOn)
    {
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue, doActionOn);
        FindObjectOfType<Player>().StopMotion();
        FindObjectOfType<GameManager>().DisablePlayerControl();
    }
}
