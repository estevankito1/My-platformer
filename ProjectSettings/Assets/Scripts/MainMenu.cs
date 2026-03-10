using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button closeCreditsButton;

    [Header("Créditos")]
    [SerializeField] private GameObject creditsPanel;  // imagen de créditos

    // -------------------------------------------------------
    void Start()
    {
        playButton?.onClick.AddListener(PlayGame);
        creditsButton?.onClick.AddListener(ShowCredits);
        closeCreditsButton?.onClick.AddListener(HideCredits);

        // Ocultar créditos al inicio
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void PlayGame() => SceneManager.LoadScene(0);
    public void ShowCredits() => creditsPanel?.SetActive(true);
    public void HideCredits() => creditsPanel?.SetActive(false);

    public void QuitGame() => Application.Quit();
}













