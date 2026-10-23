using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [SerializeField] private GameObject gameOverContent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ★GameOverUI ごと残し続ける
        DontDestroyOnLoad(gameObject);

        if (gameOverContent != null)
            gameOverContent.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOverContent == null)
        {
            Debug.LogError("gameOverContent が null です！");
            return;
        }

        Time.timeScale = 0f;
        gameOverContent.SetActive(true);
    }

    public void OnRetryButton()
    {
        Time.timeScale = 1f;

        // ★リスタート前にUIを非表示に
        if (gameOverContent != null)
            gameOverContent.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}