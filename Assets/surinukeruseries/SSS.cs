using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapCollider2D))]
public class DisappearingTilePlatform : MonoBehaviour
{
    private Tilemap tilemap;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ぶつかった相手がプレイヤーかチェック
        if (collision.CompareTag("Player"))
        {
            // プレイヤーの足元の中心あたりのワールド座標を取得
            Vector3 hitPosition = collision.bounds.center;
            hitPosition.y = collision.bounds.min.y; // プレイヤーの足元のY座標

            // タイルの座標に変換して消す
            Vector3Int cellPosition = tilemap.WorldToCell(hitPosition);

            if (tilemap.HasTile(cellPosition))
            {
                tilemap.SetTile(cellPosition, null);
            }
            else
            {
                // もし足元でヒットしない場合は、プレイヤーの現在地周辺のタイルを強制的に探して消す
                cellPosition = tilemap.WorldToCell(collision.transform.position);
                if (tilemap.HasTile(cellPosition))
                {
                    tilemap.SetTile(cellPosition, null);
                }
            }
        }
    }
}