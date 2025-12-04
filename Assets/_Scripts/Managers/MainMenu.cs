using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.Managers
{
    public class MainMenu : MonoBehaviour
    {
        void Awake()
        {
            if (!PlayerPrefs.HasKey("UnlockedLevel"))
                PlayerPrefs.SetInt("UnlockedLevel", 1);

            PlayerPrefs.Save();
        }

        public bool isMuted;
        // public Text volumeText;

        public void LoadLevel(int level)
        {
            SceneManager.LoadScene(level);
        }
        public void LoadMenu()
        {
            SceneManager.LoadScene(0);
        }
        // public void LoadSettings()
        // {
        //     SceneManager.LoadScene(); 
        // }
        // public void LoadCredits()
        // {
        //     SceneManager.LoadScene();
        // }
        public void MuteAllSound()
        {
            AudioListener.volume = 0;

        }
        public void UnMuteAllSound()
        {
            AudioListener.volume = 1;

        }
        public void ChangeVolume()
        {
            // if (isMuted)
            // {
            //     volumeText.text = "SOUND ON";
            //     UnMuteAllSound();
            //     PlayerPrefs.SetString("Sound", "SOUND ON"); 
            //     PlayerPrefs.SetInt("Muted", 0);
            //     isMuted = false;

            // } else {
            //     volumeText.text = "SOUND OFF";
            //     MuteAllSound();
            //     PlayerPrefs.SetString("Sound", "SOUND OFF"); 
            //     PlayerPrefs.SetInt("Muted", 1);
            //     isMuted = true;
            // }
        }
    }
}
