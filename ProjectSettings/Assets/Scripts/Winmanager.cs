using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class WinManager : MonoBehaviour
{
    public static WinManager Instance;

    // -------------------------------------------------------
    // ITEMS
    // -------------------------------------------------------
    [Header("Items")]
    private int totalItems;
    private int collectedItems;

    // -------------------------------------------------------
    // UI
    // -------------------------------------------------------
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI itemCounterText; // muestra "2 / 5"
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Button mainMenuButton;

    // -------------------------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Contar todos los items en la escena automáticamente
        totalItems = FindObjectsByType<WinItem>(FindObjectsSortMode.None).Length;
        collectedItems = 0;

        if (winPanel != null)
            winPanel.SetActive(false);

        mainMenuButton?.onClick.AddListener(GoToMainMenu);

        UpdateUI();
    }

    // -------------------------------------------------------
    // LLAMADO POR CADA ITEM AL RECOGERLO
    // -------------------------------------------------------
    public void ItemCollected()
    {
        collectedItems++;
        UpdateUI();

        if (collectedItems >= totalItems)
            StartCoroutine(ShowWinScreen());
    }

    // -------------------------------------------------------
    void UpdateUI()
    {
        if (itemCounterText != null)
            itemCounterText.text = $"{collectedItems} / {totalItems}";
    }

    IEnumerator ShowWinScreen()
    {
        yield return new WaitForSeconds(0.5f);

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    // -------------------------------------------------------
    public void GoToMainMenu() => SceneManager.LoadScene(2);
    public void StopTimer() { } // mantener compatibilidad con GameManager
}