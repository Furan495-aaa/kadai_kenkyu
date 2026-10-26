using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class Enemy_Attack_T : MonoBehaviour
{
    [Header("攻撃目標")]
    [SerializeField] private Transform target;

    [Header("速度")]
    [SerializeField] float SpeedX = 8f;
    [SerializeField] float SpeedY = 8f;

    [Header("発動距離")]
    [SerializeField] float ActRange = 1f;

    [Header("攻撃間隔")]
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
            //攻撃目標が発動距離内にいるのなら
            //攻撃
            Pounce();
        }
    }

    void Pounce()
    {
        //攻撃可能フラグを折る
        canAttack = false;

        //方向
        Vector2 direction;

        if (transform.position.x < target.position.x)
        {
            //方向=右
            direction = Vector2.right;
            rotaY = 180;
        }
        else
        {
            //方向=左
            direction = Vector2.left;
            rotaY = 0;
        }

        //飛びかかり
        

        //待機後再攻撃可能化
        StartCoroutine(AttackInterval());
    }

    IEnumerator AttackInterval()
    {
        //攻撃間隔分待機
        yield return new WaitForSeconds(intervalTime);

        //攻撃可能フラグを建てる
        canAttack = true;
    }
}
