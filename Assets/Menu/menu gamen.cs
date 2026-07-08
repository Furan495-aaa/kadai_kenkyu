using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移に必要
using UnityEngine.UI; // ボタンを扱うために必要
using System.Collections.Generic; // リストを使うために必要

public class PauseManager : MonoBehaviour
{
    // 他のスクリプトから簡単にPauseManagerにアクセスできるようにするための設定（シングルトン）
    public static PauseManager Instance { get; private set; }

    // メニュー画面のPanelオブジェクトをインスペクターから紐付けます
    [SerializeField] private GameObject pauseMenuPanel;
    // メニューを開いた時に最初に選択状態にしたいボタン（再開ボタンなど）
    [SerializeField] private Button firstSelectedButton; 

    // 外部のスクリプトからポーズ中かどうかを読み取るための窓口
    public bool IsPaused { get; private set; } = false;

    // プレイヤーのゲームオブジェクト
    private GameObject playerObj;
    
    // ✨ 追加：ポーズ前に「動いていた」スクリプトだけを記憶するリスト
    private List<MonoBehaviour> scriptsToDisable = new List<MonoBehaviour>();

    // ゲーム開始時（最初）に呼ばれる処理
    void Awake()
    {
        // シングルトンの初期化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false); // ゲーム開始時は絶対にメニューを非表示にする
        }
        Time.timeScale = 1f; // 時間も確実に動かしておく

        // シーン開始時に「Player」タグの付いたオブジェクトを取得
        playerObj = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        // 1. ポーズ中のフォーカス自動復旧（製品版での誤操作対策）
        if (IsPaused)
        {
            // マウスの誤クリックなどで、ボタンの選択状態が解除されて（空に）なってしまった場合
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null)
            {
                // プレイヤーが上下左右のキー（矢印キーやWASDなど）を入力した瞬間に
                if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
                {
                    // 最初のボタンにフォーカスを強制復帰させる
                    if (firstSelectedButton != null)
                    {
                        firstSelectedButton.Select();
                    }
                }
            }
        }

        // 2. Escキーによるメニューの開閉判定
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // ゲームを再開する処理（Escキー、または再開ボタンから呼ばれる）
    public void ResumeGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false); // メニューを非表示にする
        Time.timeScale = 1f;            // 時間の進み方を通常（1倍速）に戻す
        IsPaused = false;               // フラグを「通常時」に戻す

        // ポーズ解除時に、画面内のすべてのアニメーションの再生速度を「1（通常）」に戻す
        SetAllAnimatorsSpeed(1f);

        // ✨ 追加：停止させていたプレイヤーのスクリプトをすべて再起動する
        EnablePlayerScripts();
    }

    // ゲームを一時停止する処理（Escキーから呼ばれる）
    public void PauseGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);  // メニューを表示する
        Time.timeScale = 0f;             // 時間の進み方を「0」にして完全に止める
        IsPaused = true;                 // フラグを「ポーズ中」にする

        // ポーズした瞬間に、画面内のすべてのアニメーションの再生速度を「0（停止）」にする
        SetAllAnimatorsSpeed(0f);

        // ✨ 追加：プレイヤーに付いている移動・回転スクリプトを強制的にオフにする
        DisablePlayerScripts();

        // メニューを開いた瞬間に、確実にキーボード操作を受け付けるようにする処理
        if (firstSelectedButton != null)
        {
            // 直前までの選択を一度完全にクリアする（これで製品版でもバグらなくなります）
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }

            // 最初のボタンを選択状態にする
            firstSelectedButton.Select();
            
            // 念のため、ボタンをシステムに「現在選択中」として登録
            firstSelectedButton.OnSelect(null); 
        }
    }

    // タイトル画面など、他のシーンへ移動する処理（タイトルに戻るボタン用）
    public void GoToTitle()
    {
        Time.timeScale = 1f; // 重要：時間を動かしてからシーンを換えないと、次のシーンも止まったままになります！
        SceneManager.LoadScene("TitleScene"); // 引数には移動したいシーン名を入れてください
    }

    // シーン内のすべての Animator の速度を一括変更する便利関数
    private void SetAllAnimatorsSpeed(float speed)
    {
        Animator[] allAnimators = FindObjectsOfType<Animator>();
        foreach (Animator anim in allAnimators)
        {
            if (anim != null)
            {
                anim.speed = speed;
            }
        }
    }

    // ✨ 追加：プレイヤーのスクリプトをスキャンして、強制的に機能をオフにする
    private void DisablePlayerScripts()
    {
        if (playerObj == null) return;

        scriptsToDisable.Clear();

        // プレイヤーに付いているカスタムスクリプトをすべて取得
        MonoBehaviour[] customScripts = playerObj.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in customScripts)
        {
            // PauseManager自身や、消してはいけない特定のシステム用コンポーネント以外を対象にする
            if (script != null && script.enabled && script != this)
            {
                script.enabled = false; // スクリプトのチェックボックスを外す（Update等を止める）
                scriptsToDisable.Add(script); // 記憶しておく
            }
        }
    }

    // ✨ 追加：ポーズ解除時に、オフにしていたスクリプトだけを元通りオンにする
    private void EnablePlayerScripts()
    {
        foreach (MonoBehaviour script in scriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = true; // スクリプトを再度オンにする
            }
        }
        scriptsToDisable.Clear();
    }
}