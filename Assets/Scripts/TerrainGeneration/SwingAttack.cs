using UnityEngine;

public class SwingAttack : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<DestructibleTile>())
        {
            DestructibleTile destructibleTile = other.GetComponent<DestructibleTile>();
            destructibleTile.TakeDamage(1);
        }
    }
}









