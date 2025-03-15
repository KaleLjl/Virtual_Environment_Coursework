using UnityEngine;

public class AlignVRCameraToHead : MonoBehaviour
{
    public Transform vrCamera;  // The XR Camera (inside XR Origin)
    public Transform characterHead;  // The character's head bone

    void Update()
    {
        if (vrCamera != null && characterHead != null)
        {
            // Match VR camera position to the character's head
            vrCamera.position = characterHead.position;
        }
    }
}
