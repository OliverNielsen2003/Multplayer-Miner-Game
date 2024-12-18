using UnityEngine;
using Unity.Netcode;

public class CameraFollow : NetworkBehaviour
{
    public Transform playerTransform;   // The player's transform (to follow their position)
    public Camera playerCamera;         // The camera that follows the player
    private Vector3 offset;             // Offset distance from the player

    // Called when the player is spawned
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsLocalPlayer) // Only the local player should have control over the camera
        {
            // Ensure we assign the correct camera to this player
            playerCamera = GetComponentInChildren<Camera>(); // Assumes the camera is a child of the player

            // Disable the camera for other players, but keep this player's camera active
            Camera[] allCameras = FindObjectsOfType<Camera>();
            foreach (var camera in allCameras)
            {
                camera.enabled = false; // Disable all cameras
            }

            playerCamera.enabled = true; // Enable this player's camera

            // Parent the camera to the player and store the offset
            playerCamera.transform.SetParent(playerTransform);
            playerCamera.transform.localPosition = new Vector3(0f, 0f, -10f);  // Adjust based on your desired view
            offset = playerCamera.transform.position - playerTransform.position;  // Store the offset
        }
    }

    // Update is called every frame to update camera position
    void Update()
    {
        if (IsLocalPlayer)
        {
            // Keep the camera at the same position relative to the player, but ignore rotation
            playerCamera.transform.position = playerTransform.position + offset;

            // Optionally, you can reset the rotation on every frame in case it gets altered
            playerCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
