using System.Runtime.CompilerServices;
using UnityEditor.Callbacks;
using UnityEngine;

public class Enemy_Pre_A : MonoBehaviour
{
    [Header("目標")]
    [SerializeField] private Transform target;


    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 8.0f;
    private Rigidbody2D rb;

    private bool mem;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        mem = false;
    }

    void Update()
    {
        if (transform.position.x < target.position.x)
        {
            if (mem == false)
            {
                transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                mem = true;
            }
        }
        else
        {
            if (mem == true)
            {
                transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
                mem = false;
            }
        }

        Walk();
    }

    private void Walk()
    {
        //右を向いていたら
        if (transform.localScale.x > 0)
        {
            //右へ動く
            rb.linearVelocityX = -moveSpeed;
        }
        //左を向いていたら
        else if(transform.localScale.x < 0)
        {
            //左へ動く
            rb.linearVelocityX = moveSpeed;
        }
    }
}
