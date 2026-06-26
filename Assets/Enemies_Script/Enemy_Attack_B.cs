using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class Enemy_Attack_B : MonoBehaviour
{
    [Header("攻撃目標")]
    [SerializeField] private Transform target;

    [Header("弾の元本")]
    [SerializeField] GameObject enemiesBullet;

    [Header("弾速")]
    [SerializeField] float bulletSpeedX = 8f;
    [SerializeField] float bulletSpeedY = 8f;

    [UnitHeaderInspectable("発射位置補正値")]
    [SerializeField] float adjustPosX = 1f;
    [SerializeField] float adjustPosY = 1f;

    [Header("発射間隔")]
    [SerializeField] float intervalTime = 0.5f;

    //攻撃可能フラグ
    private bool canAttack = true;

    //向き
    private int rotaY = 0;

    void Update()
    {
        //攻撃可能なら
        if (canAttack)
        {
            //攻撃
            Shoot();
        }
    }

    void Shoot()
    {
        //攻撃可能フラグを折る
        canAttack = false;

        //方向
        Vector2 direction;

        //発射位置
        Vector2 shootPos;

        if (transform.position.x < target.position.x)
        {
            //方向=右
            direction = Vector2.right;
            rotaY = 180;

            //発射位置=敵位置+補正値
            shootPos = new Vector2(transform.position.x+adjustPosX, transform.position.y+adjustPosY);
        }
        else
        {
            //方向=左
            direction = Vector2.left;
            rotaY = 0;

            //発射位置=敵位置+補正値
            shootPos = new Vector2(transform.position.x-adjustPosX, transform.position.y+adjustPosY);
        }

        //弾を配置
        GameObject bullet = Instantiate(enemiesBullet, shootPos, Quaternion.Euler(0, rotaY, 0));

        //弾の判定を取得
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        //弾を発射
        rb.AddForce(direction*bulletSpeedX, ForceMode2D.Impulse);
        //rb.AddForce(Vector2.up*bulletSpeedY, ForceMode2D.Impulse);

        //待機後再攻撃可能化
        StartCoroutine(AttackInterval());
    }

    IEnumerator AttackInterval()
    {
        //発射間隔分待機
        yield return new WaitForSeconds(intervalTime);

        //攻撃可能フラグを建てる
        canAttack = true;
    }
}
