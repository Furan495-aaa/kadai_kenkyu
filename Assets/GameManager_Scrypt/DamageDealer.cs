using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerDamage player =
                other.GetComponent<PlayerDamage>();

            if (player != null)
            {
                player.TakeDamage(
                    damage,
                    transform.position
                );
            }
        }
    }
}