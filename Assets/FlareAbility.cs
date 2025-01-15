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
        rb.AddForce(-direction * 5f, ForceMode2D.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
