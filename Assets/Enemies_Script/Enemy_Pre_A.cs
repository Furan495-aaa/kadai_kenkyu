using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Enemy_Pre_A : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 8.0f;
    private Rigidbody2D rb;

    [Header("検知の設定")]
    [SerializeField] private LayerMask groundLayer;     // 地面や壁と判定するレイヤー
    [SerializeField] private float groundCheckDistance = 0.8f; // 足元の検知距離
    [SerializeField] private float wallCheckDistance = 0.4f;   // 前方の壁の検知距離

    [Header("索敵の設定")]
    [SerializeField] private float searchRadius = 5.0f; // プレイヤーを見つける円の半径
    [SerializeField] private string targetTag = "Player"; // 検索する対象のタグ名
    private Transform detectedPlayer;                   // 見つけたプレイヤーのトランスフォーム

    // プレイヤーを一度検知したかを保持するフラグ
    private bool isChasing = false;

    // 向きを管理するフラグ（true: 右向き, false: 左向き）
    private bool isFacingRight = true;

    // 当たり判定（Collider2D）
    private Collider2D enemyCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        
        // 初期スケールから向きを推測
        if (transform.localScale.x < 0)
        {
            isFacingRight = false;
        }
        else
        {
            isFacingRight = true;
        }
    }

    void Update()
    {
        if (enemyCollider == null) return;

        Vector2 center = enemyCollider.bounds.center;

        // 1. 周囲にあるすべてのColliderを検知して、その中からプレイヤーが索敵範囲内にいるか調べる
        bool isPlayerInRadius = false;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, searchRadius);
        foreach (var col in hitColliders)
        {
            if (col.CompareTag(targetTag))
            {
                detectedPlayer = col.transform;
                isPlayerInRadius = true;
                break;
            }
        }

        // 2. 追跡状態の切り替え（範囲内なら追う、範囲外に出たら追跡をやめる）
        if (isPlayerInRadius)
        {
            isChasing = true; // 範囲内にいるので追跡モードをON
        }
        else
        {
            isChasing = false; // 範囲外に出たので追跡モードを解除（通常の巡回へ）
            detectedPlayer = null;
        }

        // 3. 進行方向の決定
        Vector2 checkDirection = isFacingRight ? Vector2.right : Vector2.left;

        // 4. 自身のコライダーに当たらないよう、少し外側からレイを飛ばす計算
        float halfWidth = enemyCollider.bounds.extents.x;
        float halfHeight = enemyCollider.bounds.extents.y;

        Vector2 wallCheckPos = new Vector2(center.x + (halfWidth + 0.1f) * (isFacingRight ? 1 : -1), center.y);
        Vector2 groundCheckPos = new Vector2(center.x + (halfWidth + 0.2f) * (isFacingRight ? 1 : -1), center.y - halfHeight + 0.1f);

        bool isWallAhead = Physics2D.Raycast(wallCheckPos, checkDirection, wallCheckDistance, groundLayer);
        bool isGroundAhead = Physics2D.Raycast(groundCheckPos, Vector2.down, groundCheckDistance, groundLayer);

        // 5. 【最優先】壁がある、または足元に地面がない場合：追跡中であっても強制的に反転する（崖から落ちない）
        if (isWallAhead || !isGroundAhead)
        {
            Flip();
        }
        // 6. 追跡中（isChasingがtrue）の場合：プレイヤーの方向を向く
        else if (isChasing && detectedPlayer != null)
        {
            if (detectedPlayer.position.x > transform.position.x && !isFacingRight)
            {
                Flip();
            }
            else if (detectedPlayer.position.x < transform.position.x && isFacingRight)
            {
                Flip();
            }
        }
    }

    void FixedUpdate()
    {
        Walk();
    }

    private void Walk()
    {
        if (isFacingRight)
        {
            rb.linearVelocityX = moveSpeed;
        }
        else
        {
            rb.linearVelocityX = -moveSpeed;
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    // UnityエディタのSceneビューで確認用のギズモを描画
    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Vector2 center = col.bounds.center;
        float halfWidth = col.bounds.extents.x;
        float halfHeight = col.bounds.extents.y;

        Vector2 wallCheckPos = new Vector2(center.x + halfWidth + 0.1f, center.y);
        Vector2 groundCheckPos = new Vector2(center.x + halfWidth + 0.2f, center.y - halfHeight + 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(wallCheckPos, wallCheckPos + Vector2.right * wallCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(groundCheckPos, groundCheckPos + Vector2.down * groundCheckDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, searchRadius);
    }
}