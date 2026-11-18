using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TitleManager : MonoBehaviour
{
    [Header("シーン・移動設定")]
    [SerializeField] private string firstStageSceneName = "Screen1";
    [SerializeField] private MonoBehaviour playerMoveScript;

    [Header("UIパーツ")]
    [SerializeField] private GameObject pressAnyKeyText;
    [SerializeField] private GameObject menuButtonsGroup;
    [SerializeField] private Button startButton;

    [Header("点滅演出の速度設定")]
    [SerializeField] private float idleFadeSpeed = 2.0f; // 待機中および選択中ボタンの点滅の速さ
    [SerializeField] private float pressAnyKeyFlashInterval = 0.06f; // PRESS ANY BUTTONのキー押下後点滅の間隔

    private TMP_Text pressAnyKeyTMP;
    private TMP_Text[] menuButtonTexts;
    private bool isMenuOpened = false;
    private bool isTransitioning = false;

    private GameObject lastSelectedObj = null;
    private float selectStartTime = 0f;

    void Start()
    {
        Time.timeScale = 1f;

        if (pressAnyKeyText != null)
        {
            pressAnyKeyText.SetActive(true);
            pressAnyKeyTMP = pressAnyKeyText.GetComponent<TMP_Text>();
        }

        if (menuButtonsGroup != null)
        {
            menuButtonTexts = menuButtonsGroup.GetComponentsInChildren<TMP_Text>();
            menuButtonsGroup.SetActive(false);
        }

        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = false;
        }
    }

    void Update()
    {
        // 1. PRESS ANY BUTTON 待機中
        if (!isMenuOpened)
        {
            if (!isTransitioning)
            {
                FadeBlinkText(pressAnyKeyTMP, Time.time);

                if (Input.anyKeyDown)
                {
                    StartCoroutine(PressedBlinkAndOpenMenu());
                }
            }
            return;
        }

        // 2. バックスペースでタイトル待機に戻る
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ReturnToTitleWait();
            return;
        }

        if (isTransitioning) return;

        // 3. メニュー選択中の点滅処理
        UpdateSelectedButtonBlink();

        // フォーカス自動復旧
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
            {
                if (startButton != null)
                {
                    startButton.Select();
                }
            }
        }
    }

    private void FadeBlinkText(TMP_Text tmp, float time)
    {
        if (tmp == null) return;

        float alpha = Mathf.PingPong(time * idleFadeSpeed, 1f);
        Color c = tmp.color;
        c.a = alpha;
        tmp.color = c;
    }

    private void UpdateSelectedButtonBlink()
    {
        if (EventSystem.current == null || menuButtonTexts == null) return;

        GameObject selectedObj = EventSystem.current.currentSelectedGameObject;

        if (selectedObj != lastSelectedObj)
        {
            lastSelectedObj = selectedObj;
            selectStartTime = Time.time;
        }

        float elapsed = Time.time - selectStartTime;

        foreach (var tmp in menuButtonTexts)
        {
            if (tmp == null) continue;

            bool isSelected = (selectedObj != null && (tmp.gameObject == selectedObj || tmp.transform.IsChildOf(selectedObj.transform)));

            Color c = tmp.color;
            if (isSelected)
            {
                c.a = Mathf.PingPong(elapsed * idleFadeSpeed, 1f);
            }
            else
            {
                c.a = 1.0f; 
            }
            tmp.color = c;
        }
    }

    private void ReturnToTitleWait()
    {
        isMenuOpened = false;
        isTransitioning = false;
        lastSelectedObj = null;

        if (menuButtonsGroup != null) menuButtonsGroup.SetActive(false);
        if (pressAnyKeyText != null) pressAnyKeyText.SetActive(true);
    }

    private IEnumerator PressedBlinkAndOpenMenu()
    {
        isTransitioning = true;

        // PRESS ANY BUTTONを押したときの5回点滅だけは維持（必要に応じてここも消せます）
        if (pressAnyKeyTMP != null)
        {
            Color c = pressAnyKeyTMP.color;
            for (int i = 0; i < 5; i++)
            {
                c.a = 0f;
                pressAnyKeyTMP.color = c;
                yield return new WaitForSeconds(pressAnyKeyFlashInterval);

                c.a = 1f;
                pressAnyKeyTMP.color = c;
                yield return new WaitForSeconds(pressAnyKeyFlashInterval);
            }
        }

        OpenMainMenu();
    }

    // ==========================================
    // 決定・クリック時は即座に処理を実行
    // ==========================================

    public void OnClickStartButton()
    {
        if (isTransitioning) return;

        if (playerMoveScript != null) playerMoveScript.enabled = true;
        if (!string.IsNullOrEmpty(firstStageSceneName))
        {
            SceneManager.LoadScene(firstStageSceneName);
        }
    }

    public void OnClickQuitButton()
    {
        if (isTransitioning) return;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OpenMainMenu()
    {
        isMenuOpened = false; 
        isMenuOpened = true;
        isTransitioning = false;

        if (pressAnyKeyText != null) pressAnyKeyText.SetActive(false);
        if (menuButtonsGroup != null) menuButtonsGroup.SetActive(true);

        if (startButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            startButton.Select();
            startButton.OnSelect(null);
        }
    }
}