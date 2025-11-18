using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("[MainMenu] PlayGame");
        SceneManager.LoadScene("GameScene");
    }

    public void OpenSettings()
    {
        Debug.Log("[MainMenu] OpenSettings");
        SceneManager.LoadScene("SettingsMenu");
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] QuitGame");
        Application.Quit();
    }
}
