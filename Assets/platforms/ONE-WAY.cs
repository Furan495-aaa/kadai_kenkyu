using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    private PlatformEffector2D effector;

    void Start()
    {
        effector = GetComponent<PlatformEffector2D>();
    }

    // 下に降りる入力を検知した際の処理例
    public void DisableCollision(float duration)
    {
        StartCoroutine(DisableRoutine(duration));
    }

    private System.Collections.IEnumerator DisableRoutine(float duration)
    {
        effector.enabled = false;
        yield return new WaitForSeconds(duration);
        effector.enabled = true;
    }
}
