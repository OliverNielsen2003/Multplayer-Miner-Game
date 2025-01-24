using UnityEngine;
using Unity.Netcode;

public class SwingAttack : NetworkBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<DestructibleTile>())
        {
            var destructibleTile = other.GetComponent<NetworkObject>();
            if (destructibleTile != null && IsOwner)
            {
                ApplyDamageServerRpc(destructibleTile.NetworkObjectId, damage);
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
