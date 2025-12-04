using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.Managers.Scene_Management
{
    public class SceneSwitch : MonoBehaviour
    {
        private static string _currentScene;
        protected static string previousScene;
        // Start is called before the first frame update
        public virtual void Start()
        {
            _currentScene = SceneManager.GetActiveScene().name;
        }

        // Method to load new scene
        public void SwitchScene(string sceneName)
        {
            previousScene = _currentScene;
            SceneManager.LoadScene(sceneName);
        } 
    }
}
