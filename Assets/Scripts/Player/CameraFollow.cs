using UnityEngine;
using Unity.Netcode;

public class CameraFollow : NetworkBehaviour
{
    public Vector3 offset;
    public GameObject Camera;

    private void Start()
    {
        if (IsLocalPlayer) return;

        Camera.GetComponent<Camera>().enabled = false;
        GetComponent<PlayerController>().enabled = false;
        GetComponent<Animator>().enabled = false;
    }
    void Update()
    {
        Camera.transform.position = transform.position + offset;
        Camera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
}
