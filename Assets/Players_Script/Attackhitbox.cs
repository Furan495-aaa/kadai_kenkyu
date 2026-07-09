using UnityEngine;

// AttackHitbox オブジェクトにアタッチする
public class AttackHitbox : MonoBehaviour
{
    // Player.cs の attackDamage を参照するため
    private Player player;

    void Start()
    {
        // 親（Player）から Player コンポーネントを取得
        player = GetComponentInParent<Player>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 敵に当たったらダメージを与える
        if (other.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(player.attackDamage);
        }
    }
}