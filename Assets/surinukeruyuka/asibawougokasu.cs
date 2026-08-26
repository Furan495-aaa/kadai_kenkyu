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
    [SerializeField] private float moveDistance = 4.0f;  // 移動する距離（片道、または往復の幅）

    [Header("最初の移動方向")]
    [Tooltip("チェックが入っていれば【右 / 上】へ、外していれば【左 / 下】へ最初に向かいます")]
    [SerializeField] private bool startTowardPositive = true; 

    private Vector2 posA; // 移動の端A
    private Vector2 posB; // 移動の端B
    private Vector2 currentDestination; // 現在目指している目的地

    private bool hasReachedEnd = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        Vector2 startPos = transform.position;

        // 基準となるプラス方向ベクトル
        Vector2 dir = (moveAxis == MoveAxis.Horizontal) ? Vector2.right : Vector2.up;

        if (moveType == MoveType.PingPong)
        {
            // 往復モード：配置した場所を中心として両端を計算
            posA = startPos - (dir * moveDistance);
            posB = startPos + (dir * moveDistance);

            // ★修正: ゲーム開始時に勝手にワープさせず、
            // 「チェックが入っているなら右/上の端」へ、「外しているなら左/下の端」へ
            // 一番近い状態から自然に動くよう、現在の位置から近い方の端を目的地にする、
            // または設定された向きに合わせて最初の目的地を決定する
            if (startTowardPositive)
            {
                currentDestination = posB;
            }
            else
            {
                currentDestination = posA;
            }
        }
        else
        {
            // 片道モード：配置した場所からスタート
            posA = startPos;
            Vector2 oneShotDir = startTowardPositive ? dir : -dir;
            posB = startPos + (oneShotDir * moveDistance);
            currentDestination = posB;
        }

        hasReachedEnd = false;

        // 描画範囲の拡張（大きな値を設定しても消えないようにする）
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

        // 現在地から目的地に向かって移動
        transform.position = Vector2.MoveTowards(transform.position, currentDestination, speed * Time.deltaTime);

        // 目的地に十分近づいたか判定
        if (Vector2.Distance(transform.position, currentDestination) < 0.005f)
        {
            transform.position = currentDestination;

            if (moveType == MoveType.PingPong)
            {
                // 目的地に着いたら、AとBを入れ替えて反対側へ向かう
                currentDestination = (currentDestination == posB) ? posA : posB;
            }
            else if (moveType == MoveType.OneShot)
            {
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

        Vector2 origin = transform.position;
        Gizmos.color = Color.red;

        Vector2 dir = (moveAxis == MoveAxis.Horizontal) ? Vector2.right : Vector2.up;

        if (moveType == MoveType.PingPong)
        {
            // 往復モードのときは、現在地を中心として両側に moveDistance 分の線を引く
            Vector2 p1 = origin - (dir * moveDistance);
            Vector2 p2 = origin + (dir * moveDistance);

            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawWireSphere(p1, 0.2f);
            Gizmos.DrawWireSphere(p2, 0.2f);
        }
        else
        {
            // 片道モードのときは、現在地から指定方向へ moveDistance 進む線を引く
            Vector2 oneShotDir = startTowardPositive ? dir : -dir;
            Vector2 dest = origin + (oneShotDir * moveDistance);

            Gizmos.DrawLine(origin, dest);
            Gizmos.DrawWireSphere(origin, 0.2f);
            Gizmos.DrawWireSphere(dest, 0.2f);
        }
    }
}