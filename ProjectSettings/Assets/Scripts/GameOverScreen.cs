using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverScreen : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button continueButton;   // Reiniciar juego
    [SerializeField] private Button mainMenuButton;   // Ir al menú principal

    [Header("Fade")]
    [SerializeField] private CanvasGroup canvasGroup;

    // -------------------------------------------------------
    void Start()
    {
        continueButton?.onClick.AddListener(RetryGame);
        mainMenuButton?.onClick.AddListener(GoToMainMenu);
        StartCoroutine(FadeIn());
    }

    // -------------------------------------------------------
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