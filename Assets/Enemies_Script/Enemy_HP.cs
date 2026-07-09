using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("ステータス")]
    public int maxHp = 3;

    private int currentHp;

    void Start()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;

        // ダメージを受けたときの処理（点滅など入れてもOK）
        Debug.Log($"{gameObject.name} が {damage} ダメージを受けた！残りHP: {currentHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 死亡エフェクトなど追加できる
        Debug.Log($"{gameObject.name} が倒された！");
        Destroy(gameObject);
    }
}
