using UnityEngine;

public class Enemy_Bullet : MonoBehaviour
{
    [SerializeField] private int attack = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //プレイヤー体力取得
        //ダメージ
        if (collision.gameObject.CompareTag("Ground"))
        {
            //消える
            Destroy(gameObject);
        }
    }
}
