using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterMenu : MonoBehaviour
{
    public GameObject buttonPrefab; // Assign the CharacterButton prefab
    public Transform buttonContainer; // Assign the Content panel in Scroll View
    public Transform spawnPoint; // Where the character will appear
    public List<GameObject> characterPrefabs = new List<GameObject>(); // Holds all character prefabs

    private GameObject currentCharacter; // Stores the currently spawned character

    void Start()
    {
        LoadCharactersFromFolder();
        GenerateCharacterButtons();
    }

    void LoadCharactersFromFolder()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("Characters"); // Folder: Assets/Resources/Characters
        characterPrefabs.AddRange(loadedPrefabs);
    }

    void GenerateCharacterButtons()
    {
        foreach (GameObject characterPrefab in characterPrefabs)
        {
            // Create a new button
            GameObject newButton = Instantiate(buttonPrefab, buttonContainer);
            newButton.GetComponentInChildren<Text>().text = characterPrefab.name;
            newButton.GetComponent<Button>().onClick.AddListener(() => SelectCharacter(characterPrefab));
        }
    }

    public void SelectCharacter(GameObject characterPrefab)
    {
        if (currentCharacter != null)
        {
            Destroy(currentCharacter); // Remove the previous character
        }

        // Instantiate the selected character at the spawn point
        currentCharacter = Instantiate(characterPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}