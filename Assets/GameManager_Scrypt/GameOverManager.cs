using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("UI")]
    public GameObject gameOverUI;

    private void Awake()
    {
        Instance = this;
    }

    // PlayerData から呼び出す
    public void ShowGameOver()
    {
        // ゲームを一時停止
        Time.timeScale = 0f;

        // UIを表示
        gameOverUI.SetActive(true);
    }

    // RetryButton の OnClick に設定する
    public void OnRetryButton()
    {
        // 時間を戻す
        Time.timeScale = 1f;

        // 現在のシーンをリスタート
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}