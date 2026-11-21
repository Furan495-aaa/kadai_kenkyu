using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BeamTrap : MonoBehaviour
{
    [Header("ビーム設定")]
    [SerializeField] private float maxLength = 20f;
    [SerializeField] private float beamWidthBase = 0.1f;
    [SerializeField] private float beamWidthPulsateSpeed = 10f; // 振動速度
    [SerializeField] private float beamWidthPulsateAmount = 0.05f; // 振動幅
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private bool isActiveOnStart = true;

    [Header("エフェクト (粒子系)")]
    [SerializeField] private ParticleSystem startSourceEffect; // 始点の粒子
    [SerializeField] private ParticleSystem impactEffect;       // 衝突点の火花

    private LineRenderer _lineRenderer;
    private bool _isBeamActive;
    private float _pulsionTimer;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _isBeamActive = isActiveOnStart;

        // 初期化
        _lineRenderer.startWidth = beamWidthBase;
        _lineRenderer.endWidth = beamWidthBase;
        InitializeEffects();
    }

    void Update()
    {
        if (_isBeamActive)
        {
            UpdateBeam();
            AnimateBeamWidth();
        }
        else
        {
            DisableBeamVisuals();
        }
    }

    private void UpdateBeam()
    {
        Vector2 startPos = transform.position;
        Vector2 direction = transform.right; // 自身の右方向

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, maxLength, collisionLayers);

        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.enabled = true;

        // 始点エフェクトの処理
        if (startSourceEffect != null && !startSourceEffect.isPlaying) startSourceEffect.Play();

        if (hit.collider != null)
        {
            // 衝突した場合
            Vector2 hitPoint = hit.point;
            _lineRenderer.SetPosition(1, hitPoint);

            // 衝突点エフェクトの処理
            if (impactEffect != null)
            {
                impactEffect.transform.position = hitPoint;
                impactEffect.transform.rotation = Quaternion.LookRotation(hit.normal); // 壁の法線に向ける
                if (!impactEffect.isPlaying) impactEffect.Play();
            }

            // --- ダメージ判定ロジック ---
            if (hit.collider.CompareTag("Player"))
            {
                // ここでダメージを与える（クールダウン処理などを入れる）
                Debug.Log("Player Hit!");
            }
        }
        else
        {
            // 何にも当たらない場合
            Vector2 endPos = startPos + (direction * maxLength);
            _lineRenderer.SetPosition(1, endPos);

            // 衝突点エフェクトを止める
            if (impactEffect != null && impactEffect.isPlaying) impactEffect.Stop();
        }
    }

    // ビームの幅をアニメーションさせる（脈動）
    private void AnimateBeamWidth()
    {
        _pulsionTimer += Time.deltaTime * beamWidthPulsateSpeed;
        float currentWidth = beamWidthBase + (Mathf.Sin(_pulsionTimer) * beamWidthPulsateAmount);

        _lineRenderer.startWidth = currentWidth;
        _lineRenderer.endWidth = currentWidth;
    }

    // ビームが無効な時のビジュアル処理
    private void DisableBeamVisuals()
    {
        _lineRenderer.enabled = false;
        if (startSourceEffect != null && startSourceEffect.isPlaying) startSourceEffect.Stop();
        if (impactEffect != null && impactEffect.isPlaying) impactEffect.Stop();
    }

    private void InitializeEffects()
    {
        if (startSourceEffect != null)
        {
            var main = startSourceEffect.main;
            main.loop = true; // 始点エフェクトはループさせる
        }
    }

    // 外部からビームをON/OFFするためのメソッド
    public void SetBeamActive(bool active)
    {
        _isBeamActive = active;
    }
}