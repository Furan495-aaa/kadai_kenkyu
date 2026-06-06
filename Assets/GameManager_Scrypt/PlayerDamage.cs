using System.Collections;
using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    [Header("無敵時間")]
    public float invincibleTime = 1f;

    [Header("ノックバック")]
    public float knockbackForce = 10f;
    public float knockbackUpForce = 5f;

    private bool isInvincible;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int damage, Vector2 damageSource)
    {
        if (isInvincible)
            return;

        PlayerData.Instance.TakeDamage(damage);

        Vector2 knockbackDirection =
            ((Vector2)transform.position - damageSource).normalized;

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(
            new Vector2(
                knockbackDirection.x * knockbackForce,
                knockbackUpForce
            ),
            ForceMode2D.Impulse
        );

        StartCoroutine(InvincibleCoroutine());
    }

    IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;

        float timer = 0f;

        while (timer < invincibleTime)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);

            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);

            timer += 0.2f;
        }

        spriteRenderer.enabled = true;

        isInvincible = false;
    }
}