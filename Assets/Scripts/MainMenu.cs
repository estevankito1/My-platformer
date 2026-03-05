using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private string gameScene = "Game";

    void Start()
    {
        playButton?.onClick.AddListener(PlayGame);
        quitButton?.onClick.AddListener(QuitGame);
    }

    void PlayGame() => SceneManager.LoadScene(gameScene);
    void QuitGame() => Application.Quit();
}






