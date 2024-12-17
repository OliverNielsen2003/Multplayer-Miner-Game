using System.Collections;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    public GameObject[] stonePrefabs; // Prefab for stone
    public GameObject orePrefab; // Prefab for ores
    public GameObject backgroundPrefab; // Prefab for the background
    public GameObject shrinePrefab; // Prefab for the shrine
    public GameObject[] floorDecorPrefabs;
    public GameObject[] groundDecorPrefabs;// Prefabs for floor spikes or other decor

    public int worldSize = 100;
    public float noiseFreq = 0.05f;
    public float seed;
    public Texture2D noiseTexture;

    [Range(0f, 1f)] public float oreSpawnChance = 0.1f; // Chance of spawning ores in the foreground
    [Range(0f, 1f)] public float floorDecorChance = 0.1f; // Chance of placing floor decor
    public int oreClusterSize = 5; // Maximum size of ore clusters

    public Vector2Int shrinePosition; // Position for the shrine (set in inspector or via code)
    public Vector2Int shrineSize = new Vector2Int(5, 5); // Size of the shrine

    public float tileSize = 0.5f; // Size of each tile (adjust to match your new tile art size)

    private void Start()
    {
        seed = Random.Range(-10000, 10000);
        GenerateNoiseTexture();
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                bool isTerrainPlaced = false; // Flag to track if terrain has been placed at this position

                if (noiseTexture.GetPixel(x, y).r < 0.5f) // Low-value areas, likely where stone should be
                {
                    if (Random.value < oreSpawnChance) // Chance for an ore deposit
                    {
                        GenerateOreCluster(orePrefab, x, y); // Generate ore deposit at this position
                        isTerrainPlaced = true;
                    }
                    else
                    {
                        PlaceTile(stonePrefabs[Random.Range(0, stonePrefabs.Length)], x, y); // Place stone if no ore is generated
                        isTerrainPlaced = true;
                    }
                }

                if (!isTerrainPlaced)
                {
                    PlaceTile(backgroundPrefab, x, y); // Place background where no terrain is generated
                }
            }
        }

        // After placing terrain, add floor decor
        AddFloorDecor();
        AddGroundDecor();
    }

    private void AddFloorDecor()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                // Check if there’s a terrain block at (x, y)
                GameObject floorBlock = GetTileAtPosition(x, y);
                if (floorBlock != null)
                {
                    // Check if there’s no block directly above this one (exposed top surface)
                    if (GetTileAtPosition(x, y + 1) == null)
                    {
                        // Randomly decide whether to place decor based on chance
                        if (Random.value < floorDecorChance)
                        {
                            PlaceFloorDecor(floorBlock, x, y+1);
                        }
                    }
                }
            }
        }
    }
    private void AddGroundDecor()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                // Check if there’s a terrain block at (x, y)
                GameObject floorBlock = GetTileAtPosition(x, y);
                if (floorBlock != null)
                {
                    // Check if there’s no block directly above this one (exposed top surface)
                    if (GetTileAtPosition(x, y + 1) == null)
                    {
                        // Randomly decide whether to place decor based on chance
                        if (Random.value < floorDecorChance)
                        {
                            // Position the decor on top of the block
                            Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);

                            // Instantiate decor and make it a child of the floor block
                            GameObject decor = Instantiate(groundDecorPrefabs[Random.Range(0, groundDecorPrefabs.Length)], position, Quaternion.identity);
                            decor.transform.SetParent(floorBlock.transform);
                        }
                    }
                }
            }
        }
    }

    private void PlaceFloorDecor(GameObject floorBlock, int x, int y)
    {
        // Position the decor on top of the block
        Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);

        // Instantiate decor and make it a child of the floor block
        GameObject decor = Instantiate(floorDecorPrefabs[Random.Range(0, floorDecorPrefabs.Length)], position, Quaternion.identity);
        decor.transform.SetParent(floorBlock.transform);
    }


    private void GenerateNoiseTexture()
    {
        noiseTexture = new Texture2D(worldSize, worldSize);

        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * noiseFreq, (y + seed) * noiseFreq);
                noiseTexture.SetPixel(x, y, new Color(v, v, v));
            }
        }

        noiseTexture.Apply();
    }

    private void GenerateOreCluster(GameObject orePrefab, int startX, int startY)
    {
        int clusterSize = Random.Range(1, oreClusterSize + 1);

        PlaceTile(orePrefab, startX, startY); // Place the first ore tile

        for (int i = 0; i < clusterSize - 1; i++)
        {
            int offsetX = Random.Range(-1, 2);
            int offsetY = Random.Range(-1, 2);

            int x = startX + offsetX;
            int y = startY + offsetY;

            if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
            {
                PlaceTile(orePrefab, x, y); // Place more ore tiles for the cluster
            }
        }
    }


    public void PlaceTile(GameObject prefab, int x, int y)
    {
        // Scale the position by the tile size to account for the smaller tiles
        Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);

        // Destroy any existing tile at this position
        GameObject existingTile = GetTileAtPosition(x, y);
        if (existingTile != null)
        {
            Destroy(existingTile);
        }

        // Instantiate the new tile
        GameObject tile = Instantiate(prefab, position, Quaternion.identity);
        DestructibleTile destructibleTile = tile.GetComponent<DestructibleTile>();
    }


    public GameObject GetTileAtPosition(int x, int y)
    {
        Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);
        Collider2D[] colliders = Physics2D.OverlapPointAll(position);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("DestructibleTile"))
            {
                return collider.gameObject;
            }
        }

        return null;
    }

    public void DestroyTile(int x, int y)
    {
        GameObject tile = GetTileAtPosition(x, y);
        if (tile != null)
        {
            Destroy(tile); // Destroy the GameObject at the given position
        }
    }
}
