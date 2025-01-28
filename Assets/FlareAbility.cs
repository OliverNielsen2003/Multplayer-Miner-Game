using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class FlareAbility : NetworkBehaviour
{
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    public GameObject Effect;

    public Transform Flare;

    public void Start()
    {
        if (IsLocalPlayer) return;
        SpawnEffectClientRpc();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();

        StartCoroutine(Death());
    }

    public void Update()
    {
        UpdateTransformClientRpc();
    }

    [Rpc(SendTo.Server)]
    public void SpawnEffectClientRpc()
    {
        Debug.Log("Spawning Flare effect");
        GameObject flare = Instantiate(Effect, transform.GetChild(1).transform.position, Effect.transform.rotation);
        flare.GetComponent<NetworkObject>().Spawn();
        flare.GetComponent<ShadowEffect>().pos = transform.GetChild(1).transform;

        Flare = flare.transform;
    }

    [Rpc(SendTo.Server)]
    public void UpdateTransformClientRpc()
    {
        Flare.position = transform.GetChild(1).transform.position;
    }

    public IEnumerator Death()
    {
        yield return new WaitForSeconds(10f);
        DestroyClientRpc();
    }

    [Rpc(SendTo.Server)]
    public void DestroyClientRpc()
    {
        Destroy(gameObject);
    }
}
