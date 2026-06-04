using UnityEngine;
using System.Collections;

public class Enemy_Bullet : MonoBehaviour
{
    [Header("攻撃力")]
    [SerializeField] private int attack = 1;

    [Header("持続時間")]
    [SerializeField] private int timeToDestroy = 5;

    void Start()
    {
        //破壊カウントダウン
        StartCoroutine(timeCount());
    }

    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //プレイヤー体力取得
        if (collision.gameObject.CompareTag("Player"))
        {
            //ダメージ

            //消える
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            //消える
            Destroy(gameObject);
        }
    }

    IEnumerator timeCount()
    {
        //待機
        yield return new WaitForSeconds(timeToDestroy);

        //消える
        Destroy(gameObject);
    }
}
