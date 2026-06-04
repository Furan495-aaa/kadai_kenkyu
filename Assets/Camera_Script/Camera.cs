using System;
using UnityEngine;

public class Camera : MonoBehaviour
{
    //対象の位置取得
    [Header("追従対象")]
    [SerializeField] private Transform target;

    //移動範囲限度値設定
    [Header("追従限界")]
    [SerializeField] private float xmin;
    [SerializeField] private float xmax;
    [SerializeField] private float ymin;
    [SerializeField] private float ymax;

    void Update()
    {
        FollowTarget();
    }

    //カメラの位置に対象の位置を代入
    private void FollowTarget()
    {
        if (target != null)
        {
            float x = Mathf.Clamp(target.position.x, xmin, xmax);
            float y = Mathf.Clamp(target.position.y, ymin, ymax);
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}
