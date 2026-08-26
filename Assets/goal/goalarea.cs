using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GoalController : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private float fadeDuration = 1.0f; // フェード（暗転）にかかる時間
    [SerializeField] private float sceneDelay = 0.5f;   // ★追加：暗転し終わってから次のシーンに切り替わるまでの「待機時間」

    private CanvasGroup fadeCanvasGroup;

    private void Start()
    {
        GameObject fadeObj = GameObject.Find("FadeImage");
        if (fadeObj != null)
        {
            fadeCanvasGroup = fadeObj.GetComponent<CanvasGroup>();
        }

        if (fadeCanvasGroup != null)
        {
            StartCoroutine(Fade(0));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("クリア！プレイヤーを固定してフェードアウト開始");

            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                // ゴールの中心座標を渡して、プレイヤーをピタッと止めて真ん中へスライドさせる
                player.StopControl(transform.position);
            }

            // フェードアウトとシーン遷移を開始
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        // 1. フェードアウト（暗転）が完了するまで待つ
        yield return StartCoroutine(Fade(1));

        // 2. ★追加：完全に暗転した状態で、少しだけ「間（ま）」を作る（0.5秒など）
        if (sceneDelay > 0f)
        {
            yield return new WaitForSeconds(sceneDelay);
        }

        // 3. 次のシーンへ
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsedTime = 0.0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}