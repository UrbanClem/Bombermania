using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("Botones de Texto")]
    public TextMeshProUGUI playButtonText;
    public TextMeshProUGUI settingsButtonText;
    public TextMeshProUGUI quitButtonText;

    [Header("Configuración de Botones")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color selectedColor = new Color(1f, 0.5f, 0f);
    public Color pressedColor = Color.gray;

    [Header("Navegación")]
    public float navigationCooldown = 0.2f;

    private TextMeshProUGUI[] buttons;
    private int currentSelection = 0;
    private float lastNavigationTime = 0f;

    // Referencias a dispositivos de entrada
    private Keyboard keyboard;
    private Gamepad gamepad;

    private void Start()
    {
        // Inicializar array de botones
        buttons = new TextMeshProUGUI[] { playButtonText, settingsButtonText, quitButtonText };

        // Configurar cada botón
        for (int i = 0; i < buttons.Length; i++)
        {
            SetupButton(buttons[i], i);
        }

        // Seleccionar el primer botón por defecto
        SelectButton(0);
        SetupEventSystem();

        // Obtener referencias a dispositivos de entrada
        keyboard = Keyboard.current;
        gamepad = Gamepad.current;
    }

    private void Update()
    {
        HandleNavigationInput();
        HandleSelectionInput();
    }

    private void HandleNavigationInput()
    {
        if (Time.unscaledTime - lastNavigationTime < navigationCooldown)
            return;

        bool navigated = false;

        // Navegación con teclado
        if (keyboard != null)
        {
            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                Navigate(-1);
                navigated = true;
            }
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                Navigate(1);
                navigated = true;
            }
        }

        // Navegación con gamepad
        if (gamepad != null && !navigated)
        {
            Vector2 stickInput = gamepad.leftStick.ReadValue();
            Vector2 dPadInput = gamepad.dpad.ReadValue();

            // Usar D-pad o stick izquierdo
            if (dPadInput.y > 0.5f || stickInput.y > 0.5f)
            {
                Navigate(-1);
                navigated = true;
            }
            else if (dPadInput.y < -0.5f || stickInput.y < -0.5f)
            {
                Navigate(1);
                navigated = true;
            }
        }

        if (navigated)
        {
            lastNavigationTime = Time.unscaledTime;
        }
    }

    private void HandleSelectionInput()
    {
        bool selected = false;

        // Selección con teclado
        if (keyboard != null)
        {
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                ExecuteCurrentButton();
                selected = true;
            }
        }

        // Selección con gamepad
        if (gamepad != null && !selected)
        {
            if (gamepad.aButton.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame)
            {
                ExecuteCurrentButton();
                selected = true;
            }
        }
    }

    private void SetupButton(TextMeshProUGUI buttonText, int index)
    {
        if (buttonText != null)
        {
            switch (index)
            {
                case 0: buttonText.text = "START"; break;
                case 1: buttonText.text = "CONFIGURACIÓN"; break;
                case 2: buttonText.text = "QUIT"; break;
            }

            Button button = buttonText.GetComponent<Button>();
            if (button == null)
            {
                button = buttonText.gameObject.AddComponent<Button>();
            }

            ColorBlock colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = pressedColor;
            colors.selectedColor = selectedColor;
            button.colors = colors;

            button.onClick.RemoveAllListeners();
            switch (index)
            {
                case 0: button.onClick.AddListener(PlayGame); break;
                case 1: button.onClick.AddListener(OpenSettings); break;
                case 2: button.onClick.AddListener(QuitGame); break;
            }
        }
    }

    private void Navigate(int direction)
    {
        int newSelection = currentSelection + direction;
        if (newSelection < 0)
            newSelection = buttons.Length - 1;
        else if (newSelection >= buttons.Length)
            newSelection = 0;

        SelectButton(newSelection);
    }

    private void SelectButton(int index)
    {
        if (currentSelection >= 0 && currentSelection < buttons.Length)
        {
            if (buttons[currentSelection] != null)
            {
                buttons[currentSelection].color = normalColor;
            }
        }

        currentSelection = index;
        if (buttons[currentSelection] != null)
        {
            buttons[currentSelection].color = selectedColor;
            
            // Actualizar selección en EventSystem para navegación UI
            EventSystem.current.SetSelectedGameObject(buttons[currentSelection].gameObject);
        }
    }

    private void ExecuteCurrentButton()
    {
        switch (currentSelection)
        {
            case 0: PlayGame(); break;
            case 1: OpenSettings(); break;
            case 2: QuitGame(); break;
        }
    }

    public void PlayGame()
    {
        Debug.Log("[MainMenu] Cargando Stage1...");
        StartCoroutine(PlayButtonEffect(playButtonText, "Stage1"));
    }

    public void OpenSettings()
    {
        Debug.Log("[MainMenu] Abriendo configuración...");
        StartCoroutine(PlayButtonEffect(settingsButtonText, "SettingsMenu"));
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Saliendo del juego...");
        StartCoroutine(PlayButtonEffect(quitButtonText, "quit"));
    }

    private IEnumerator PlayButtonEffect(TextMeshProUGUI buttonText, string sceneName)
    {
        if (buttonText != null)
        {
            buttonText.color = pressedColor;
            yield return new WaitForSecondsRealtime(0.1f);
            buttonText.color = selectedColor;
        }

        if (sceneName == "quit")
        {
            Application.Quit();
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void SetupEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }

        if (buttons.Length > 0 && buttons[0] != null)
        {
            EventSystem.current.firstSelectedGameObject = buttons[0].gameObject;
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }
    }

    public void OnButtonEnter(TextMeshProUGUI buttonText)
    {
        if (buttonText != null)
        {
            buttonText.color = hoverColor;
            
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == buttonText)
                {
                    currentSelection = i;
                    SelectButton(i);
                    break;
                }
            }
        }
    }

    public void OnButtonExit(TextMeshProUGUI buttonText)
    {
        if (buttonText != null)
        {
            if (buttons[currentSelection] != buttonText)
            {
                buttonText.color = normalColor;
            }
        }
    }
}