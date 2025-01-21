using System.Collections;
using UnityEngine;

public class FlareAbility : MonoBehaviour
{
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();

        Vector3 direction = Random.insideUnitCircle.normalized;
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        if (Player.GetComponent<PlayerController>().isFacingRight)
        {
            rb.AddForce(new Vector2(0.65f, 0.65f) * 5f, ForceMode2D.Impulse);
        }
        else
        {
            rb.AddForce(new Vector2(-0.65f, 0.65f) * 5f, ForceMode2D.Impulse);
        }
        rb.AddTorque(Random.Range(-0.35f, 0.35f), ForceMode2D.Impulse);

        StartCoroutine(Death());
    }

    public IEnumerator Death()
    {
        yield return new WaitForSeconds(10f);
        Destroy(gameObject);
    }
}
