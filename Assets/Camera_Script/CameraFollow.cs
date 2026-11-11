using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    private bool isLocked = false;
    private float lockYPosition; // Y軸の固定位置を保存する変数

    void LateUpdate()
    {
        if (player == null) return;

        if (!isLocked)
        {
            // 普段はプレイヤーのX・Y両方に追従
            transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
        }
        else
        {
            // 固定中：X軸はプレイヤーに追従し、Y軸だけは指定の高さ（lockYPosition）に固定
            transform.position = new Vector3(player.position.x, lockYPosition, transform.position.z);
        }
    }

    public void Lock(Vector3 pos) 
    { 
        lockYPosition = pos.y; // 指定された位置のY座標だけを記憶
        isLocked = true; 
    }
    
    public void Unlock() 
    { 
        isLocked = false; 
    }
}