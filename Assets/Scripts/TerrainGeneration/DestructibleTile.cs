using UnityEngine;

public class DestructibleTile : MonoBehaviour
{
    public float MaxHealth;
    public int health;  // Starting health of the tile
    private float RegenTime = 3f;
    public SpriteRenderer spriteRenderer;  // SpriteRenderer to handle visual feedback
    public GameObject ReplacementTile;  // Prefab for destruction effects (optional)
    public GameObject destructionResidue;

    //sprites
    public Sprite normal;
    public Sprite hurt;
    public Sprite Damaged;

    // This can be called when the object takes damage
    public void TakeDamage(int damage)
    {
        RegenTime = 5f;
        health -= damage;
        GameObject.FindAnyObjectByType<AudioManager>().PlaySFX(GameObject.FindAnyObjectByType<AudioManager>().Clips[0], true);
        UpdateSprite();

        if (health <= 0)
        {
            DestroyTile();
        }
    }


    private void DestroyTile()
    {
        GameObject.FindAnyObjectByType<AudioManager>().PlaySFX(GameObject.FindAnyObjectByType<AudioManager>().Clips[1], true);
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

        Destroy(gameObject);  // Destroy the destructible object
    }

    private void Update()
    {
        CountTimers();
    }

    private void UpdateSprite()
    {
        if (health == MaxHealth)
        {
            spriteRenderer.sprite = normal;
        }
        else if (health < MaxHealth && health >= (MaxHealth / 2f))
        {
            spriteRenderer.sprite = hurt;
        }
        else if (health < MaxHealth && health < (MaxHealth / 2f))
        {
            spriteRenderer.sprite = Damaged;
        }
    }

    private void CountTimers()
    {
        if (health < MaxHealth)
        {
            RegenTime -= Time.deltaTime;
            if (RegenTime <= 0)
            {
                health++;
                UpdateSprite();
                RegenTime = 5f;
            }
        }

    }

}







