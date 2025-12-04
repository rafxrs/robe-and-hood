using _Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        GameObject _containers;
        GameManager _gameManager;
        public Text coinText;
        public Text gameOverCoinText;
        public Text levelCompleteCoinText;
        public int coins;
        // Start is called before the first frame update
        void Start()
        {        
            _containers = GameObject.Find("Containers");
            _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
            // if (gameManager==null)
            // {
            //     Debug.Log("game manager is null");
            // }
            // coins = PlayerPrefs.GetInt("coins", 0);
            coinText.text = "0";
        }

        public void UpdateCoin(int coin)
        {
            coins = coin;
            coinText.text = coin.ToString();
            levelCompleteCoinText.text = coin.ToString();
            gameOverCoinText.text = coin.ToString();
        }
        public void LevelComplete()
        {
            _gameManager.LevelComplete();
        }
        public void GameOverSequence()
        {
            _gameManager.GameOver();
            // StartCoroutine(GameOverRoutine());
            Destroy(_containers);

        }
    }
}
