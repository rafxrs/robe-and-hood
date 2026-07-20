using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
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

        //-------------------------------------------------------------------------------------------//
        // INPUT SYSTEM ACTION
        // Advance - Space (advances to the next sentence)
        //-------------------------------------------------------------------------------------------//
        private InputAction _advanceAction;
        private bool _advanceQueued;

        private void Awake()
        {
            _advanceAction = new InputAction("Advance", InputActionType.Button);
            _advanceAction.AddBinding("<Keyboard>/space");
        }

        private void OnEnable()
        {
            _advanceAction.performed += OnAdvancePerformed;
            _advanceAction.Enable();
        }

        private void OnDisable()
        {
            _advanceAction.performed -= OnAdvancePerformed;
            _advanceAction.Disable();
        }

        private void OnDestroy()
        {
            _advanceAction?.Dispose();
        }

        private void OnAdvancePerformed(InputAction.CallbackContext ctx) => _advanceQueued = true;

        // Start is called before the first frame update
        void Start()
        {
            _sentences = new Queue<string>();
        }

        void Update()
        {
            // Snapshot and immediately clear the queued press so it's only ever consumed on the
            // exact frame it happened - matches the old Input.GetKeyDown one-frame-pulse behaviour.
            bool advancePressed = _advanceQueued;
            _advanceQueued = false;

            if (advancePressed && inDialogue)
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
            Debug.Log("Starting conversation with " + dialogue.name);

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
            foreach (var letter in sentence.ToCharArray())
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