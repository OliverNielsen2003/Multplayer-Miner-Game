using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShadowEffect : NetworkBehaviour
{
    public Transform pos;

    public void Awake()
    {
        StartCoroutine(Death());
    }
    void Update()
    {
        //transform.position = pos.position;
    }

    IEnumerator Death()
    {
        yield return new WaitForSecondsRealtime(10f);
        DeathClientRpc();
    }

    [Rpc(SendTo.Server)]
    public void DeathClientRpc()
    {
        Destroy(gameObject);
    }
}