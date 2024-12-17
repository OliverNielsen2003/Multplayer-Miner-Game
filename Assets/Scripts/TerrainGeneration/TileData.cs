using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileData : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;

    public TileData(int maxHealth)
    {
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            DestroyTile();
        }
    }

    void DestroyTile()
    {
        Destroy(gameObject);
    }
}

