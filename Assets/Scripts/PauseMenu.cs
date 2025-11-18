using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // si usas el Input System nuevo

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;      // arrástrale el Panel
    public InputActionReference pauseAction; // crea una acción "Pause" (Esc o Start) y arrástrala aquí

    bool paused;

    void OnEnable()
    {
        // Estado inicial
        paused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI) pauseMenuUI.SetActive(false);

        // Input System (si lo usas)
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += _ => TogglePause();
        }
    }

    void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= _ => TogglePause();
            pauseAction.action.Disable();
        }
    }

    public void TogglePause()
    {
        if (paused) Resume();
        else Pause();
    }

    public void Resume()
    {
        paused = false;
        if (pauseMenuUI) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Pause()
    {
        paused = true;
        if (pauseMenuUI) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
