using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Vidas")]
    [SerializeField] private int maxLives = 3;
    private int currentLives;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 1.5f;


    private GameObject player;

    // -------------------------------------------------------
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentLives = maxLives;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        UIManager.Instance?.UpdateLives(currentLives, maxLives);
    }

    // -------------------------------------------------------
    // PLAYER MURIÓ
    // -------------------------------------------------------
    public void PlayerDied()
    {
        currentLives--;
        UIManager.Instance?.UpdateLives(currentLives, maxLives);

        // Detener el timer de victoria
        WinManager.Instance?.StopTimer();

        if (currentLives <= 0)
            StartCoroutine(LoadGameOver());
        else
            StartCoroutine(RespawnPlayer());
    }

    IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (player != null && respawnPoint != null)
        {
            player.transform.position = respawnPoint.position;
            player.GetComponent<PlayerMovement>()?.ResetHealth();
            player.SetActive(true);
        }
    }

    IEnumerator LoadGameOver()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(1);
    }

    // -------------------------------------------------------
    // PÚBLICOS
    // -------------------------------------------------------
    public void RestartGame()
    {
        currentLives = maxLives;
        SceneManager.LoadScene(0);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(2);
    }

    public int GetLives() => currentLives;
}