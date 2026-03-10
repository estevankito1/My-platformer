using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // -------------------------------------------------------
    // SALUD DEL PLAYER
    // -------------------------------------------------------
    [Header("Salud del Player")]
    [SerializeField] private Slider playerHealthBar;
    [SerializeField] private float hideDelay = 2.5f;
    private Coroutine hideCoroutine;

    // -------------------------------------------------------
    // SALUD DEL ENEMY (enemigo seleccionado/más cercano)
    // -------------------------------------------------------
    [Header("Salud del Enemy")]
    [SerializeField] private Slider enemyHealthBar;
    [SerializeField] private TextMeshProUGUI enemyHealthText; // opcional
    [SerializeField] private GameObject enemyHealthPanel;     // panel contenedor
    [SerializeField] private float enemyHideDelay = 3f;
    private Coroutine enemyHideCoroutine;

    // -------------------------------------------------------
    // VIDAS
    // -------------------------------------------------------
    [Header("Vidas")]
    [SerializeField] private TextMeshProUGUI livesText;

    // -------------------------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Ocultar barras al inicio
        if (playerHealthBar != null)
            playerHealthBar.gameObject.SetActive(false);

        if (enemyHealthPanel != null)
            enemyHealthPanel.SetActive(false);
        else if (enemyHealthBar != null)
            enemyHealthBar.gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    // BARRA DE SALUD DEL PLAYER
    // -------------------------------------------------------
    public void UpdateHealthBar(float current, float max)
    {
        if (playerHealthBar == null) return;

        playerHealthBar.value = current / max;
        playerHealthBar.gameObject.SetActive(true);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HidePlayerHealthBar());
    }

    IEnumerator HidePlayerHealthBar()
    {
        yield return new WaitForSeconds(hideDelay);
        if (playerHealthBar != null)
            playerHealthBar.gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    // BARRA DE SALUD DEL ENEMY
    // -------------------------------------------------------
    public void UpdateEnemyHealthBar(float current, float max)
    {
        if (enemyHealthBar == null) return;

        enemyHealthBar.value = current / max;

        // Mostrar panel o barra
        if (enemyHealthPanel != null)
            enemyHealthPanel.SetActive(true);
        else
            enemyHealthBar.gameObject.SetActive(true);

        // Texto opcional: "50 / 100"
        if (enemyHealthText != null)
            enemyHealthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

        // Reiniciar temporizador de ocultamiento
        if (enemyHideCoroutine != null) StopCoroutine(enemyHideCoroutine);
        enemyHideCoroutine = StartCoroutine(HideEnemyHealthBar());
    }

    // Ocultar barra del enemy cuando muere
    public void HideEnemyHealthBarNow()
    {
        if (enemyHideCoroutine != null) StopCoroutine(enemyHideCoroutine);

        if (enemyHealthPanel != null)
            enemyHealthPanel.SetActive(false);
        else if (enemyHealthBar != null)
            enemyHealthBar.gameObject.SetActive(false);
    }

    IEnumerator HideEnemyHealthBar()
    {
        yield return new WaitForSeconds(enemyHideDelay);

        if (enemyHealthPanel != null)
            enemyHealthPanel.SetActive(false);
        else if (enemyHealthBar != null)
            enemyHealthBar.gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    // VIDAS
    // -------------------------------------------------------
    public void UpdateLives(int current, int max)
    {
        if (livesText != null)
            livesText.text = $"Vidas: {current}";
    }
}