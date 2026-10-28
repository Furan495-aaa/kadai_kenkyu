using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MovingPlatform : MonoBehaviour
{
    public enum MoveAxis { Horizontal, Vertical }
    public enum MoveType { PingPong, OneShot }
    public enum MoveTrigger { Always, OnPlayerRide }

    [Header("移動設定")]
    [SerializeField] private bool isMovingPlatform = true;
    [SerializeField] private MoveTrigger moveTrigger = MoveTrigger.Always;
    [SerializeField] private MoveAxis moveAxis = MoveAxis.Horizontal;
    [SerializeField] private MoveType moveType = MoveType.PingPong;
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float moveDistance = 4.0f;

    [Header("最初の移動方向")]
    [SerializeField] private bool startTowardPositive = true; 

    private Vector2 posA;
    private Vector2 posB;
    private Vector2 currentDestination;

    private bool hasReachedEnd;
    private bool hasStartedMoving;
    private Transform ridingPlayer;

    void Start()
    {
        Vector2 startPos = transform.position;
        Vector2 dir = (moveAxis == MoveAxis.Horizontal) ? Vector2.right : Vector2.up;

        if (moveType == MoveType.PingPong)
        {
            posA = startPos - (dir * moveDistance);
            posB = startPos + (dir * moveDistance);
            currentDestination = startTowardPositive ? posB : posA;
        }
        else
        {
            posA = startPos;
            posB = startPos + (startTowardPositive ? dir : -dir) * moveDistance;
            currentDestination = posB;
        }

        hasStartedMoving = (moveTrigger == MoveTrigger.Always);

        // スプライトのBounds拡張（画面外で消えるのを防ぐ）
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Bounds b = sr.localBounds;
            b.Expand(moveDistance * 4f); 
            sr.localBounds = b;
        }
    }

    void Update()
    {
        if (!isMovingPlatform || !hasStartedMoving || (moveType == MoveType.OneShot && hasReachedEnd))
            return;

        Vector2 oldPos = transform.position;

        // 一定速度で移動
        transform.position = Vector2.MoveTowards(transform.position, currentDestination, speed * Time.deltaTime);

        // 乗っているプレイヤーを一緒に動かす
        if (ridingPlayer != null)
        {
            ridingPlayer.position += (Vector3)((Vector2)transform.position - oldPos);
        }

        // 目的地に到着したか判定
        if ((Vector2)transform.position == currentDestination)
        {
            if (moveType == MoveType.PingPong)
            {
                currentDestination = (currentDestination == posB) ? posA : posB;
            }
            else
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
                ridingPlayer = collision.transform;
            }
            else if (ridingPlayer == collision.transform)
            {
                ridingPlayer = null;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && ridingPlayer == collision.transform)
        {
            ridingPlayer = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (!isMovingPlatform) return;

        Gizmos.color = Color.red;
        Vector2 origin = transform.position;
        Vector2 dir = (moveAxis == MoveAxis.Horizontal) ? Vector2.right : Vector2.up;

        Vector2 p1, p2;
        if (moveType == MoveType.PingPong)
        {
            p1 = origin - (dir * moveDistance);
            p2 = origin + (dir * moveDistance);
        }
        else
        {
            p1 = origin;
            p2 = origin + (startTowardPositive ? dir : -dir) * moveDistance;
        }

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawWireSphere(p1, 0.2f);
        Gizmos.DrawWireSphere(p2, 0.2f);
    }
}