using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyDrops : MonoBehaviour
{
    public bool isAttack = false;
    public bool isMoney = true;

    public void Death()
    {
        Destroy(gameObject);
    }

    private Rigidbody2D rb;
    private CircleCollider2D col;

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
            Death();
            GameObject.FindAnyObjectByType<AudioManager>().PlaySFX(GameObject.FindAnyObjectByType<AudioManager>().Clips[3], true);
        }
       // GameObject.FindAnyObjectByType<AudioManager>().PlaySFX(GameObject.FindAnyObjectByType<AudioManager>().Clips[2], true);
    }
}
