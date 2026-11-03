using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
public class Enemy_Attack_B8 : MonoBehaviour
{
    [Header("攻撃目標")]
    [SerializeField] private Transform target;

    [Header("弾の元本")]
    [SerializeField] GameObject enemiesBullet;

    [Header("弾速")]
    [SerializeField] float bulletSpeedX = 8f;
    //[SerializeField] float bulletSpeedY = 8f;

    [Header("発射位置補正値")]
    [SerializeField] float adjustPosX = 1f;
    [SerializeField] float adjustPosY = 1f;

    [Header("発射間隔")]
    [SerializeField] float intervalTime = 0.5f;

    //攻撃可能フラグ
    private bool canAttack = true;

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

        //発射位置
        Vector2 shootPos = new Vector2(transform.position.x+adjustPosX, transform.position.y+adjustPosY);

        //方向
        Vector2 direction = (target.transform.position - transform.position);
        float rad = Mathf.Atan2(direction.y, direction.x);
        float rotaZ = rad*Mathf.Rad2Deg + 180f;
        int dirIndex = Mathf.FloorToInt((rotaZ + 22.5f)% 360f/45f);
        Debug.Log(dirIndex);
        if (dirIndex == 2)
        {
            dirIndex = (direction.x >= 0)? 3 : 1;
        }
        Debug.Log(dirIndex);
        rad = dirIndex*(Mathf.PI/4f);
        rotaZ = rad*Mathf.Rad2Deg;
        direction = new Vector2(-Mathf.Cos(rad), -Mathf.Sin(rad));

        //弾を配置
        GameObject bullet = Instantiate(enemiesBullet, shootPos, Quaternion.Euler(0, 0, rotaZ));

        //弾の判定を取得
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        //弾を発射
        rb.AddForce(direction*bulletSpeedX, ForceMode2D.Impulse);

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
