using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// -------------------------------------------------------
// PANTALLA DE GAME OVER
// -------------------------------------------------------
public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private CanvasGroup canvasGroup;

    void Start()
    {
        retryButton?.onClick.AddListener(() => GameManager.Instance?.RestartGame());
        mainMenuButton?.onClick.AddListener(() => GameManager.Instance?.GoToMainMenu());
        StartCoroutine(FadeIn());
    }

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


