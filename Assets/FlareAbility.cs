using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class FlareAbility : NetworkBehaviour
{
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    public GameObject Effect;

    public GameObject Flare;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject flare = Instantiate(Effect, transform.GetChild(1).transform.position, Effect.transform.rotation);
        flare.GetComponent<ShadowEffect>().pos = transform.GetChild(1).transform;
        Flare = flare;

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
        DestroyClientRpc();
    }

    [Rpc(SendTo.Server)]
    public void DestroyClientRpc()
    {
        Destroy(Flare);
        Destroy(gameObject);
    }
}
