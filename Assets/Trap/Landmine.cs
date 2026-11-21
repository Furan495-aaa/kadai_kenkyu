using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Landmine : MonoBehaviour
{
    [Header("検知・タイミング設定")]
    [Tooltip("反応するタグ（例: Player, Enemy）")]
    [SerializeField] private string[] triggerTags = { "Player" };

    [Tooltip("踏まれてから爆発するまでの時間（秒）")]
    [SerializeField] private float fuseDelay = 0.3f;

    [Tooltip("予兆中の点滅間隔（秒）")]
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("爆発の設定")]
    [Tooltip("爆発の半径")]
    [SerializeField] private float explosionRadius = 2.0f;

    [Tooltip("爆発ダメージ")]
    [SerializeField] private int damage = 100;

    [Tooltip("吹き飛ばす力（ノックバック）")]
    [SerializeField] private float knockbackForce = 10f;

    [Tooltip("ダメージを与える対象のレイヤー")]
    [SerializeField] private LayerMask damageableLayers;

    [Header("演出（任意）")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip armSound;
    [SerializeField] private AudioClip explodeSound;

    private bool isTriggered = false;
    private Color originalColor;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // トリガー判定を確実にする
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTriggered) return;

        if (HasMatchingTag(other.gameObject))
        {
            isTriggered = true;
            StartCoroutine(ArmAndExplodeRoutine());
        }
    }

    private bool HasMatchingTag(GameObject target)
    {
        foreach (string tagName in triggerTags)
        {
            if (!string.IsNullOrEmpty(tagName) && target.CompareTag(tagName))
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator ArmAndExplodeRoutine()
    {
        if (audioSource != null && armSound != null)
        {
            audioSource.PlayOneShot(armSound);
        }

        float elapsed = 0f;
        bool toggleColor = false;

        // 爆発までのカウントダウン中に点滅させる
        while (elapsed < fuseDelay)
        {
            if (spriteRenderer != null)
            {
                toggleColor = !toggleColor;
                spriteRenderer.color = toggleColor ? warningColor : originalColor;
            }

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        Explode();
    }

    private void Explode()
    {
        // 爆発エフェクト生成
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 爆発音再生
        if (audioSource != null && explodeSound != null)
        {
            audioSource.PlayOneShot(explodeSound);
        }

        // 範囲内の対象を取得
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);

        foreach (Collider2D hit in hits)
        {
            // ダメージ処理（プロジェクトのHealth/ダメージ用コンポーネントに合わせて変更してください）
            // 例: IDamageable や PlayerHealth など
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }

            // ノックバック処理（Rigidbody2Dを持つ対象を爆心地から押し出す）
            if (hit.TryGetComponent<Rigidbody2D>(out var rb))
            {
                Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                // 少し上方向への跳ね上がりを入れると2Dアクションらしくなりやすい
                dir = (dir + Vector2.up * 0.35f).normalized;
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }

        // 地雷本体を削除（音を鳴らし切る必要がある場合はSE再生用オブジェクトを別にするかDestroy遅延を調整）
        Destroy(gameObject);
    }

    // シーンビューで爆発範囲を視覚化
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawSphere(transform.position, explosionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

// プロジェクトに合わせて既存のダメージ処理に置き換えてもOKです
public interface IDamageable
{
    void TakeDamage(int amount);
}