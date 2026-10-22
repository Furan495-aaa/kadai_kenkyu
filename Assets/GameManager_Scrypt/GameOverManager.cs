using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    // ★GameOverUI の中身だけを非表示にする用
    [SerializeField] private GameObject gameOverContent;

    private void Awake()
    {
        Instance = this;

        // 自分自身はアクティブのまま、中身だけ非表示にする
        gameOverContent.SetActive(false);
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        gameOverContent.SetActive(true);
    }

    public void OnRetryButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}