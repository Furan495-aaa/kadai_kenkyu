using System.Runtime.CompilerServices;
using UnityEditor.Callbacks;
using UnityEngine;

public class Enemy_Pre_A : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 5.0f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
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
