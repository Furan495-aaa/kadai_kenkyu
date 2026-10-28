using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))] // 画面外自動停止に必要
public class Enemy_Flight : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 8.0f;
    [SerializeField] private float upSpeed = 8.0f;
    [SerializeField] private float downSpeed = 8.0f;

    private Rigidbody2D rb;

    [Header("プレイヤーとの距離")]
    [SerializeField] private float YHDist = 10;
    [SerializeField] private float YLDist = 10;

    [Header("検知の設定")]
    [SerializeField] private LayerMask groundLayer;          // 壁と判定するレイヤー
    [SerializeField] private float wallCheckDistance = 0.4f;   // 前方の壁の検知距離

    [Header("索敵の設定")]
    [SerializeField] private float searchRadius = 5.0f;     // プレイヤーを見つける円の半径
    [SerializeField] private string targetTag = "Player";     // 検索する対象のタグ名
    [SerializeField] private float turnThreshold = 0.15f;     // 真上・真下でのバタつきを防ぐしきい値

    private Transform detectedPlayer;                       // 見つけたプレイヤー
    private bool isChasing = false;
    private bool isFacingRight = true;
    private Collider2D enemyCollider;
    private bool isVisibleOnScreen = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        isFacingRight = transform.localScale.x >= 0;
    }

    // ─── 画面外自動停止 ───
    void OnBecameVisible() { isVisibleOnScreen = true; }
    void OnBecameInvisible() 
    { 
        isVisibleOnScreen = false; 
        if (rb != null) { rb.linearVelocityX = 0; rb.linearVelocityY = 0; }
    }

    void Update()
    {
        if (!isVisibleOnScreen || enemyCollider == null) return;

        Vector2 center = enemyCollider.bounds.center;

        // 1. プレイヤーの索敵（最初に見つかった対象を取得して軽量化）
        detectedPlayer = null;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, searchRadius);
        foreach (var col in hitColliders)
        {
            if (col.CompareTag(targetTag))
            {
                detectedPlayer = col.transform;
                break;
            }
        }
        isChasing = (detectedPlayer != null);

        // 2. 進行方向のレイキャスト判定（壁チェック）
        float halfWidth = enemyCollider.bounds.extents.x;
        Vector2 wallCheckPos = new Vector2(center.x + (halfWidth + 0.1f) * (isFacingRight ? 1 : -1), center.y);
        bool isWallAhead = Physics2D.Raycast(wallCheckPos, isFacingRight ? Vector2.right : Vector2.left, wallCheckDistance, groundLayer);

        // 3. 【最優先】壁がある場合は強制反転
        if (isWallAhead)
        {
            Flip();
        }
        // 4. 追跡中：プレイヤーの方向を向く ＆ 上下移動（真上でのバタつき防止対応）
        else if (isChasing)
        {
            float xDiff = detectedPlayer.position.x - transform.position.x;

            // X座標の差が「turnThreshold」以上離れているときだけ向きを変える
            if (xDiff > turnThreshold && !isFacingRight)
            {
                Flip();
            }
            else if (xDiff < -turnThreshold && isFacingRight)
            {
                Flip();
            }

            // 上下移動の制御
            if (detectedPlayer.position.y + YHDist > transform.position.y)
            {
                rb.linearVelocityY = upSpeed;
            }
            else if (detectedPlayer.position.y + YLDist < transform.position.y)
            {
                rb.linearVelocityY = -downSpeed;
            }
            else
            {
                rb.linearVelocityY = 0;
            }
        }
        else
        {
            rb.linearVelocityY = 0;
        }
    }

    void FixedUpdate()
    {
        if (!isVisibleOnScreen) return;
        rb.linearVelocityX = isFacingRight ? moveSpeed : -moveSpeed;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(col.bounds.center, searchRadius);
    }
}