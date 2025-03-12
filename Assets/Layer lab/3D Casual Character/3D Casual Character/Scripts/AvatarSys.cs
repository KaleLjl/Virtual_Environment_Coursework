using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSys : MonoBehaviour {

    public static AvatarSys _instance;

    // Assign these in the Unity Inspector
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
    // Default parts: { { Category, Variant } } – ensure these match your folder and part naming conventions.
    private string[,] avatarStr = new string[,] { {"Hair", "1"}, {"Top", "1"}, {"Bottom", "1"}, {"Shoes", "1"}, {"Face", "A1"} };

    void Awake() {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        InstantiateAvatar();
        SaveData(sourceTrans, data, target, smr);
        InitAvatar();
    }

    // Instantiate the model and target from assigned prefabs
    void InstantiateAvatar() {
        if (modelPrefab == null || targetPrefab == null) {
            Debug.LogError("Please assign the model and target prefabs in the Unity Inspector!");
            return;
        }

        GameObject go = Instantiate(modelPrefab);
        sourceTrans = go.transform;
        go.SetActive(false);

        target = Instantiate(targetPrefab);
        hips = target.GetComponentsInChildren<Transform>();
    }

    // Collects part data from sourceTrans/Parts and creates corresponding empty target parts.
    void SaveData(Transform sourceTrans, Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data, GameObject target, Dictionary<string, SkinnedMeshRenderer> smr) {
        data.Clear();
        smr.Clear();

        if (sourceTrans == null)
            return;

        // Look for the "Parts" folder in the resource model.
        Transform partsParent = sourceTrans.Find("Parts");
        if (partsParent == null) {
            Debug.LogError("No 'Parts' folder found in source model!");
            return;
        }

        // Iterate through each category (e.g., Hair, Top, etc.)
        foreach (Transform category in partsParent) {
            if (!data.ContainsKey(category.name))
                data.Add(category.name, new Dictionary<string, SkinnedMeshRenderer>());

            // Find (or create) the corresponding category folder in the target.
            Transform targetPartsParent = target.transform.Find("Parts");
            if (targetPartsParent == null) {
                Debug.LogError("No 'Parts' folder found in target model!");
                continue;
            }
            Transform targetCategory = targetPartsParent.Find(category.name);
            if (targetCategory == null) {
                GameObject newCategory = new GameObject(category.name);
                newCategory.transform.parent = targetPartsParent;
                targetCategory = newCategory.transform;
            }

            // For each part in the category (e.g., Hair_1, Hair_2, etc.)
            foreach (Transform part in category) {
                SkinnedMeshRenderer partSMR = part.GetComponent<SkinnedMeshRenderer>();
                if (partSMR != null) {
                    // Key name should match: e.g., "Hair_1"
                    data[category.name].Add(part.name, partSMR);
                    Debug.Log("Collected part: " + part.name + " under category: " + category.name);
                }
            }

            // Create an empty SkinnedMeshRenderer on the target for this category if not already created.
            if (!smr.ContainsKey(category.name)) {
                GameObject partGo = new GameObject(category.name);
                partGo.transform.parent = targetCategory;
                smr.Add(category.name, partGo.AddComponent<SkinnedMeshRenderer>());
            }
        }
    }

    // Changes the target's mesh for a given category (part) using the variant number.
    // 'part' is the category (e.g., "Hair") and 'num' is the variant (e.g., "1" for "Hair_1").
    void ChangeMesh(string part, string num, Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data, Transform[] hips, Dictionary<string, SkinnedMeshRenderer> smr, string[,] str) {
        string fullPartName = part + "_" + num;

        if (!data.ContainsKey(part) || !data[part].ContainsKey(fullPartName)) {
            Debug.LogError("No part found with key " + fullPartName);
            return;
        }

        SkinnedMeshRenderer skm = data[part][fullPartName];

        // Remap bones from the resource part to the target's bones.
        List<Transform> bones = new List<Transform>();
        foreach (var bone in skm.bones) {
            foreach (var targetBone in hips) {
                if (targetBone.name == bone.name) {
                    bones.Add(targetBone);
                    break;
                }
            }
        }
        // Replace the target's part with the new mesh, material, and bones.
        smr[part].bones = bones.ToArray();
        smr[part].materials = skm.materials;
        smr[part].sharedMesh = skm.sharedMesh;

        SaveDataForPart(part, num, str);
    }

    // Update the record for the given part in the string array.
    void SaveDataForPart(string part, string num, string[,] str) {
        int length = str.GetLength(0);
        for (int i = 0; i < length; i++) {
            if (str[i, 0] == part) {
                str[i, 1] = num;
            }
        }
    }

    // Initialize the avatar with default parts.
    void InitAvatar() {
        int length = avatarStr.GetLength(0);
        for (int i = 0; i < length; i++) {
            ChangeMesh(avatarStr[i, 0], avatarStr[i, 1], data, hips, smr, avatarStr);
        }
    }

    // Public method to change a part on the fly (e.g., can be hooked up to UI buttons)
    public void OnChangePart(string part, string num) {
        ChangeMesh(part, num, data, hips, smr, avatarStr);
    }
}