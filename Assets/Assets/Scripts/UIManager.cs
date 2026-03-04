using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Salud")]
    public Slider healthBar;
    public float hideDelay = 2f;      // segundos hasta que se oculta
    private Coroutine hideCoroutine;

    // -------------------------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Oculto al inicio
        if (healthBar != null)
            healthBar.gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    public void UpdateHealthBar(float current, float max)
    {
        if (healthBar == null) return;

        healthBar.value = current / max;

        // Mostrar el slider
        healthBar.gameObject.SetActive(true);

        // Reiniciar el temporizador de ocultamiento
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        if (healthBar != null)
            healthBar.gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    public void UpdateLives(int lives)
    {
        // Sin UI de vidas, el GameManager las sigue contando internamente
    }
}