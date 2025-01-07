using UnityEngine;
using Unity.Netcode;

public class CameraFollow : NetworkBehaviour
{
    public Camera playerCamera; // The camera that follows the player
    private Vector3 offset;     // Offset distance from the player

    // Called when the player is spawned
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Get the camera attached to this player prefab (child camera)
        playerCamera = GetComponentInChildren<Camera>();

        // Disable all cameras in the scene
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (var camera in allCameras)
        {
            camera.enabled = false; // Disable all cameras
        }

        // Enable the camera for the local player
        playerCamera.enabled = true;

        // Set the camera to follow the player and store the offset
        playerCamera.transform.SetParent(transform); // Attach camera to the player
        playerCamera.transform.localPosition = new Vector3(0f, 0f, -10f);  // Adjust based on your desired view
        offset = playerCamera.transform.position - transform.position;  // Store the offset
    }

    // Update is called every frame to update camera position
    void Update()
    {
        ChangeCameraClientRpc();
    }

    [ClientRpc]
    public void ChangeCameraClientRpc()
    {
        if (playerCamera != null)
        {
            playerCamera = GetComponentInChildren<Camera>();

            // Disable all cameras in the scene
            Camera[] allCameras = FindObjectsOfType<Camera>();
            foreach (var camera in allCameras)
            {
                camera.enabled = false; // Disable all cameras
            }

            // Enable the camera for the local player
            playerCamera.enabled = true;

            // Update the camera's position relative to the player, maintaining the offset
            playerCamera.transform.position = transform.position + offset;

            // Optionally reset rotation to prevent unwanted rotation changes (e.g., if other systems affect it)
            playerCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // Ensures camera does not rotate
        }
    }
}
