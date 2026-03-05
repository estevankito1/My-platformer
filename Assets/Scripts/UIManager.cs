using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Salud")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private float hideDelay = 2.5f;
    private Coroutine hideCoroutine;

    [Header("Vidas")]
    [SerializeField] private TextMeshProUGUI livesText;

    // -------------------------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Ocultar barra al inicio
        if (healthBar != null)
            healthBar.gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    // BARRA DE SALUD
    // -------------------------------------------------------
    public void UpdateHealthBar(float current, float max)
    {
        if (healthBar == null) return;

        healthBar.value = current / max;
        healthBar.gameObject.SetActive(true);

        // Reiniciar temporizador de ocultamiento
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideHealthBar());
    }

    IEnumerator HideHealthBar()
    {
        yield return new WaitForSeconds(hideDelay);
        if (healthBar != null)
            healthBar.gameObject.SetActive(false);
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