using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("HP設定")]
    public int maxHp = 5;
    private static int currentHp = -1; 

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
    private bool hasAirDashed; 

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
    public float maxFallSpeed = -20f;

    [Header("判定猶予 (Coyote / Buffer)")]
    [Tooltip("足場から離れてもジャンプできる猶予時間（0.166秒 ＝ 60fpsで約10フレーム）")]
    public float coyoteTime = 0.166f;
    public float jumpBufferTime = 0.1f;

    [Header("接地判定")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("攻撃")]
    public GameObject attackHitbox;
    public float attackDuration = 0.2f;
    public float attackCooldown = 0.4f;
    public int attackDamage = 1;
    public float hitboxOffsetX = 0.8f;

    [Header("足音")]
    public float walkSEInterval = 0.4f;

    private bool isAttacking;
    private float attackCooldownTimer;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Collider2D playerCollider;

    private PhysicsMaterial2D smoothMaterial;
    private PhysicsMaterial2D frictionMaterial;

    private float moveInput;
    private bool isGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private int jumpCount;
    private bool canControl = true;
    private float walkSETimer;

    void Awake()
    {
        if (currentHp < 0) currentHp = maxHp;

        smoothMaterial = new PhysicsMaterial2D("PlayerSmooth") { friction = 0f, bounciness = 0f };
        frictionMaterial = new PhysicsMaterial2D("PlayerFriction") { friction = 100f, bounciness = 0f };
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (!canControl) return;

        // 横移動・反転処理
        moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != 0)
        {
            spriteRenderer.flipX = (moveInput < 0);
        }

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        HandleDashInput();

        // --- 接地判定 & コヨーテタイムの更新 ---
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            coyoteTimer = coyoteTime; // 地上にいる間は常にタイマーを最大値に維持
            jumpCount = 0;            // 地上についたらジャンプ回数リセット
            hasAirDashed = false;     // 空中ダッシュもリセット
        }
        else
        {
            // 地上から空中に外れた瞬間からタイマーを減らす
            coyoteTimer -= Time.deltaTime;
        }

        // 坂道での滑り止め切り替え
        if (playerCollider != null)
        {
            playerCollider.sharedMaterial = (isGrounded && moveInput == 0 && !isDashing) ? frictionMaterial : smoothMaterial;
        }

        // ジャンプバッファー（ボタンが押された瞬間を記憶）
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        // --- ジャンプ実行判定 ---
        if (jumpBufferTimer > 0)
        {
            // 地上ジャンプ または コヨーテタイム猶予内のジャンプ
            if (coyoteTimer > 0 || isGrounded)
            {
                Jump();
                jumpBufferTimer = 0;
                coyoteTimer = 0;     // 連続ジャンプ防止
                jumpCount = 1;       // 地上ジャンプとして1回分カウント
            }
            // 空中ジャンプ（二段ジャンプなど）
            else if (jumpCount < maxJumpCount)
            {
                Jump();
                jumpBufferTimer = 0;
                jumpCount++;
            }
        }

        // 可変ジャンプ（ボタンを離したときに上昇を抑える）
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        if (!isDashing)
        {
            ApplyBetterJumpGravity();
            ClampFallSpeed();
        }

        // 攻撃クールダウン
        if (attackCooldownTimer > 0) attackCooldownTimer -= Time.deltaTime;

        // 攻撃入力
        if (Input.GetButtonDown("Fire1") && !isAttacking && !isDashing && attackCooldownTimer <= 0)
        {
            StartCoroutine(Attack());
        }

        UpdateAnimation();
        HandleWalkSE();
    }

    void UpdateAnimation()
    {
        bool isMoving = moveInput != 0;
        anim.SetBool("Walk", isMoving && (isGrounded || coyoteTimer > 0));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("velocityY", rb.linearVelocity.y);
        anim.SetBool("isAttacking", isAttacking);
    }

    void FixedUpdate()
    {
        if (!canControl) return;
        Move();
    }

    void Move()
    {
        if (isDashing) return;

        float targetSpeed = moveInput * maxSpeed;
        float speedDifference = targetSpeed - rb.linearVelocity.x;
        float accelerationRate = isGrounded ? 
            ((Mathf.Abs(targetSpeed) > 0.01f) ? groundAcceleration : groundDeceleration) : 
            ((Mathf.Abs(targetSpeed) > 0.01f) ? airAcceleration : airDeceleration);

        rb.AddForce(Vector2.right * (speedDifference * accelerationRate));
    }

    void HandleDashInput()
    {
        if (isDashing || dashCooldownTimer > 0 || (!isGrounded && hasAirDashed)) return;

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftShift) || 
            Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.Mouse4))
        {
            int direction = (moveInput != 0) ? (int)Mathf.Sign(moveInput) : (spriteRenderer.flipX ? -1 : 1);
            if (!isGrounded) hasAirDashed = true;
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
        AudioManager.Instance.Play(AudioManager.Instance.attackSE);

        float dir = spriteRenderer.flipX ? -1f : 1f;
        attackHitbox.transform.localPosition = new Vector2(hitboxOffsetX * dir, attackHitbox.transform.localPosition.y);
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
        AudioManager.Instance.Play(AudioManager.Instance.jumpSE);
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

    public bool IsGrounded { get { return isGrounded; } }

    public void TakeDamage(int damage)
    {
        currentHp = Mathf.Max(0, currentHp - damage);
        Debug.Log("ダメージを受けた！現在のHP: " + currentHp);
        if (currentHp <= 0) { /* ゲームオーバー処理 */ }
    }

    public void Heal(int amount)
    {
        currentHp = Mathf.Min(maxHp, currentHp + amount);
        Debug.Log("回復した！現在のHP: " + currentHp);
    }

    public int GetCurrentHp() => currentHp;
    public static void ResetHp() => currentHp = -1;

    public void StartGoalSlide(Vector3 goalPosition)
    {
        canControl = false;
        moveInput = 0;
        if (anim != null) anim.SetBool("Walk", false);
        StartCoroutine(GoalSlideRoutine(goalPosition));
    }

    private IEnumerator GoalSlideRoutine(Vector3 goalPosition)
    {
        float startX = transform.position.x;
        float offset = 0.5f;
        float targetX = (startX < goalPosition.x) ? (goalPosition.x - offset) : (goalPosition.x + offset);
        float slideSpeed = 10f;

        while (true)
        {
            float currentX = Mathf.MoveTowards(transform.position.x, targetX, slideSpeed * Time.deltaTime);
            bool currentlyGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            if (currentlyGrounded || Mathf.Abs(currentX - targetX) < 0.01f)
            {
                transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
                rb.linearVelocity = Vector2.zero;
                break;
            }

            transform.position = new Vector3(currentX, transform.position.y, transform.position.z);
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            yield return null;
        }
    }

    void HandleWalkSE()
    {
        if (isGrounded && Mathf.Abs(moveInput) > 0 && !isDashing)
        {
            walkSETimer -= Time.deltaTime;
            if (walkSETimer <= 0)
            {
                AudioManager.Instance.Play(AudioManager.Instance.walkSE);
                walkSETimer = walkSEInterval;
            }
        }
        else
        {
            walkSETimer = 0;
        }
    }
}