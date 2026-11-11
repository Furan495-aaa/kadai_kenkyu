using UnityEngine;

public class CameraLockZone : MonoBehaviour
{
    [Tooltip("カメラを固定したい座標（XとY）")]
    public Vector3 fixedCameraPosition; 

    // 既に一度発動したかどうかを記憶するフラグ
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // プレイヤーであり、かつ「まだ一度も発動していなければ」実行
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            UnityEngine.Camera mainCam = UnityEngine.Camera.main;
            if (mainCam != null)
            {
                CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
                if (camFollow != null)
                {
                    camFollow.Lock(fixedCameraPosition);
                    
                    // 一度発動したらフラグをONにして、次から反応しないようにする
                    hasTriggered = true;
                }
            }
        }
    }
}