using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("移動")]
    public float maxSpeed = 8f;

    public float groundAcceleration = 25f;
    public float groundDeceleration = 30f;

    public float airAcceleration = 15f;
    public float airDeceleration = 10f;

    [Header("ダッシュ")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;

    private bool isDashing;
    private float dashCooldownTimer;

    private float lastLeftPressTime;
    private float lastRightPressTime;

    public float doubleTapTime = 0.3f;

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
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    private float moveInput;

    private bool isGrounded;

    private float coyoteTimer;
    private float jumpBufferTimer;

    // 現在のジャンプ回数
    private int jumpCount;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        anim = GetComponent<Animator>();

    }

    void Update()
    {
        // 横移動入力
        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput < 0)
        {
            spriteRenderer.flipX = true;
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        HandleDashInput();

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
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        // 歩行
        anim.SetBool("Walk", moveInput != 0 && isGrounded);

        // 接地
        anim.SetBool("isGrounded", isGrounded);

        // 縦方向の速度（ジャンプ・落下判定に使用）
        anim.SetFloat("velocityY", rb.linearVelocity.y);

        
    }

    
    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        if (isDashing)
            return;
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

    void HandleDashInput()
    {
        if (isDashing || dashCooldownTimer > 0)
            return;

        if (Input.GetKeyDown(KeyCode.Q)||
        Input.GetKeyDown(KeyCode.LeftShift) ||
        Input.GetKeyDown(KeyCode.RightShift)||
        Input.GetKeyDown(KeyCode.Mouse4))
        {
            int direction;

            if (moveInput > 0)
            {
                direction = 1;
            }
            else if (moveInput < 0)
            {
                direction = -1;
            }
            else
            {
                return;
            }

            StartCoroutine(Dash(direction));
        }
    }

    IEnumerator Dash(int direction)
    {
        isDashing = true;

        float originalGravity = rb.gravityScale;

        rb.gravityScale = 0;

        rb.linearVelocity =
            new Vector2(direction * dashSpeed, 0);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;

        isDashing = false;

        dashCooldownTimer = dashCooldown;
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