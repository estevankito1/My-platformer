using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverScreen : MonoBehaviour
{
    [Header("Pantalla Principal Game Over")]
    [SerializeField] private GameObject gameOverPanel;   // imagen de Game Over + botón continuar
    [SerializeField] private Button continueButton;      // "¿Quieres continuar?"

    [Header("Pantalla de Confirmación")]
    [SerializeField] private GameObject confirmPanel;    // "¿Estás seguro?"
    [SerializeField] private Button yesButton;           // Sí → reiniciar
    [SerializeField] private Button noButton;            // No → main menu

    [Header("Fade")]
    [SerializeField] private CanvasGroup canvasGroup;


    // -------------------------------------------------------
    void Start()
    {
        // Asignar botones
        continueButton?.onClick.AddListener(ShowConfirm);
        yesButton?.onClick.AddListener(RetryGame);
        noButton?.onClick.AddListener(GoToMainMenu);

        // Mostrar solo el panel principal al inicio
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (confirmPanel != null) confirmPanel.SetActive(false);

        StartCoroutine(FadeIn());
    }

    // -------------------------------------------------------
    // Mostrar panel de confirmación
    public void ShowConfirm()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(true);
    }

    public void RetryGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
        else
            SceneManager.LoadScene(0);
    }

    public void GoToMainMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
        else
            SceneManager.LoadScene(2);
    }

    // -------------------------------------------------------
    IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        canvasGroup.alpha = 0f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            canvasGroup.alpha = Mathf.Clamp01(t);
            yield return null;
        }
    }
}