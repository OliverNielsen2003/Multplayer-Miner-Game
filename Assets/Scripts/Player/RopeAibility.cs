using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class RopeAibility : NetworkBehaviour
{
    public TerrainGeneration generator;
    public GameObject rope;
    public LayerMask LayerToSeek;
    public GameObject ActiveTile;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        generator = GameObject.FindAnyObjectByType<TerrainGeneration>();
        RaycastHit2D circleRay = Physics2D.CircleCast(transform.position, 0.1f, Vector2.zero, 0f, LayerToSeek);
        if (circleRay)
        {
            transform.position = circleRay.transform.position;
            ActiveTile = circleRay.transform.gameObject;
            FindRopeAbility(1);
        }
    }

    public void FindRopeAbility(int repeats)
    {
        for (int x = 0; x < 100; x++)
        {
            for(int y = 0; y < 100; y++)
            {
                if (ActiveTile == generator.GetTileAtPosition(x, y))
                {
                    GameObject currentBlock = generator.GetTileAtPosition(x, y + repeats);
                    if (currentBlock == null)
                    {
                        repeats++;
                        generator.SpawnTile(rope, x, y + repeats);
                        FindRopeAbility(repeats);
                    }
                }
            }
        }
    }


}
