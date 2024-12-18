using UnityEngine;
using Unity.Netcode;

public class SwingAttack : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<DestructibleTile>())
        {
            // Request the server to apply damage
            var destructibleTile = other.GetComponent<NetworkObject>();
            if (destructibleTile != null && IsOwner)
            {
                ApplyDamageServerRpc(destructibleTile.NetworkObjectId, 1); // Example: 1 damage
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyDamageServerRpc(ulong tileNetworkObjectId, int damage)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(tileNetworkObjectId, out var networkTile))
        {
            var destructibleTile = networkTile.GetComponent<DestructibleTile>();
            if (destructibleTile != null)
            {
                destructibleTile.TakeDamage(damage);
            }
        }
    }
}
