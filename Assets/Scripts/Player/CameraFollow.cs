using UnityEngine;
using Unity.Netcode;

public class CameraFollow : NetworkBehaviour
{
    public GameObject playerCamera;
    public Vector3 offset;
    public GameObject Prefab;

    private void Start()
    {
        playerCamera = Instantiate(Prefab, new Vector3(transform.position.x, transform.position.y, Prefab.transform.position.z), transform.rotation);
    }

    // Update is called every frame to update camera position
    void Update()
    {
        playerCamera.transform.position = transform.position + offset;
        playerCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
}
