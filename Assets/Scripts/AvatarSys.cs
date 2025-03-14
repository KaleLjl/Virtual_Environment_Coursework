using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSys : MonoBehaviour
{
    public static AvatarSys _instance;

    // Assign these in the Unity Inspector or load from Resources
    [SerializeField] private GameObject modelPrefab;    // The resource model prefab (e.g., AvatarModel)
    [SerializeField] private GameObject targetPrefab;   // The target avatar prefab (e.g., AvatarTarget)

    // Single avatar variables
    private Transform sourceTrans;       // The instantiated resource model
    private GameObject target;           // The instantiated target avatar
    // Data dictionary: key = category (e.g., "Hair"), value = dictionary mapping full part name (e.g., "Hair_1") to its SkinnedMeshRenderer.
    private Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data = new Dictionary<string, Dictionary<string, SkinnedMeshRenderer>>();
    private Transform[] hips;            // Target avatar's bone transforms
    // Dictionary for target's parts – one per category – used to swap meshes, materials, and bones.
    private Dictionary<string, SkinnedMeshRenderer> smr = new Dictionary<string, SkinnedMeshRenderer>();
    
    // Categories for avatar parts - adjusted to match your project structure
    private readonly string[] categories = new string[] 
    { 
        "Body",
        "Bottom",
        "Top",
        "Bag",
        "Headgear",
        "Shoes", 
        "Glove",
        "Eyewear",
        "Face",
        "Hair"
    };

    // Default parts: { { Category, Variant } } – ensure these match your folder and part naming conventions.
    private string[,] avatarStr;

    // Special case categories that use different naming conventions
    private readonly Dictionary<string, string> specialFormats = new Dictionary<string, string>() 
    {
        { "Face", "A1" }  // Default Face part uses A1 format instead of just 1
    };

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // If not assigned in inspector, load from Resources
        if (modelPrefab == null)
            modelPrefab = Resources.Load<GameObject>("Model");
        if (targetPrefab == null)
            targetPrefab = Resources.Load<GameObject>("Target");
            
        if (modelPrefab == null || targetPrefab == null)
        {
            Debug.LogError("Failed to load prefabs from Resources folder or Inspector. Make sure Model.prefab and Target.prefab are assigned!");
        }
        
        // Initialize the default parts array based on categories
        avatarStr = new string[categories.Length, 2];
        for (int i = 0; i < categories.Length; i++)
        {
            avatarStr[i, 0] = categories[i];
            
            // Check if this category has a special naming format
            if (specialFormats.ContainsKey(categories[i]))
            {
                avatarStr[i, 1] = specialFormats[categories[i]];
            }
            else
            {
                avatarStr[i, 1] = "1";  // Default to variant 1
            }
        }
    }

    void Start()
    {
        InstantiateAvatar();
        SaveData(sourceTrans, data, target, smr);
        InitAvatar();
    }

    // Instantiate the model and target from assigned prefabs
    void InstantiateAvatar()
    {
        if (modelPrefab == null || targetPrefab == null)
        {
            Debug.LogError("Please assign the model and target prefabs in the Unity Inspector or ensure they're in Resources!");
            return;
        }

        GameObject go = Instantiate(modelPrefab);
        sourceTrans = go.transform;
        go.SetActive(false);

        target = Instantiate(targetPrefab);
        hips = target.GetComponentsInChildren<Transform>();
    }

    // Collects part data from sourceTrans/Parts and creates corresponding empty target parts.
    void SaveData(Transform sourceTrans, Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data, GameObject target, Dictionary<string, SkinnedMeshRenderer> smr)
    {
        data.Clear();
        smr.Clear();

        if (sourceTrans == null)
            return;

        // Look for the "Parts" folder in the resource model.
        Transform partsParent = sourceTrans.Find("Parts");
        
        // If no Parts folder is found, use the root model as parent
        if (partsParent == null)
        {
            partsParent = sourceTrans;
            Debug.Log("No 'Parts' folder found in source model, using model root instead");
        }

        // Create Parts folder in target if it doesn't exist
        Transform targetPartsParent = target.transform.Find("Parts");
        if (targetPartsParent == null)
        {
            GameObject partsGo = new GameObject("Parts");
            partsGo.transform.parent = target.transform;
            targetPartsParent = partsGo.transform;
            Debug.Log("Created 'Parts' folder in target model");
        }

        // Collect all potential parts by recursively searching the model
        foreach (string category in categories)
        {
            if (!data.ContainsKey(category))
                data.Add(category, new Dictionary<string, SkinnedMeshRenderer>());

            // Find (or create) the corresponding category folder in the target
            Transform targetCategory = targetPartsParent.Find(category);
            if (targetCategory == null)
            {
                GameObject newCategory = new GameObject(category);
                newCategory.transform.parent = targetPartsParent;
                targetCategory = newCategory.transform;
                Debug.Log("Created category folder: " + category);
            }

            // Find all parts in the model that match this category
            CollectPartsForCategory(sourceTrans, category, data);

            // Create an empty SkinnedMeshRenderer on the target for this category if not already created
            if (!smr.ContainsKey(category))
            {
                GameObject partGo = new GameObject(category);
                partGo.transform.parent = targetCategory;
                smr.Add(category, partGo.AddComponent<SkinnedMeshRenderer>());
                Debug.Log("Created SkinnedMeshRenderer for category: " + category);
            }
        }
    }

    // Recursively searches for parts that match the given category
    void CollectPartsForCategory(Transform parent, string category, Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data)
    {
        foreach (Transform child in parent)
        {
            // Check if this is a part that belongs to the category
            if (child.name.StartsWith(category + "_"))
            {
                SkinnedMeshRenderer partSMR = child.GetComponent<SkinnedMeshRenderer>();
                if (partSMR != null)
                {
                    // Key name should match: e.g., "Hair_1" or "Face_A1"
                    data[category].Add(child.name, partSMR);
                    Debug.Log("Collected part: " + child.name + " under category: " + category);
                }
            }

            // Recursively search in children
            CollectPartsForCategory(child, category, data);
        }
    }

    // Changes the target's mesh for a given category (part) using the variant number.
    // 'part' is the category (e.g., "Hair") and 'num' is the variant (e.g., "1" for "Hair_1", or "A1" for "Face_A1").
    void ChangeMesh(string part, string num, Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data, Transform[] hips, Dictionary<string, SkinnedMeshRenderer> smr, string[,] str)
    {
        string fullPartName = part + "_" + num;

        // If the exact part name isn't found, try to find a similar one in that category
        if (!data.ContainsKey(part) || !data[part].ContainsKey(fullPartName))
        {
            Debug.LogWarning("No exact match for part " + fullPartName + ", checking for alternatives");
            
            if (data.ContainsKey(part))
            {
                var alternativePartNames = new List<string>(data[part].Keys);
                if (alternativePartNames.Count > 0)
                {
                    // Try to find the closest match based on prefix
                    string bestMatch = null;
                    foreach (var partName in alternativePartNames)
                    {
                        if (bestMatch == null || 
                            (partName.StartsWith(part + "_" + num.Substring(0, 1)) && 
                             partName.Length < bestMatch.Length))
                        {
                            bestMatch = partName;
                        }
                    }
                    
                    if (bestMatch != null)
                    {
                        Debug.Log("Found alternative part: " + bestMatch + " instead of " + fullPartName);
                        fullPartName = bestMatch;
                    }
                    else
                    {
                        Debug.LogError("No part found with key " + fullPartName + " and no suitable alternatives");
                        return;
                    }
                }
                else
                {
                    Debug.LogError("Category " + part + " exists but has no parts");
                    return;
                }
            }
            else
            {
                Debug.LogError("Category " + part + " doesn't exist in data dictionary");
                return;
            }
        }

        SkinnedMeshRenderer skm = data[part][fullPartName];

        // Remap bones from the resource part to the target's bones.
        List<Transform> bones = new List<Transform>();
        foreach (var bone in skm.bones)
        {
            foreach (var targetBone in hips)
            {
                if (targetBone.name == bone.name)
                {
                    bones.Add(targetBone);
                    break;
                }
            }
        }
        
        // Replace the target's part with the new mesh, material, and bones.
        smr[part].bones = bones.ToArray();
        smr[part].materials = skm.materials;
        smr[part].sharedMesh = skm.sharedMesh;
        smr[part].rootBone = FindMatchingBone(skm.rootBone, hips);

        // Extract the variant code (everything after the underscore)
        string variantCode = fullPartName.Substring(part.Length + 1);
        SaveDataForPart(part, variantCode, str);
    }

    // Helper method to find matching bone by name
    private Transform FindMatchingBone(Transform sourceBone, Transform[] targetBones)
    {
        if (sourceBone == null) return null;
        
        foreach (var bone in targetBones)
        {
            if (bone.name == sourceBone.name)
                return bone;
        }
        return null;
    }

    // Update the record for the given part in the string array.
    void SaveDataForPart(string part, string num, string[,] str)
    {
        int length = str.GetLength(0);
        for (int i = 0; i < length; i++)
        {
            if (str[i, 0] == part)
            {
                str[i, 1] = num;
            }
        }
    }

    // Initialize the avatar with default parts.
    void InitAvatar()
    {
        int length = avatarStr.GetLength(0);
        for (int i = 0; i < length; i++)
        {
            // Only try to change parts that exist in the data dictionary
            if (data.ContainsKey(avatarStr[i, 0]))
            {
                ChangeMesh(avatarStr[i, 0], avatarStr[i, 1], data, hips, smr, avatarStr);
            }
        }
    }

    // Public method to change a part on the fly (e.g., can be hooked up to UI buttons)
    public void OnChangePart(string part, string num)
    {
        ChangeMesh(part, num, data, hips, smr, avatarStr);
    }

    // Helper method to get current active parts
    public Dictionary<string, string> GetCurrentParts()
    {
        Dictionary<string, string> currentParts = new Dictionary<string, string>();
        int length = avatarStr.GetLength(0);
        
        for (int i = 0; i < length; i++)
        {
            currentParts.Add(avatarStr[i, 0], avatarStr[i, 1]);
        }
        
        return currentParts;
    }
}