using UnityEngine;
using Unity.Netcode;

public class DestructibleTile : NetworkBehaviour
{
    public int MaxHealth = 100;  // Maximum health of the tile
    private NetworkVariable<int> health = new NetworkVariable<int>();
    private float RegenTime = 3f;

    public SpriteRenderer spriteRenderer;  // SpriteRenderer to handle visual feedback
    public GameObject ReplacementTile;  // Prefab for destruction effects (optional)
    public GameObject destructionResidue;

    // Sprites
    public Sprite normal;
    public Sprite hurt;
    public Sprite damaged;

    private void Start()
    {
        if (IsServer)
        {
            health.Value = MaxHealth;  // Initialize health on the server
        }

        health.OnValueChanged += (oldHealth, newHealth) =>
        {
            UpdateSprite();
        };
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;  // Ensure only the server processes damage

        RegenTime = 5f;
        health.Value -= damage;

        // Update clients with new sprite and play SFX
        UpdateSpriteClientRpc();

        if (health.Value <= 0)
        {
            DestroyTile();
        }
    }

    private void DestroyTile()
    {
        if (ReplacementTile != null)
        {
            Instantiate(ReplacementTile, transform.position, Quaternion.identity);  // Instantiate a destruction effect
        }
        if (destructionResidue != null)
        {
            int rand = Random.Range(2, 4);
            for (int i = 0; i < rand; i++)
            {
                Instantiate(destructionResidue, transform.position, Quaternion.identity);
            }
        }

        // Notify all clients to destroy the tile
        DestroyTileClientRpc();

        Destroy(gameObject);  // Destroy the destructible object on the server
    }

    private void Update()
    {
        CountTimers();
    }

    private void CountTimers()
    {
        if (!IsServer) return;  // Ensure regeneration only happens on the server

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

    [ClientRpc]
    private void UpdateSpriteClientRpc()
    {
        UpdateSprite();
        // Play sound effect on all clients
        var audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PlaySFX(audioManager.Clips[0], true);
        }
    }

    [ClientRpc]
    private void DestroyTileClientRpc()
    {
        Destroy(gameObject);  // Destroy the tile on clients
    }
}
