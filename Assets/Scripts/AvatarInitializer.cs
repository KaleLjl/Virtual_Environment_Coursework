using UnityEngine;

[RequireComponent(typeof(Transform))]
public class AvatarInitializer : MonoBehaviour
{
    [SerializeField] private GameObject modelPrefab;
    [SerializeField] private GameObject targetPrefab;

    private Transform sourceTrans;
    private GameObject target;
    private Transform[] hips;

    public Transform SourceTrans => sourceTrans;
    public GameObject Target => target;
    public Transform[] Hips => hips;

    private void Awake()
    {
        // Ensure we have the required components
        if (modelPrefab == null || targetPrefab == null)
        {
            Debug.LogWarning("Model or Target prefab not assigned in AvatarInitializer!");
        }
    }

    public void InitializeAvatar()
    {
        if (modelPrefab == null || targetPrefab == null)
        {
            Debug.LogError("Please assign the model and target prefabs in the Unity Inspector!");
            return;
        }

        // Initialize the resource model (hidden)
        GameObject go = Instantiate(modelPrefab);
        sourceTrans = go.transform;
        go.SetActive(false);

        // Initialize the target avatar with physics
        target = Instantiate(targetPrefab);
        SetupPhysics();
        
        // Get all bone transforms for the avatar
        hips = target.GetComponentsInChildren<Transform>();
    }

    private void SetupPhysics()
    {
        target.transform.position = new Vector3(2, 1.5f, 2);
        target.transform.rotation = Quaternion.identity;
        target.transform.localScale = new Vector3(1, 1, 1);
        
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = target.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.mass = 70f;
        rb.linearDamping = 1f;
        rb.angularDamping = 0.05f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                        RigidbodyConstraints.FreezeRotationY | 
                        RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
} 