using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class VictoryMenu : MonoBehaviour
{
    [Header("Botones de Texto")]
    public TextMeshProUGUI quitButtonText;

    [Header("Configuración de Botones")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color selectedColor = new Color(1f, 0.5f, 0f);
    public Color pressedColor = Color.gray;

    [Header("Navegación")]
    public float navigationCooldown = 0.2f;

    [Header("Texto de Victoria")]
    public TextMeshProUGUI victoryText;
    public string victoryMessage = "VICTORIA";

    [Header("Configuración adicional")]
    public string mainMenuScene = "MainMenu";

    private TextMeshProUGUI[] buttons;
    private int currentSelection = 0;
    private float lastNavigationTime = 0f;

    // Referencias a dispositivos de entrada
    private Keyboard keyboard;
    private Gamepad gamepad;

    private void Start()
    {
        // Inicializar array de botones (solo uno)
        buttons = new TextMeshProUGUI[] { quitButtonText };

        // Configurar texto de Victoria
        if (victoryText != null)
        {
            victoryText.text = victoryMessage;
        }

        // Configurar el botón
        for (int i = 0; i < buttons.Length; i++)
        {
            SetupButton(buttons[i], i);
        }

        // Seleccionar el botón por defecto
        SelectButton(0);
        SetupEventSystem();

        // Obtener referencias a dispositivos de entrada
        keyboard = Keyboard.current;
        gamepad = Gamepad.current;

        Debug.Log("[VictoryMenu] Menú de Victoria iniciado");
    }

    private void Update()
    {
        HandleSelectionInput();
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
            if (gamepad.aButton.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame ||
                gamepad.bButton.wasPressedThisFrame || gamepad.xButton.wasPressedThisFrame ||
                gamepad.yButton.wasPressedThisFrame)
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
            buttonText.text = "QUIT";

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
            button.onClick.AddListener(QuitToMainMenu);
        }
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
        QuitToMainMenu();
    }

    public void QuitToMainMenu()
    {
        Debug.Log("[VictoryMenu] Volviendo al menú principal...");
        StartCoroutine(PlayButtonEffect(quitButtonText));
    }

    private IEnumerator PlayButtonEffect(TextMeshProUGUI buttonText)
    {
        if (buttonText != null)
        {
            buttonText.color = pressedColor;
            yield return new WaitForSecondsRealtime(0.1f);
            buttonText.color = selectedColor;
        }

        // Pequeña pausa antes de cambiar de escena
        yield return new WaitForSecondsRealtime(0.2f);

        // Cargar el menú principal
        SceneManager.LoadScene(mainMenuScene);
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

    // Método para activar/desactivar el menú de Victoria
    public void ShowVictoryMenu(bool show)
    {
        gameObject.SetActive(show);
        
        if (show)
        {
            // Seleccionar el botón cuando se muestra el menú
            SelectButton(0);
            Debug.Log("[VictoryMenu] Menú de Victoria activado");
        }
    }

    // Método para cambiar el mensaje de victoria
    public void SetVictoryMessage(string message)
    {
        victoryMessage = message;
        if (victoryText != null)
        {
            victoryText.text = victoryMessage;
        }
    }
}