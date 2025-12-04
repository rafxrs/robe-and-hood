using _Scripts.Managers;
using _Scripts.Units.Objects;
using UnityEngine;

namespace _Scripts.Units.Player
{
    public class DetectPlayer : MonoBehaviour
    {
        public bool playerDetected;
        // public Transform areaPos;
        // public float areaWidth;
        // public float areaHeight;
        // public LayerMask whatIsPlayer;
        public Action action;
        public GameObject doActionOn;
        GameObject _player;
        Transform _playerTransform;
        GameManager _gameManager;
        private static readonly int Open = Animator.StringToHash("Open");


        // Start is called before the first frame update
        void Start()
        {
            _player = GameObject.Find("Player");
            _playerTransform = _player.GetComponent<Transform>();
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        // Update is called once per frame
        void Update()
        {
            // playerDetected = Physics2D.OverlapBox(areaPos.position, new Vector2(areaWidth, areaHeight),0,whatIsPlayer);

            if (playerDetected)
            {
                // do action
                switch (action)
                {
                    case Action.Dialogue:
                        _gameManager.ShowEButton();
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            if (doActionOn != null)
                            {
                                doActionOn.SetActive(true);
                            }
                            GetComponent<DialogueTrigger>().TriggerDialogue(doActionOn);
                            _gameManager.HideEButton();
                        }
                        break;
                    case Action.Door:
                        _gameManager.ShowEButton();
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            Debug.Log("Opening the door");
                            _playerTransform.position = doActionOn.transform.position;
                            _gameManager.HideEButton();
                        }
                        break;
                    case Action.Tutorial:
                        GetComponent<DialogueTrigger>().TriggerDialogue(doActionOn);
                        break;
                    case Action.SelectWeapon:
                        _gameManager.ShowEButton();
                        // highlight specific weapon
                        doActionOn.SetActive(true);
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            int num=0;
                            switch (this.name)
                            {
                                case "Spear":
                                    num = 0;
                                    break;
                                case "Bow":
                                    num = 1;
                                    break;
                                case "Sword":
                                    num = 2;
                                    break;
                            }
                            _player.GetComponent<PlayerCombat>().ChooseWeapon(num);
                            FindObjectOfType<AudioManager>().Play("Equip");
                            // GameObject ws = GameObject.Find("WeaponSelect");
                            // Destroy(ws);
                            Debug.Log("Selecting Weapon");
                            // doActionOn.SetActive(false);
                            _gameManager.HideEButton();
                        }
                        break;
                    case Action.EndOfLevel:
                        _gameManager.ShowEButton();
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            Debug.Log("Finishing level");
                            _gameManager.LevelComplete(); 
                            Destroy(this.gameObject);
                        }
                        break;
                    case Action.Chest:
                        _gameManager.ShowEButton();
                        // if key, unlock chest;
                        // if no key, show missing key
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            bool unlocked = _player.GetComponent<Player>().UnlockChest();
                            if (unlocked)
                            {
                                Animator animator = GetComponent<Animator>();
                                if (animator !=null)
                                {
                                    animator.SetTrigger(Open);
                                }
                                GetComponent<RewardSpawner>().Reward(transform.position);
                                _gameManager.HideEButton();
                                this.enabled = false;
                            }
                            else 
                            {
                                _player.transform.Find("MissingKey").gameObject.SetActive(true);
                                Invoke(nameof(HideMissingKey), 1f);
                            }
                        
                            _gameManager.HideEButton();
                        }
                        break;
                    case Action.Lever:
                        _gameManager.ShowEButton();
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            Debug.Log("Switching lever");
                            GetComponent<Lever>().SwitchLeverState();
                        }
                        break;
                    case Action.Teleport:
                        Debug.Log("Teleporting the player");
                        _playerTransform.position = doActionOn.transform.position;
                        _gameManager.HideEButton();
                        break;
                    case Action.ShowInfo:
                        doActionOn.SetActive(true);
                        break;
                    case Action.MovingPlatform:
                        _gameManager.ShowEButton();
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            Debug.Log("Activating Platform");
                            doActionOn.GetComponent<MovingPlatform>().Activate();
                        }
                        break;
                }
            
            }
        }   
    
        [System.Serializable]
        public enum Action
        {
            Dialogue,
            Door,
            Tutorial,
            SelectWeapon,
            EndOfLevel,
            Chest,
            Lever,
            Teleport,
            ShowInfo,
            MovingPlatform,
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerDetected = true;
            }
        }
        void OnTriggerExit2D(Collider2D other)
        {
        
            if (other.CompareTag("Player"))
            {
                //Debug.Log("Player left the collider");
                playerDetected = false;
                _gameManager.HideEButton();

                if (action == Action.SelectWeapon || action == Action.ShowInfo)
                {
                    doActionOn.SetActive(false);
                }
            }
        }

        void HideMissingKey()
        {
            _player.transform.Find("MissingKey").gameObject.SetActive(false);
        }
        // void OnDrawGizmosSelected()
        // {
        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawWireCube(areaPos.position, new Vector3(areaWidth,areaHeight,1));
        // }
    }
}
