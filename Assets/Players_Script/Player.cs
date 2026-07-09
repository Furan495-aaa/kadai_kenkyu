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
    // ★追加: 空中ダッシュをすでに使ったかどうかのフラグ
    private bool hasAirDashed; 

    private float lastLeftPressTime;
    private float lastRightPressTime;

    public float doubleTapTime = 0.3f;

    [Header("ジャンプ")]
    public float jumpForce = 14f;

    [Header("攻撃")]
    public GameObject attackHitbox;
    public float attackDuration = 0.2f;
    public float attackCooldown = 0.4f;
    public int attackDamage = 1;

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

    private bool isAttacking;
    private float attackCooldownTimer;
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

        HandleDashInput();

        // 接地判定
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Coyote Time / 着地時のリセット処理
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;

            // 地面についたらジャンプ回数回復
            jumpCount = 0;

            // ★追加: 地面についたら空中ダッシュのフラグをリセット（再度使用可能にする）
            hasAirDashed = false; 
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

        // ダッシュ中でない場合のみ重力と最大落下速度を制御
        if (!isDashing)
        {
            ApplyBetterJumpGravity();
            ClampFallSpeed();
        }
        // 攻撃クールダウン
        if (attackCooldownTimer > 0)
            attackCooldownTimer -= Time.deltaTime;

        // 攻撃入力（攻撃中・ダッシュ中は受け付けない）
        if (Input.GetButtonDown("Fire1") && !isAttacking && !isDashing && attackCooldownTimer <= 0)
        {
            StartCoroutine(Attack());
        }

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        anim.SetBool("Walk", moveInput != 0 && isGrounded);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("velocityY", rb.linearVelocity.y);
        anim.SetBool("isAttacking", isAttacking);
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
        float speedDifference = targetSpeed - rb.linearVelocity.x;
        float accelerationRate;

        if (isGrounded)
        {
            accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? groundAcceleration : groundDeceleration;
        }
        else
        {
            accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? airAcceleration : airDeceleration;
        }

        float movement = speedDifference * accelerationRate;
        rb.AddForce(Vector2.right * movement);
    }

    void HandleDashInput()
    {
        if (isDashing || dashCooldownTimer > 0)
            return;

        // ★修正点: 空中にいる、かつ、すでに空中ダッシュを使用済みの場合は入力を受け付けない
        if (!isGrounded && hasAirDashed)
            return;

        if (Input.GetKeyDown(KeyCode.Q) ||
            Input.GetKeyDown(KeyCode.LeftShift) ||
            Input.GetKeyDown(KeyCode.RightShift) ||
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
                direction = spriteRenderer.flipX ? -1 : 1;
            }

            // ★追加: 今回のダッシュが「空中でのダッシュ」なら、使用済みフラグを立てる
            if (!isGrounded)
            {
                hasAirDashed = true;
            }

            StartCoroutine(Dash(direction));
        }
    }

    IEnumerator Dash(int direction)
    {
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(direction * dashSpeed, 0);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
        dashCooldownTimer = dashCooldown;
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        // 向いている方向にヒットボックスを出す
        float dir = spriteRenderer.flipX ? -1f : 1f;
        attackHitbox.transform.localPosition = new Vector2(
            Mathf.Abs(attackHitbox.transform.localPosition.x) * dir,
            attackHitbox.transform.localPosition.y
        );

        attackHitbox.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        attackHitbox.SetActive(false);

        attackCooldownTimer = attackCooldown;
        isAttacking = false;
    }
    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void ApplyBetterJumpGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallGravity;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.gravityScale = lowJumpGravity;
        }
        else
        {
            rb.gravityScale = normalGravity;
        }
    }

    void ClampFallSpeed()
    {
        if (rb.linearVelocity.y < maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}