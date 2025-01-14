using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DestroyDrops : MonoBehaviour
{
    public bool isAttack = false;
    public bool isMoney = true;
    private Rigidbody2D rb;
    private CircleCollider2D col;

    [ClientRpc]
    public void DeathClientRpc()
    {
        Destroy(gameObject);
    }

    void Start()
    {
        if (!isAttack)
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CircleCollider2D>();

            Vector3 direction = Random.insideUnitCircle.normalized;
            rb.AddForce(-direction, ForceMode2D.Impulse);
        }
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (isMoney && other.transform.tag == "Player")
        {
            DeathClientRpc();
            GameObject.FindAnyObjectByType<AudioManager>().PlaySFX(GameObject.FindAnyObjectByType<AudioManager>().Clips[3], true);
        }
       // GameObject.FindAnyObjectByType<AudioManager>().PlaySFX(GameObject.FindAnyObjectByType<AudioManager>().Clips[2], true);
    }
}
