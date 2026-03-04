using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Vidas")]
    public int maxLives = 3;
    private int currentLives;

    [Header("Escenas")]
    public string GameOverScene = "GameOver";
    public string mainMenuScene = "MainMenu";
    public string gameScene = "Game";

    [Header("Respawn")]
    public Transform respawnPoint;
    public float respawnDelay = 2f;

    private GameObject player;

    // -------------------------------------------------------
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // persiste entre escenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentLives = maxLives;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        UIManager.Instance?.UpdateLives(currentLives);
    }

    // -------------------------------------------------------
    // Llamado desde PlayerMovement cuando el jugador muere
    // -------------------------------------------------------
    public void PlayerDied()
    {
        currentLives--;
        UIManager.Instance?.UpdateLives(currentLives);

        if (currentLives <= 0)
        {
            Instance = null;
            SceneManager.LoadScene(GameOverScene);
        }
        else
        {
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        if (player != null) player.SetActive(false);

        yield return new WaitForSeconds(respawnDelay);

        if (player != null && respawnPoint != null)
        {
            player.transform.position = respawnPoint.position;
            player.SetActive(true);
            player.GetComponent<PlayerMovement>()?.ResetHealth();
        }
    }

    // -------------------------------------------------------
    // Botones del Game Over
    // -------------------------------------------------------
    public void RestartGame()
    {
        currentLives = maxLives;
        SceneManager.LoadScene(gameScene);
    }

    public void GoToMainMenu()
    {
        Destroy(gameObject);
        SceneManager.LoadScene(mainMenuScene);
    }

    public int GetLives() => currentLives;
}