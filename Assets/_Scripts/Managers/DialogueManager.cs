using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Managers
{
    public class DialogueManager : MonoBehaviour
    {
        public Text nameText;
        public Text dialogueText;
        public Animator animator;
        private GameObject _doActionOn;
    

        [SerializeField] bool inDialogue;

        Queue<string> _sentences;

        private static readonly int IsOpen = Animator.StringToHash("isOpen");

        // Start is called before the first frame update
        void Start()
        {
            _sentences = new Queue<string>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && inDialogue)
            {
                DisplayNextSentence();
            }
        }

        public void StartDialogue(Dialogue dialogue, GameObject doActionOn)
    
        {
            inDialogue = true;
            animator.SetBool(IsOpen, true);
            nameText.text = dialogue.name;
            _doActionOn = doActionOn;
            Debug.Log("Starting conversation with "+dialogue.name);

            _sentences.Clear();

            foreach (string sentence in dialogue.sentences)
            {
                _sentences.Enqueue(sentence);
            }
            DisplayNextSentence();
        }

        public void DisplayNextSentence()
        {
            if (_sentences is { Count: 0 })
            {
                EndDialogue();
            }
            
            else 
            {
                if (_sentences != null)
                {
                    var sentence = _sentences.Dequeue();
                    StopAllCoroutines();
                    StartCoroutine(TypeSentence(sentence));
                }
            }
        }

        private IEnumerator TypeSentence([NotNull] string sentence)
        {
            if (sentence == null) throw new ArgumentNullException(nameof(sentence));
            dialogueText.text = "";
            foreach(var letter in sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return null;
            }
        }

        void EndDialogue()
        {
            inDialogue = false;
            animator.SetBool(IsOpen, false);
            if (_doActionOn != null)
            {
                _doActionOn.SetActive(false);
            }
            Debug.Log("End of conversation");
            FindObjectOfType<GameManager>().EnablePlayerControl();
        }

    
    }
}