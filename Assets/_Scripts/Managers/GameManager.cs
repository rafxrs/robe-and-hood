using System;
using _Scripts.Units.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace _Scripts.Managers
{
    public class GameManager : StaticInstance<GameManager>
    {
        //-------------------------------------------------------------------------------------------//
        public static event Action<GameState> OnBeforeStateChanged;
        public static event Action<GameState> OnAfterStateChanged;

        public int starsCount;
        public bool isGameOver;
        public bool isPaused;
        public bool levelComplete;
        public bool mustChooseWeapon;
        public static bool playerControl;

        public GameObject pausePanel;
        public GameObject gameOverPanel;
        public GameObject chooseWeaponPanel;
        public GameObject levelCompletePanel;
        [FormerlySerializedAs("Stars")] public GameObject[] stars = new GameObject[3];
        public GameObject eButton;
        Player _player;

        //-------------------------------------------------------------------------------------------//
        // INPUT SYSTEM ACTIONS
        // Confirm  - Space (advance to next level once it's complete)
        // Restart  - R
        // Menu     - M
        // Pause    - P / Escape (toggles pause)
        // Weapon1/2/3 - 1 / 2 / 3 (weapon choice screen)
        //-------------------------------------------------------------------------------------------//
        private InputAction _confirmAction;
        private InputAction _restartAction;
        private InputAction _menuAction;
        private InputAction _pauseAction;
        private InputAction _weapon1Action;
        private InputAction _weapon2Action;
        private InputAction _weapon3Action;

        // One-frame latches, mirroring the old Input.GetKeyDown pulse semantics
        private bool _confirmQueued;
        private bool _restartQueued;
        private bool _menuQueued;
        private bool _pauseQueued;
        private bool _weapon1Queued;
        private bool _weapon2Queued;
        private bool _weapon3Queued;

        // NOTE: assumes StaticInstance<T> declares "protected virtual void Awake()".
        // If your base class differs, move this action-construction block into Start() instead.
        protected override void Awake()
        {
            base.Awake();

            _confirmAction = new InputAction("Confirm", InputActionType.Button);
            _confirmAction.AddBinding("<Keyboard>/space");

            _restartAction = new InputAction("Restart", InputActionType.Button);
            _restartAction.AddBinding("<Keyboard>/r");

            _menuAction = new InputAction("Menu", InputActionType.Button);
            _menuAction.AddBinding("<Keyboard>/m");

            _pauseAction = new InputAction("Pause", InputActionType.Button);
            _pauseAction.AddBinding("<Keyboard>/p");
            _pauseAction.AddBinding("<Keyboard>/escape");

            _weapon1Action = new InputAction("Weapon1", InputActionType.Button);
            _weapon1Action.AddBinding("<Keyboard>/1");

            _weapon2Action = new InputAction("Weapon2", InputActionType.Button);
            _weapon2Action.AddBinding("<Keyboard>/2");

            _weapon3Action = new InputAction("Weapon3", InputActionType.Button);
            _weapon3Action.AddBinding("<Keyboard>/3");
        }

        private void OnEnable()
        {
            _confirmAction.performed += OnConfirmPerformed;
            _confirmAction.Enable();

            _restartAction.performed += OnRestartPerformed;
            _restartAction.Enable();

            _menuAction.performed += OnMenuPerformed;
            _menuAction.Enable();

            _pauseAction.performed += OnPausePerformed;
            _pauseAction.Enable();

            _weapon1Action.performed += OnWeapon1Performed;
            _weapon1Action.Enable();

            _weapon2Action.performed += OnWeapon2Performed;
            _weapon2Action.Enable();

            _weapon3Action.performed += OnWeapon3Performed;
            _weapon3Action.Enable();
        }

        private void OnDisable()
        {
            _confirmAction.performed -= OnConfirmPerformed;
            _confirmAction.Disable();

            _restartAction.performed -= OnRestartPerformed;
            _restartAction.Disable();

            _menuAction.performed -= OnMenuPerformed;
            _menuAction.Disable();

            _pauseAction.performed -= OnPausePerformed;
            _pauseAction.Disable();

            _weapon1Action.performed -= OnWeapon1Performed;
            _weapon1Action.Disable();

            _weapon2Action.performed -= OnWeapon2Performed;
            _weapon2Action.Disable();

            _weapon3Action.performed -= OnWeapon3Performed;
            _weapon3Action.Disable();
        }

        private void OnDestroy()
        {
            _confirmAction?.Dispose();
            _restartAction?.Dispose();
            _menuAction?.Dispose();
            _pauseAction?.Dispose();
            _weapon1Action?.Dispose();
            _weapon2Action?.Dispose();
            _weapon3Action?.Dispose();
        }

        private void OnConfirmPerformed(InputAction.CallbackContext ctx) => _confirmQueued = true;
        private void OnRestartPerformed(InputAction.CallbackContext ctx) => _restartQueued = true;
        private void OnMenuPerformed(InputAction.CallbackContext ctx) => _menuQueued = true;
        private void OnPausePerformed(InputAction.CallbackContext ctx) => _pauseQueued = true;
        private void OnWeapon1Performed(InputAction.CallbackContext ctx) => _weapon1Queued = true;
        private void OnWeapon2Performed(InputAction.CallbackContext ctx) => _weapon2Queued = true;
        private void OnWeapon3Performed(InputAction.CallbackContext ctx) => _weapon3Queued = true;

        //-------------------------------------------------------------------------------------------//
        [Serializable]
        public enum GameState
        {
            Starting = 0,
            SpawningHeroes = 1,
            SpawningEnemies = 2,
            HeroTurn = 3,
            EnemyTurn = 4,
            Win = 5,
            Lose = 6,
            Menu = 7,
        }

        //-------------------------------------------------------------------------------------------//
        // Kick the game off with the first state
        void Start() => ChangeState(GameState.Starting);

        // Update is called once per frame
        void Update()
        {
            // Snapshot and immediately clear each queued press so it's only ever consumed on the
            // exact frame it happened - matches the old GetKeyDown one-frame-pulse behaviour.
            bool confirmPressed = _confirmQueued; _confirmQueued = false;
            bool restartPressed = _restartQueued; _restartQueued = false;
            bool menuPressed = _menuQueued; _menuQueued = false;
            bool pausePressed = _pauseQueued; _pauseQueued = false;
            bool weapon1Pressed = _weapon1Queued; _weapon1Queued = false;
            bool weapon2Pressed = _weapon2Queued; _weapon2Queued = false;
            bool weapon3Pressed = _weapon3Queued; _weapon3Queued = false;

            // if level complete and press space, go to next level
            if (confirmPressed && levelComplete)
            {
                LoadNextLevel();
            }
            //if r key is pressed restart scene
            if (restartPressed && (isGameOver || isPaused))
            {
                Restart();

            }
            // m for menu
            if (menuPressed && (isGameOver || isPaused))
            {
                SceneManager.LoadScene(0); // main menu
            }
            // p / escape for pause
            if (pausePressed)
            {
                if (isPaused)
                {
                    // resume game
                    pausePanel.SetActive(false);
                    playerControl = true;
                    Time.timeScale = 1;
                    isPaused = false;
                }
                else
                {
                    // pause the game
                    isPaused = true;
                    playerControl = false;
                    pausePanel.SetActive(true);
                    Time.timeScale = 0;
                }

            }
            if (chooseWeaponPanel.activeSelf)
            {
                if (weapon1Pressed)
                {
                    _player.GetComponent<PlayerCombat>().ChooseWeapon(0);
                    Resume();
                }
                else if (weapon2Pressed)
                {
                    _player.GetComponent<PlayerCombat>().ChooseWeapon(1);
                    Resume();
                }
                else if (weapon3Pressed)
                {
                    _player.GetComponent<PlayerCombat>().ChooseWeapon(2);
                    Resume();
                }
            }
        }
        //-------------------------------------------------------------------------------------------//
        private void ChangeState(GameState newState)
        {
            OnBeforeStateChanged?.Invoke(newState);

            switch (newState)
            {
                case GameState.Starting:
                    HandleStarting();
                    break;
                case GameState.SpawningHeroes:
                    HandleSpawningHeroes();
                    break;
                case GameState.SpawningEnemies:
                    HandleSpawningEnemies();
                    break;
                case GameState.HeroTurn:
                    HandleHeroTurn();
                    break;
                case GameState.EnemyTurn:
                    break;
                case GameState.Win:
                    break;
                case GameState.Lose:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }

            OnAfterStateChanged?.Invoke(newState);

            Debug.Log($"New state: {newState}");
        }
        //-------------------------------------------------------------------------------------------//
        private void HandleStarting()
        {
            levelComplete = false;
            _player = GameObject.Find("Player").GetComponent<Player>();
            if (_player == null)
            {
                Debug.LogError("Player is null");
            }
            // Do some start setup, could be environment, cinematic etc

            // reset stars GFX
            foreach (GameObject obj in stars)
            {
                obj.SetActive(false);
            }

            if (mustChooseWeapon)
            {
                playerControl = false;
                Time.timeScale = 0;
            }
            else
            {
                playerControl = true;
                Time.timeScale = 1;
            }

            pausePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            levelCompletePanel.SetActive(false);
            chooseWeaponPanel.SetActive(mustChooseWeapon);
            _player = GameObject.Find("Player").GetComponent<Player>();
            // Eventually call ChangeState again with your next state

            ChangeState(GameState.SpawningHeroes);
        }
        //-------------------------------------------------------------------------------------------//
        private void HandleSpawningHeroes()
        {
            // UnitManager.Instance.SpawnEnemies();

            ChangeState(GameState.SpawningEnemies);
        }
        private void HandleSpawningEnemies()
        {

            // Spawn enemies

            ChangeState(GameState.HeroTurn);
        }
        //-------------------------------------------------------------------------------------------//
        private void HandleHeroTurn()
        {
            // If you're making a turn based game, this could show the turn menu, highlight available units etc

            // Keep track of how many units need to make a move, once they've all finished, change the state. This could
            // be monitored in the unit manager or the units themselves.
        }
        //-------------------------------------------------------------------------------------------//
        public void GameOver()
        {
            Time.timeScale = 0;
            isGameOver = true;
            gameOverPanel.SetActive(true);
        }
        //-------------------------------------------------------------------------------------------//
        public void LevelComplete()
        {
            AudioManager.instance.Play("Level Complete");
            HideEButton();

            starsCount = 1;
            if (_player.coins >= 100) starsCount += 1;
            if (!_player.tookDamage) starsCount += 1;

            for (int i = 0; i < starsCount; i++)
                stars[i].SetActive(true);

            Time.timeScale = 0;
            playerControl = false;
            isGameOver = true;
            levelComplete = true;
            levelCompletePanel.SetActive(true);

            UnlockNextLevel();
        }

        void UnlockNextLevel()
        {
            int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
            int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

            // Only unlock if this level matches the highest unlocked level
            if (currentBuildIndex == unlocked)
            {
                PlayerPrefs.SetInt("UnlockedLevel", unlocked + 1);
                PlayerPrefs.Save();
            }
        }

        //-------------------------------------------------------------------------------------------//
        public void Resume()
        {
            Invoke(nameof(EnablePlayerControl), 0.1f);
            chooseWeaponPanel.SetActive(false);
            pausePanel.SetActive(false);
            isPaused = false;
            Time.timeScale = 1;
        }
        public void EnablePlayerControl()
        {
            playerControl = true;
        }
        public void DisablePlayerControl()
        {
            playerControl = false;
        }
        //-------------------------------------------------------------------------------------------//
        public void LoadLevel(string level)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(level);
        }

        public void LoadNextLevel()
        {
            Time.timeScale = 1;
            int currentLevel = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentLevel + 1);
        }
        //-------------------------------------------------------------------------------------------//
        public void Restart()
        {
            Time.timeScale = 1;
            levelComplete = false;
            string currentScene = SceneManager.GetActiveScene().name;
            LoadLevel(currentScene);
        }
        //-------------------------------------------------------------------------------------------//
        public void LoadMenu()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(0);
        }
        //-------------------------------------------------------------------------------------------//
        public void ShowEButton()
        {
            eButton.SetActive(true);
        }
        public void HideEButton()
        {
            eButton.SetActive(false);
        }

        //-------------------------------------------------------------------------------------------//

        public void ExitGame()
        {
            Application.Quit();
        }
        //-------------------------------------------------------------------------------------------//    
    }
}