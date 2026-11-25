using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TitleManager : MonoBehaviour
{
    [Serializable]
    private struct SeSetting
    {
        public AudioClip clip;
        [Range(0f, 1.5f)] public float volumeScale;
        // マイナス値: 先行再生の秒数 / 0: 同時 / プラス値: 遅延秒数
        [Range(-2f, 2f)] public float timingOffsetSeconds;
    }

    [Header("シーン・移動設定")]
    [SerializeField] private string firstStageSceneName = "Stage1";
    [SerializeField] private MonoBehaviour playerMoveScript;

    [Header("UIパーツ")]
    [SerializeField] private GameObject pressAnyKeyText;
    [SerializeField] private GameObject menuButtonsGroup;
    [SerializeField] private Button startButton;

    [Header("サウンド設定（SEごとに個別調整）")]
    [SerializeField] private AudioSource seAudioSource;
    [SerializeField] private SeSetting pressAnyKeySE = new SeSetting { volumeScale = 1f, timingOffsetSeconds = 0f };
    [SerializeField] private SeSetting submitSE = new SeSetting { volumeScale = 1f, timingOffsetSeconds = 0f };
    [SerializeField] private SeSetting cancelSE = new SeSetting { volumeScale = 1f, timingOffsetSeconds = 0f };

    [Header("点滅演出の速度設定")]
    [SerializeField] private float idleFadeSpeed = 2.0f;
    [SerializeField] private float pressAnyKeyFlashInterval = 0.06f;

    private TMP_Text pressAnyKeyTMP;
    private TMP_Text[] menuButtonTexts;
    private bool isMenuOpened = false;
    private bool isTransitioning = false;

    private GameObject lastSelectedObj = null;
    private float selectStartTime = 0f;

    void Start()
    {
        Time.timeScale = 1f;

        if (seAudioSource == null)
        {
            seAudioSource = GetComponent<AudioSource>();
        }

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
        if (!isMenuOpened)
        {
            if (!isTransitioning)
            {
                FadeBlinkText(pressAnyKeyTMP, Time.time);

                if (Input.anyKeyDown)
                {
                    if (gameObject.activeInHierarchy)
                    {
                        StartCoroutine(PressedBlinkAndOpenMenuRoutine());
                    }
                    else
                    {
                        PlaySeImmediate(pressAnyKeySE);
                        OpenMainMenu();
                    }
                }
            }
            return;
        }

        if (isTransitioning) return;

        // Backspaceキーでキャンセルしてタイトル待機に戻る
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            TriggerSe(cancelSE);
            ReturnToTitleWait();
            return;
        }

        UpdateSelectedButtonBlink();

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

    private void PlaySeImmediate(SeSetting setting)
    {
        if (seAudioSource == null || setting.clip == null) return;
        if (!seAudioSource.enabled || !seAudioSource.gameObject.activeInHierarchy) return;
        seAudioSource.PlayOneShot(setting.clip, setting.volumeScale);
    }

    private void TriggerSe(SeSetting setting)
    {
        if (setting.timingOffsetSeconds > 0f && gameObject.activeInHierarchy)
        {
            StartCoroutine(PlaySeAfterDelayRoutine(setting, setting.timingOffsetSeconds));
        }
        else
        {
            PlaySeImmediate(setting);
        }
    }

    private IEnumerator PlaySeAfterDelayRoutine(SeSetting setting, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySeImmediate(setting);
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

    private IEnumerator PressedBlinkAndOpenMenuRoutine()
    {
        isTransitioning = true;

        float offset = pressAnyKeySE.timingOffsetSeconds;

        if (offset < 0f)
        {
            PlaySeImmediate(pressAnyKeySE);
            yield return new WaitForSeconds(-offset);
        }
        else if (offset > 0f)
        {
            StartCoroutine(PlaySeAfterDelayRoutine(pressAnyKeySE, offset));
        }
        else
        {
            PlaySeImmediate(pressAnyKeySE);
        }

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

    public void OnClickStartButton()
    {
        GoToScreen1();
    }

    public void GoToScreen1()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        PlaySeImmediate(submitSE);

        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = true;
        }

        string targetScene = string.IsNullOrEmpty(firstStageSceneName) ? "Stage1" : firstStageSceneName;
        SceneManager.LoadScene(targetScene);
    }

    public void OnClickQuitButton()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        PlaySeImmediate(submitSE);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OpenMainMenu()
    {
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