using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    [Header("HP")]
    public int maxHP = 100;
    public int currentHP;

    [Header("HPバー")]
    public Image hpBarFill;         // Fill の Image
    public Image hpBarDelay;        // 遅延バー（不要なら省略OK）
    public float delaySpeed = 2f;   // 遅延バーの追従速度

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentHP = maxHP;

        // 起動時にHPバーを初期化
        UpdateHPBar();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentHP = maxHP;

        Time.timeScale = 1f;
        // シーン内の HPBarFill タグのオブジェクトを自動で探す
        GameObject fillObj = GameObject.FindWithTag("HPBarFill");
        if (fillObj != null)
            hpBarFill = fillObj.GetComponent<Image>();

        UpdateHPBar();

        currentHP = maxHP;

        if (fillObj != null)
            hpBarFill = fillObj.GetComponent<Image>();

        UpdateHPBar();
    }

    // 遅延バーの追従処理
    private void Update()
    {
        if (hpBarDelay == null) return;

        hpBarDelay.fillAmount = Mathf.Lerp(
            hpBarDelay.fillAmount,
            hpBarFill.fillAmount,
            delaySpeed * Time.deltaTime
        );
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        currentHP -= damage;

        if (currentHP < 0)
            currentHP = 0;

        Debug.Log($"ダメージ: {damage}  現在HP: {currentHP}");

        // ダメージ時にHPバーを更新
        UpdateHPBar();

        if (currentHP <= 0)
        {
            Die();

            if (GameOverManager.Instance == null)
            {
                Debug.LogError("GameOverManager が見つかりません！");
                return;
            }

            GameOverManager.Instance.ShowGameOver();
        }
    }

    // 回復処理
    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);

        UpdateHPBar();
    }

    // HPバー更新
    private void UpdateHPBar()
    {
        if (hpBarFill == null) return;

        hpBarFill.fillAmount = (float)currentHP / maxHP;

        // 遅延バーがある場合、初期化時は即座に合わせる
        if (hpBarDelay != null && currentHP == maxHP)
            hpBarDelay.fillAmount = 1f;
    }

    private void Die()
    {
        Debug.Log("プレイヤー死亡");

       
    

        // Instance が null でも直接探しに行く
        GameOverManager manager =
            FindFirstObjectByType<GameOverManager>();

        if (manager == null)
        {
            Debug.LogError("GameOverManager がシーンに存在しません！");
            return;
        }

        manager.ShowGameOver();
    

    GameOverManager[] managers =
        FindObjectsByType<GameOverManager>(FindObjectsSortMode.None);

        Debug.Log($"シーン内の GameOverManager の数: {managers.Length}");

        foreach (var m in managers)
        {
            Debug.Log($"見つかった: {m.gameObject.name} " +
                      $"アクティブ: {m.gameObject.activeInHierarchy}");
        }

        if (GameOverManager.Instance == null)
        {
            Debug.LogError("GameOverManager が見つかりません！");
            return;
        }

        GameOverManager.Instance.ShowGameOver();
    }
}