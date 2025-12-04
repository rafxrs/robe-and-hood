using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelButtonController : MonoBehaviour
{
    [System.Serializable]
    public class LevelUI
    {
        public Button button;
        public GameObject lockIcon;
        public int sceneIndex;
    }

    public LevelUI[] levelButtons;

    void Start()
    {
        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

        foreach (var lvl in levelButtons)
        {
            // Always clear old listeners first
            lvl.button.onClick.RemoveAllListeners();

            bool isUnlocked = lvl.sceneIndex <= unlocked;

            lvl.button.interactable = isUnlocked;
            lvl.lockIcon.SetActive(!isUnlocked);

            if (isUnlocked)
            {
                int index = lvl.sceneIndex; // copy closure
                lvl.button.onClick.AddListener(() => LoadLevel(index));
            }
        }
    }

    void LoadLevel(int index)
    {
        SceneManager.LoadScene(index);
    }
}
