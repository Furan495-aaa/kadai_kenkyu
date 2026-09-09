using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapCollider2D))]
public class TriggerOnlyTilemapFade : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float transparentAlpha = 0.3f;

    private Tilemap tilemap;
    private Color normalColor;
    private Color transparentColor;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        normalColor = tilemap.color;
        transparentColor = new Color(normalColor.r, normalColor.g, normalColor.b, transparentAlpha);

        TilemapCollider2D col = GetComponent<TilemapCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("何かが触れました: " + collision.gameObject.name + " (Tag: " + collision.tag + ")");

        if (collision.CompareTag("Player"))
        {
            Debug.Log("プレイヤーを検知して透過します！");
            tilemap.color = transparentColor;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            tilemap.color = normalColor;
        }
    }
}