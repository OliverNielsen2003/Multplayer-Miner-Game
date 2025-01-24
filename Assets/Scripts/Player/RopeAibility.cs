using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using JetBrains.Annotations;

public class RopeAibility : NetworkBehaviour
{
    public TerrainGeneration generator;
    public GameObject rope;
    public LayerMask LayerToSeek;
    public GameObject ActiveTile;

    public bool placingRope;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (placingRope)
        {
            generator = GameObject.FindAnyObjectByType<TerrainGeneration>();
            RaycastHit2D circleRay = Physics2D.CircleCast(transform.position, 0.1f, Vector2.zero, 0f, LayerToSeek);
            if (circleRay && circleRay.transform.GetComponent<TileNode>() != null)
            {
                transform.position = circleRay.transform.position;
                ActiveTile = circleRay.transform.gameObject;
                FindRopeAbility(1);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerController>().isOnRope = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerController>().isOnRope = false;
        }
    }

    public void FindRopeAbility(int repeats)
    {
        for (int x = 0; x < 100; x++)
        {
            for(int y = 0; y < 100; y++)
            {
                if (ActiveTile == generator.GetTileAtPosition(x, y, true))
                {
                    GameObject currentBlock = generator.GetTileAtPosition(x, y + repeats, true);
                    if (currentBlock != null && currentBlock.GetComponent<TileNode>() != null)
                    {
                        generator.SpawnTile(rope, x, y + repeats);
                        repeats++;
                        FindRopeAbility(repeats);
                    }
                }
            }
        }
    }


}
