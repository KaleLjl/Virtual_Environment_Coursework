using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarCustomizationUI : MonoBehaviour
{
    [Header("Category Settings")]
    [Tooltip("Enter the category name exactly as used in AvatarSys (e.g., \"Hair\", \"Top\", \"Bottom\", \"Shoes\", \"Face\").")]
    public string category;

    [Header("UI References")]
    [Tooltip("The parent container where buttons will be added (e.g., the Content object of a Scroll View).")]
    public Transform buttonContainer;
    [Tooltip("A prefab for a UI Button that contains a Text component to display the variant name.")]
    public Button buttonPrefab;

    IEnumerator Start()
    {
        // Wait one frame to allow AvatarSys to finish initializing.
        yield return null;

        // Query available variants from AvatarSys.
        Dictionary<string, SkinnedMeshRenderer> parts;
        if (AvatarSys._instance != null && AvatarSys._instance.GetPartsForCategory(category, out parts))
        {
            Debug.Log("Found " + parts.Count + " parts for category: " + category);
            foreach (KeyValuePair<string, SkinnedMeshRenderer> kvp in parts)
            {
                // Instantiate a new button.
                Button newButton = Instantiate(buttonPrefab, buttonContainer);

                // Set the button's GameObject name to the key.
                newButton.name = kvp.Key;

                // Get the Text component on the button and update its text.
                Text btnText = newButton.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = kvp.Key;
                }
                else
                {
                    Debug.LogWarning("Button prefab is missing a Text component.");
                }

                // Extract variant number (if needed for your OnChangePart call)
                string variantNumber = kvp.Key.Substring(category.Length + 1);
                string part = category;
                string variant = variantNumber;
                newButton.onClick.AddListener(() =>
                {
                    AvatarSys._instance.OnChangePart(part, variant);
                });
            }
        }
        else
        {
            Debug.LogWarning("No parts found for category: " + category);
        }
    }
}