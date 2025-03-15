using UnityEngine;

public class SyncCharacterToXR : MonoBehaviour
{
    public Transform xrOrigin;  // XR Rig (VR Player)
    public Transform character; // Character model
    public LayerMask groundLayer; // Layer for the ground
    public float smoothSpeed = 10f; // Adjust for smoother movement

    private void Update()
    {
        // Get the desired position (sync with XR Origin)
        Vector3 targetPosition = xrOrigin.position;

        // Raycast down to find the ground position
        if (Physics.Raycast(character.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f, groundLayer))
        {
            targetPosition.y = hit.point.y; // Snap to ground
        }

        // Smoothly interpolate to the new position to avoid jitter
        character.position = Vector3.Lerp(character.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}