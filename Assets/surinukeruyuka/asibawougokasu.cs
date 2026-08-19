using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MovingPlatform : MonoBehaviour
{
    public enum MoveAxis
    {
        Horizontal, // 左右
        Vertical    // 上下
    }

    public enum MoveType
    {
        PingPong,   // 往復運動する
        OneShot     // 端まで行ったら停止する
    }

    [Header("移動の有無")]
    [SerializeField] private bool isMovingPlatform = true;

    [Header("移動設定")]
    [SerializeField] private MoveAxis moveAxis = MoveAxis.Horizontal;
    [SerializeField] private MoveType moveType = MoveType.PingPong;
    [SerializeField] private float speed = 3.0f;       // 1秒間に進むメートル数
    [SerializeField] private float moveDistance = 4.0f;  // 移動する距離

    [Header("最初の移動方向")]
    [Tooltip("チェックが入っていれば【右 / 上】へ、外していれば【左 / 下】へ最初に向かいます")]
    [SerializeField] private bool startTowardPositive = true; 

    private Vector2 startPos;
    private Vector2 targetPos;
    private Vector2 currentDestination;
    private bool movingToTarget = true;
    private bool hasReachedEnd = false;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPos = transform.position;

        // 軸と方向から、ゴールの位置を正確に計算する
        Vector2 dir = (moveAxis == MoveAxis.Horizontal) ? Vector2.right : Vector2.up;
        if (!startTowardPositive)
        {
            dir = -dir;
        }

        targetPos = startPos + (dir * moveDistance);
        
        // 最初の目的地を設定
        currentDestination = targetPos;
        movingToTarget = true;
        hasReachedEnd = false;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Bounds localBounds = spriteRenderer.localBounds;
            localBounds.Expand(moveDistance * 4f); 
            spriteRenderer.localBounds = localBounds;
        }
    }

    void Update()
    {
        if (!isMovingPlatform) return;
        if (moveType == MoveType.OneShot && hasReachedEnd) return;

        // 毎フレーム、現在地から目的地へ一定のスピードで近づく
        transform.position = Vector2.MoveTowards(transform.position, currentDestination, speed * Time.deltaTime);

        // 目的地に到着したかどうかの判定
        if (Vector2.Distance(transform.position, currentDestination) < 0.001f)
        {
            if (moveType == MoveType.PingPong)
            {
                // 往復モード：目的地に着いたら、次に向かう先を反対側に切り替える
                if (movingToTarget)
                {
                    currentDestination = startPos;
                    movingToTarget = false;
                }
                else
                {
                    currentDestination = targetPos;
                    movingToTarget = true;
                }
            }
            else if (moveType == MoveType.OneShot)
            {
                // 片道モード：目的地に着いたらそこで停止フラグを立てる
                hasReachedEnd = true;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null && player.IsGrounded)
            {
                collision.transform.SetParent(transform);
            }
            else
            {
                if (collision.transform.parent == transform)
                {
                    collision.transform.SetParent(null);
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }

    private void OnDrawGizmos()
    {
        if (!isMovingPlatform) return;

        Vector2 origin = Application.isPlaying ? startPos : (Vector2)transform.position;
        Gizmos.color = Color.red;

        // 描画用の方向計算
        Vector2 dir = (moveAxis == MoveAxis.Horizontal) ? Vector2.right : Vector2.up;
        if (!startTowardPositive)
        {
            dir = -dir;
        }

        Vector2 dest = origin + (dir * moveDistance);

        if (moveType == MoveType.PingPong)
        {
            // 往復のときは反対側も含めた全域を表示
            Vector2 oppositeDest = origin - (dir * moveDistance);
            Gizmos.DrawLine(oppositeDest, dest);
            Gizmos.DrawWireSphere(oppositeDest, 0.2f);
            Gizmos.DrawWireSphere(dest, 0.2f);
        }
        else
        {
            // 片道のときはスタートから目的地までを表示
            Gizmos.DrawLine(origin, dest);
            Gizmos.DrawWireSphere(origin, 0.2f);
            Gizmos.DrawWireSphere(dest, 0.2f);
        }
    }
}