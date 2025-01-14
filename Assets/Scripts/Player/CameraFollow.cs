using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class CameraFollow : NetworkBehaviour
{
    public Vector3 offset;
    public GameObject Camera;

    private void Start()
    {
        if (IsLocalPlayer) return;

        Camera.GetComponent<Camera>().enabled = false;
        GetComponent<PlayerController>().enabled = false;
        //transform.GetChild(4).gameObject.GetComponent<Animator>().enabled = false;
        GetComponent<PlayerInput>().enabled = false;
    }
    void Update()
    {
        Camera.transform.position = transform.position + offset;
        Camera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
}
