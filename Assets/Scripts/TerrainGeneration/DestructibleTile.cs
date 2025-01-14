using UnityEngine;
using Unity.Netcode;

public class DestructibleTile : NetworkBehaviour
{
    public int MaxHealth = 100;  // Maximum health of the tile
    private NetworkVariable<int> health = new NetworkVariable<int>();
    private float RegenTime = 3f;

    public SpriteRenderer spriteRenderer;
    public GameObject ReplacementTile;
    public GameObject destructionResidue;
    public GameObject destructionEffect;

    // Sprites
    public Sprite normal;
    public Sprite hurt;
    public Sprite damaged;

    private void Start()
    {
        if (IsServer)
        {
            health.Value = MaxHealth;
        }

        health.OnValueChanged += (oldHealth, newHealth) =>
        {
            UpdateSprite();
        };
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;

        RegenTime = 5f;
        health.Value -= damage;

        UpdateSpriteClientRpc();

        if (health.Value <= 0)
        {
            ReplaceTileClientRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void ReplaceTileClientRpc()
    {
        if (ReplacementTile != null)
        {
            GameObject replaceTile = Instantiate(ReplacementTile, transform.position, Quaternion.identity);
            GameObject effect = Instantiate(destructionEffect, transform.position, Quaternion.identity);
            replaceTile.GetComponent<NetworkObject>().Spawn();
            effect.GetComponent<NetworkObject>().Spawn();
        }
        if (destructionResidue != null)
        {
            int rand = Random.Range(2, 4);
            for (int i = 0; i < rand; i++)
            {
                GameObject residue = Instantiate(destructionResidue, transform.position, Quaternion.identity);
                residue.GetComponent<NetworkObject>().Spawn();
            }
        }

        DestroyChildrenClientRpc();
        DestroyTileClientRpc();
    }

    private void Update()
    {
        CountTimers();
    }

    private void CountTimers()
    {
        if (!IsServer) return;

        if (health.Value < MaxHealth)
        {
            RegenTime -= Time.deltaTime;
            if (RegenTime <= 0)
            {
                health.Value++;
                RegenTime = 5f;
            }
        }
    }

    private void UpdateSprite()
    {
        if (health.Value == MaxHealth)
        {
            spriteRenderer.sprite = normal;
        }
        else if (health.Value < MaxHealth && health.Value >= (MaxHealth / 2f))
        {
            spriteRenderer.sprite = hurt;
        }
        else if (health.Value < MaxHealth && health.Value < (MaxHealth / 2f))
        {
            spriteRenderer.sprite = damaged;
        }
    }

    [Rpc(SendTo.Server)]
    private void UpdateSpriteClientRpc()
    {
        UpdateSprite();
        var audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PlaySFX(audioManager.Clips[0], true);
        }
    }

    [Rpc(SendTo.Server)]
    private void DestroyTileClientRpc()
    {
        Destroy(gameObject);
    }

    [Rpc(SendTo.Server)]
    private void DestroyChildrenClientRpc()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
