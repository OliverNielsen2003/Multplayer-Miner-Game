using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class TerrainGeneration : NetworkBehaviour
{
    public GameObject[] stonePrefabs; // Prefabs for stone
    public GameObject orePrefab; // Prefab for ores
    public GameObject backgroundPrefab; // Prefab for the background
    public GameObject shrinePrefab; // Prefab for the shrine
    public GameObject[] floorDecorPrefabs; // Floor decor prefabs
    public GameObject[] groundDecorPrefabs; // Ground decor prefabs
    public GameObject[] ceilingDecorPrefabs; // Ceiling decor prefabs

    public int worldSize = 100;
    public float noiseFreq = 0.05f;
    private Texture2D noiseTexture;

    [Range(0f, 1f)] public float oreSpawnChance = 0.1f;
    [Range(0f, 1f)] public float floorDecorChance = 0.1f;
    public int oreClusterSize = 5;

    public Vector2Int shrinePosition;
    public Vector2Int shrineSize = new Vector2Int(5, 5);
    public float tileSize = 0.5f;

    private NetworkVariable<int> networkSeed = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            // Generate and sync seed
            int generatedSeed = Random.Range(-10000, 10000);
            networkSeed.Value = generatedSeed;
            InitializeWorld(generatedSeed);
        }
        else
        {
            // Use seed from server
            networkSeed.OnValueChanged += (oldSeed, newSeed) =>
            {
                InitializeWorld(newSeed);
            };
        }
    }

    private void InitializeWorld(int seed)
    {
        Random.InitState(seed);
        GenerateNoiseTexture();
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                bool isTerrainPlaced = false;

                if (noiseTexture.GetPixel(x, y).r < 0.5f)
                {
                    if (Random.value < oreSpawnChance)
                    {
                        GenerateOreCluster(orePrefab, x, y);
                        isTerrainPlaced = true;
                    }
                    else
                    {
                        SpawnTile(stonePrefabs[Random.Range(0, stonePrefabs.Length)], x, y);
                        isTerrainPlaced = true;
                    }
                }

                if (!isTerrainPlaced)
                {
                    SpawnTile(backgroundPrefab, x, y);
                }
            }
        }

        // Add decor after terrain generation
        AddFloorDecor();
        AddGroundDecor();
        AddCeilingDecor();
    }

    private void GenerateOreCluster(GameObject orePrefab, int startX, int startY)
    {
        int clusterSize = Random.Range(1, oreClusterSize + 1);
        SpawnTile(orePrefab, startX, startY);

        for (int i = 0; i < clusterSize - 1; i++)
        {
            int offsetX = Random.Range(-1, 2);
            int offsetY = Random.Range(-1, 2);

            int x = startX + offsetX;
            int y = startY + offsetY;

            if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
            {
                SpawnTile(orePrefab, x, y);
            }
        }
    }

    private void AddFloorDecor()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                GameObject floorBlock = GetTileAtPosition(x, y, false);
                if (floorBlock != null && GetTileAtPosition(x, y + 1, false) == null)
                {
                    if (Random.value < floorDecorChance)
                    {
                        SpawnDecor(floorDecorPrefabs, floorBlock, x, y);
                    }
                }
            }
        }
    }
    private void AddCeilingDecor()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                GameObject floorBlock = GetTileAtPosition(x, y, false);
                if (floorBlock != null && GetTileAtPosition(x, y - 1, false) == null)
                {
                    if (Random.value < floorDecorChance)
                    {
                        SpawnDecor(ceilingDecorPrefabs, floorBlock, x, y - 1);
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
                GameObject groundBlock = GetTileAtPosition(x, y, false);
                if (groundBlock != null && GetTileAtPosition(x, y + 1, false) == null)
                {
                    if (Random.value < floorDecorChance)
                    {
                        SpawnDecor(groundDecorPrefabs, groundBlock, x, y + 1);
                    }
                }
            }
        }
    }


    public void SpawnTile(GameObject prefab, int x, int y)
    {
        if (!IsServer) return; // Only the server spawns tiles

        Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);
        GameObject tile = Instantiate(prefab, position, Quaternion.identity);
        tile.GetComponent<NetworkObject>().Spawn(); // Spawn on the network
        tile.transform.SetParent(this.transform);
    }

    private void SpawnDecor(GameObject[] decorPrefabs, GameObject parentBlock, int x, int y)
    {
        if (!IsServer) return;

        Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);
        GameObject decor = Instantiate(decorPrefabs[Random.Range(0, decorPrefabs.Length)], position, Quaternion.identity);
        decor.GetComponent<NetworkObject>().Spawn(); // Spawn decor on the network
        decor.transform.SetParent(parentBlock.transform);
    }

    public GameObject GetTileAtPosition(int x, int y, bool bg)
    {
        Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);
        Collider2D[] colliders = Physics2D.OverlapPointAll(position);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("DestructibleTile"))
            {
                if (!bg)
                {
                    if (collider.gameObject.GetComponent<DestructibleTile>() != null)
                    {
                        return collider.gameObject;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (collider.gameObject.GetComponent<TileNode>() != null)
                    {
                        return collider.gameObject;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        return null;
    }

    private void GenerateNoiseTexture()
    {
        noiseTexture = new Texture2D(worldSize, worldSize);

        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + networkSeed.Value) * noiseFreq, (y + networkSeed.Value) * noiseFreq);
                noiseTexture.SetPixel(x, y, new Color(v, v, v));
            }
        }

        noiseTexture.Apply();
    }
}
