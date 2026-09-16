using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
public class FakePlatformEditorTransparent : MonoBehaviour
{
    [Header("--- 挙動の切り替え設定 ---")]
    [Tooltip("チェックを入れると、近づいたときに見た目を消します")]
    public bool enableReveal = true;

    [Tooltip("チェックを入れると、離れたときに消えたタイルを元に戻します")]
    public bool restoreOnLeave = true;

    [Header("--- 基本設定 ---")]
    public Transform player;              // プレイヤーのTransform
    public float revealRadius = 4.0f;     // 見た目が消える範囲（半径）

    [Header("--- 制作中の表示設定（エディタ用） ---")]
    [Tooltip("チェックを入れていると、制作中（非プレイ時）に足場を半透明にします")]
    public bool transparentInEditor = true;
    [Range(0.1f, 1.0f)]
    [Tooltip("制作中の足場の透明度（低いほど透けます）")]
    public float editorAlpha = 0.4f;

    private Tilemap targetTilemap;
    private TilemapRenderer tilemapRenderer;
    private Dictionary<Vector3Int, TileBase> hiddenTiles = new Dictionary<Vector3Int, TileBase>();
    private int radiusInCells;
    private Vector3Int prevPlayerCellPos;

    private void Awake()
    {
        targetTilemap = GetComponent<Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        radiusInCells = Mathf.CeilToInt(revealRadius);

        // ゲームプレイが始まったら、専用マテリアルのアルファ値を完全に不透明（1.0）にする
        SetMaterialAlpha(1.0f);
    }

    private void Update()
    {
        if (!Application.isPlaying) return;
        if (!enableReveal || player == null) return;

        Vector3Int currentPlayerCellPos = targetTilemap.WorldToCell(player.position);

        if (currentPlayerCellPos == prevPlayerCellPos && hiddenTiles.Count > 0)
        {
            return;
        }

        prevPlayerCellPos = currentPlayerCellPos;

        if (restoreOnLeave)
        {
            RestoreHiddenTiles(currentPlayerCellPos);
        }

        HideTilesInRange(currentPlayerCellPos);
    }

    private void RestoreHiddenTiles(Vector3Int playerCellPos)
    {
        List<Vector3Int> tilesToRestore = new List<Vector3Int>();

        foreach (var kvp in hiddenTiles)
        {
            Vector3Int cellPos = kvp.Key;
            if (Vector3Int.Distance(cellPos, playerCellPos) > radiusInCells)
            {
                tilesToRestore.Add(cellPos);
            }
        }

        foreach (var pos in tilesToRestore)
        {
            targetTilemap.SetTile(pos, hiddenTiles[pos]);
            hiddenTiles.Remove(pos);
        }
    }

    private void HideTilesInRange(Vector3Int playerCellPos)
    {
        int sideLength = radiusInCells * 2 + 1;
        Vector3Int startPos = playerCellPos - new Vector3Int(radiusInCells, radiusInCells, 0);
        
        BoundsInt area = new BoundsInt(startPos, new Vector3Int(sideLength, sideLength, 1));
        TileBase[] tiles = targetTilemap.GetTilesBlock(area);
        Vector3Int position;

        for (int x = 0; x < sideLength; x++)
        {
            for (int y = 0; y < sideLength; y++)
            {
                position = startPos + new Vector3Int(x, y, 0);
                TileBase tile = tiles[x + y * sideLength];

                if (tile != null && !hiddenTiles.ContainsKey(position))
                {
                    Vector3 worldPos = targetTilemap.CellToWorld(position) + targetTilemap.cellSize / 2f;
                    
                    if (Vector2.Distance(player.position, worldPos) <= revealRadius)
                    {
                        hiddenTiles.Add(position, tile);
                        targetTilemap.SetTile(position, null);
                    }
                }
            }
        }
    }

    // エディタ上で値を変えたときや、非プレイ時に自動で実行される
    private void OnValidate()
    {
        radiusInCells = Mathf.CeilToInt(revealRadius);

        if (!Application.isPlaying)
        {
            float targetAlpha = transparentInEditor ? editorAlpha : 1.0f;
            SetMaterialAlpha(targetAlpha);
        }
    }

    // 独立した専用マテリアル（.material）のアルファ値だけを安全に変更する
    private void SetMaterialAlpha(float alpha)
    {
        if (tilemapRenderer == null)
        {
            tilemapRenderer = GetComponent<TilemapRenderer>();
        }

        if (tilemapRenderer != null)
        {
            // .material を使用しているため、他のオブジェクト（プレイヤーや敵）には一切影響しません
            Material mat = tilemapRenderer.material;
            if (mat != null)
            {
                // シェーダーによってプロパティ名が違う場合があるため両方に対応
                if (mat.HasProperty("_Color"))
                {
                    Color col = mat.color;
                    col.a = alpha;
                    mat.color = col;
                }
                else if (mat.HasProperty("_BaseColor"))
                {
                    Color col = mat.GetColor("_BaseColor");
                    col.a = alpha;
                    mat.SetColor("_BaseColor", col);
                }
            }
        }
    }
}