using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverScreen : MonoBehaviour
{
    [Header("Botones")]
    public Button retryButton;
    public Button mainMenuButton;

    void Start()
    {
        retryButton.onClick.AddListener(OnRetry);
        mainMenuButton.onClick.AddListener(OnMainMenu);
        StartCoroutine(FadeIn());
    }

    void OnRetry()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
        else
            SceneManager.LoadScene("Game");
    }

    void OnMainMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
        else
            SceneManager.LoadScene("MainMenu");
    }

    IEnumerator FadeIn()
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) yield break;

        cg.alpha = 0f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            cg.alpha = t;
            yield return null;
        }
    }
}