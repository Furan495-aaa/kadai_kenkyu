using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MovingPlatform : MonoBehaviour
{
    public enum MoveAxis { Horizontal, Vertical }
    public enum MoveType { PingPong, OneShot }
    public enum MoveTrigger { Always, OnPlayerRide }

    [Header("移動の有無")]
    [SerializeField] private bool isMovingPlatform = true;

    [Header("移動のタイミング（トリガー）")]
    [SerializeField] private MoveTrigger moveTrigger = MoveTrigger.Always;

    [Header("移動設定")]
    [SerializeField] private MoveAxis moveAxis = MoveAxis.Horizontal;
    [SerializeField] private MoveType moveType = MoveType.PingPong;
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float moveDistance = 4.0f;

    [Header("最初の移動方向")]
    [Tooltip("チェックが入っていれば【右 / 上】へ、外していれば【左 / 下】へ最初に向かいます")]
    [SerializeField] private bool startTowardPositive = true; 

    private Vector2 posA;
    private Vector2 posB;
    private Vector2 currentDestination;

    private bool hasReachedEnd = false;
    private bool hasStartedMoving = false;
    private SpriteRenderer spriteRenderer;

    private Transform ridingPlayer = null; // 現在乗っているプレイヤー

    void Start()
    {
        // 配置した場所をそのままスタート位置とする
        Vector2 startPos = transform.position;
        Vector2 dir = (moveAxis == MoveAxis.Horizontal) ? Vector2.right : Vector2.up;

        posA = startPos - (dir * moveDistance);
        posB = startPos + (dir * moveDistance);

        if (moveType == MoveType.PingPong)
        {
            // ★修正: 勝手に transform.position を書き換えてワープさせない！
            // 配置された現在位置のまま、最初に向かう目的地（端）だけを決定する
            currentDestination = startTowardPositive ? posB : posA;
        }
        else
        {
            // 片道モードの場合も同様に現在地からスタート
            posA = startPos;
            Vector2 oneShotDir = startTowardPositive ? dir : -dir;
            posB = startPos + (oneShotDir * moveDistance);
            currentDestination = posB;
        }

        hasReachedEnd = false;
        hasStartedMoving = (moveTrigger == MoveTrigger.Always);

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
        if (!hasStartedMoving) return;
        if (moveType == MoveType.OneShot && hasReachedEnd) return;

        // 移動前の足場の位置を保存
        Vector2 oldPos = transform.position;

        // 目的地に向かって一定の速度で移動
        transform.position = Vector2.MoveTowards(transform.position, currentDestination, speed * Time.deltaTime);

        // 足場の移動量を計算
        Vector2 platformDelta = (Vector2)transform.position - oldPos;

        // プレイヤーが乗っていれば、足場の移動量だけプレイヤーを動かす（立ち絵が傾かない・滑らない）
        if (ridingPlayer != null)
        {
            ridingPlayer.position += (Vector3)platformDelta;
        }

        // 目的地に到達したかチェック
        if (Vector2.Distance(transform.position, currentDestination) < 0.005f)
        {
            transform.position = currentDestination;

            if (moveType == MoveType.PingPong)
            {
                // 目的地を反対側の端に切り替える
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
                if (moveTrigger == MoveTrigger.OnPlayerRide && !hasStartedMoving)
                {
                    hasStartedMoving = true;
                }

                // 乗っているプレイヤーを記憶
                ridingPlayer = collision.transform;
            }
            else
            {
                if (ridingPlayer == collision.transform)
                {
                    ridingPlayer = null;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (ridingPlayer == collision.transform)
            {
                ridingPlayer = null;
            }
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
            Vector2 p1 = origin - (dir * moveDistance);
            Vector2 p2 = origin + (dir * moveDistance);

            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawWireSphere(p1, 0.2f);
            Gizmos.DrawWireSphere(p2, 0.2f);
        }
        else
        {
            Vector2 oneShotDir = startTowardPositive ? dir : -dir;
            Vector2 dest = origin + (oneShotDir * moveDistance);

            Gizmos.DrawLine(origin, dest);
            Gizmos.DrawWireSphere(origin, 0.2f);
            Gizmos.DrawWireSphere(dest, 0.2f);
        }
    }
}