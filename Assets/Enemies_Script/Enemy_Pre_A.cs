using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Enemy_Pre_A : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 8.0f;
    private Rigidbody2D rb;

    [Header("検知の設定")]
    [SerializeField] private LayerMask groundLayer;         // 地面や壁と判定するレイヤー
    [SerializeField] private float wallCheckDistance = 0.4f;   // 前方の壁の検知距離

    [Header("索敵の設定")]
    [SerializeField] private float searchRadius = 5.0f;     // プレイヤーを見つける円の半径
    [SerializeField] private string targetTag = "Player";     // 検索する対象のタグ名
    [SerializeField] private float turnThreshold = 0.15f;     // 真上でのバタつきを防ぐためのしきい値
    
    private Transform detectedPlayer;                       // 見つけたプレイヤー
    private bool isChasing = false;
    private bool isFacingRight = true;
    private Collider2D enemyCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        
        // 摩擦ゼロのマテリアルを適用
        enemyCollider.sharedMaterial = new PhysicsMaterial2D("EnemySmooth") { friction = 0f, bounciness = 0f };

        isFacingRight = transform.localScale.x >= 0;
    }

    void Update()
    {
        if (enemyCollider == null) return;
        Vector2 center = enemyCollider.bounds.center;

        // 1. プレイヤーの索敵（軽量化のためヒットした最初の1つを効率よく取得）
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
        float halfHeight = enemyCollider.bounds.extents.y;

        Vector2 wallCheckPos = new Vector2(center.x + (halfWidth + 0.1f) * (isFacingRight ? 1 : -1), center.y);
        Vector2 groundCheckPos = new Vector2(center.x + (halfWidth + 0.2f) * (isFacingRight ? 1 : -1), center.y - halfHeight + 0.1f);

        bool isWallAhead = Physics2D.Raycast(wallCheckPos, isFacingRight ? Vector2.right : Vector2.left, wallCheckDistance, groundLayer);
        bool isGroundAhead = Physics2D.Raycast(groundCheckPos, Vector2.down, 0.8f, groundLayer);

        // 3. 【最優先】壁や崖がある場合は強制反転
        if (isWallAhead || !isGroundAhead)
        {
            Flip();
        }
        // 4. 追跡中：プレイヤーの方向を向く（真上でのバタつき防止対応）
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
        }
    }

    void FixedUpdate()
    {
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