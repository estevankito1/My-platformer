using UnityEngine;
using TMPro;

public class WinItem : MonoBehaviour
{
    // -------------------------------------------------------
    // CONFIGURACIÓN
    // -------------------------------------------------------
    [Header("Recolección")]
    [SerializeField] private float interactRange = 1.5f;  // distancia para recoger
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Visual")]
    [SerializeField] private GameObject promptUI; // texto "Presiona E" encima del item
    [SerializeField] private GameObject collectEffect; // partículas opcionales

    private Transform player;
    private bool playerNearby = false;
    private bool collected = false;

    // -------------------------------------------------------
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Ocultar prompt al inicio
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    // -------------------------------------------------------
    void Update()
    {
        if (collected || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        playerNearby = dist <= interactRange;

        // Mostrar/ocultar prompt
        if (promptUI != null)
            promptUI.SetActive(playerNearby);

        // Recoger al presionar E
        if (playerNearby && Input.GetKeyDown(interactKey))
            Collect();
    }

    // -------------------------------------------------------
    void Collect()
    {
        collected = true;

        // Efecto visual opcional
        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        // Avisar al WinManager
        WinManager.Instance?.ItemCollected();

        // Destruir el item
        Destroy(gameObject);
    }

    // -------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}