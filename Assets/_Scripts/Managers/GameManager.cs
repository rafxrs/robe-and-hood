using System;
using _Scripts.Units.Player;
using UnityEngine;
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
        /// <summary>
        /// This is obviously an example and I have no idea what kind of game you're making.
        /// You can use a similar manager for controlling your menu states or dynamic-cinematic, etc
        /// </summary>
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
            // if level complete and press space, go to next level
            if (Input.GetKeyDown(KeyCode.Space) && levelComplete)
            {
                LoadNextLevel();
            }
            //if r key is pressed restart scene
            if (Input.GetKeyDown(KeyCode.R) && (isGameOver || isPaused))
            {
                Restart();

            }
            // m for menu
            if (Input.GetKeyDown(KeyCode.M) && (isGameOver || isPaused))
            {
                SceneManager.LoadScene(0); // main menu
            }
            // p for pause
            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
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
            else if (isPaused && Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
                // SceneManager.LoadScene(0  );
            }
            if (chooseWeaponPanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    _player.GetComponent<PlayerCombat>().ChooseWeapon(0);
                    Resume();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    _player.GetComponent<PlayerCombat>().ChooseWeapon(1);
                    Resume();
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
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
