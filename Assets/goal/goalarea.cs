using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GoalController : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private float fadeDuration = 1.0f; // フェードにかかる時間
    [SerializeField] private float sceneDelay = 0.5f;   // 暗転後の待機時間

    private CanvasGroup fadeCanvasGroup;
    private bool isGoalTriggered = false; // 二重発動防止

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
        if (isGoalTriggered) return;

        if (collision.CompareTag("Player"))
        {
            isGoalTriggered = true;
            Debug.Log("クリア！減速しながら中心へ移動し、フェードアウト開始");

            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                // ゴールの中心座標を渡して、少しずつ減速しながらスライドさせる
                player.StartGoalSlide(transform.position);
            }

            // フェードアウトとシーン遷移のコルーチンを開始
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        // 1. フェードアウト（暗転）が完了するまで待つ
        yield return StartCoroutine(Fade(1));

        // 2. 暗転した状態で少し待つ
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