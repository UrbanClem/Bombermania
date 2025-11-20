using UnityEngine;
using TMPro;

public class LivesDisplay : MonoBehaviour
{
    public TextMeshProUGUI livesText;
    public string displayFormat = "Vidas: {0}";

    private void Update()
    {
        if (GameManager.Instance != null && livesText != null)
        {
            int currentLives = GameManager.Instance.GetCurrentLives();
            livesText.text = string.Format(displayFormat, currentLives);
        }
    }
}