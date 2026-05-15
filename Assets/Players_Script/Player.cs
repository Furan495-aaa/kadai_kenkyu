using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("移動")]
    public float maxSpeed = 8f;

    public float groundAcceleration = 25f;
    public float groundDeceleration = 30f;

    public float airAcceleration = 15f;
    public float airDeceleration = 10f;

    [Header("ジャンプ")]
    public float jumpForce = 14f;

    [Header("二段ジャンプ")]
    public int maxJumpCount = 2;

    [Header("可変ジャンプ")]
    public float jumpCutMultiplier = 0.5f;

    [Header("重力")]
    public float normalGravity = 3f;
    public float fallGravity = 6f;
    public float lowJumpGravity = 5f;

    [Header("落下")]
    public float maxFallSpeed = -20f;

    [Header("Coyote Time")]
    public float coyoteTime = 0.1f;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.1f;

    [Header("接地判定")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;

    private float moveInput;

    private bool isGrounded;

    private float coyoteTimer;
    private float jumpBufferTimer;

    // 現在のジャンプ回数
    private int jumpCount;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 横移動入力
        moveInput = Input.GetAxisRaw("Horizontal");

        // 接地判定
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Coyote Time
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;

            // 地面についたらジャンプ回数回復
            jumpCount = 0;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // Jump Buffer
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        // ジャンプ処理
        if (jumpBufferTimer > 0)
        {
            // 地上ジャンプ
            if (coyoteTimer > 0)
            {
                Jump();

                jumpBufferTimer = 0;
                coyoteTimer = 0;

                jumpCount = 1;
            }
            // 空中ジャンプ
            else if (jumpCount < maxJumpCount)
            {
                Jump();

                jumpBufferTimer = 0;

                jumpCount++;
            }
        }

        // 可変ジャンプ
        if (Input.GetButtonUp("Jump") &&
            rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
        }

        // 重力制御
        ApplyBetterJumpGravity();

        // 最大落下速度
        ClampFallSpeed();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        float targetSpeed = moveInput * maxSpeed;

        float speedDifference =
            targetSpeed - rb.linearVelocity.x;

        float accelerationRate;

        // 地上と空中で加速を変える
        if (isGrounded)
        {
            accelerationRate =
                (Mathf.Abs(targetSpeed) > 0.01f)
                ? groundAcceleration
                : groundDeceleration;
        }
        else
        {
            accelerationRate =
                (Mathf.Abs(targetSpeed) > 0.01f)
                ? airAcceleration
                : airDeceleration;
        }

        float movement =
            speedDifference * accelerationRate;

        rb.AddForce(Vector2.right * movement);
    }

    void Jump()
    {
        // 落下速度リセット
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            0f
        );

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );
    }

    void ApplyBetterJumpGravity()
    {
        // 落下中
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallGravity;
        }
        // 上昇中でボタンを離した
        else if (rb.linearVelocity.y > 0 &&
                 !Input.GetButton("Jump"))
        {
            rb.gravityScale = lowJumpGravity;
        }
        // 通常
        else
        {
            rb.gravityScale = normalGravity;
        }
    }

    void ClampFallSpeed()
    {
        if (rb.linearVelocity.y < maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                maxFallSpeed
            );
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}