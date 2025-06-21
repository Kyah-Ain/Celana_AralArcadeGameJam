using System.Collections; // Grants access to collections and data structures like ArrayList
using System.Collections.Generic; // Grants access to collections and data structures like List and Dictionary
using UnityEngine; // Grants access to Unity's core features 
using UnityEngine.UI; // Allows the use of UI elements, such as Image components

public class SaveManager : MonoBehaviour
{
    // ------------------------- VARIABLES -------------------------
    
    [Header("Player Prefs Values")]
    public int levelAt = 0; // Stores the player's highest unlocked level
    public int lastLevel; // Stores the player's last played level

    [Header("UI Reference")]
    public Button btnLoad; // Reference to the Load button UI element

    [Header("Load Buttons")]
    public GameObject[] loadButtons = new GameObject[0]; // Array of level load buttons (disabled by default)

    // ------------------------- FUNCTIONS -------------------------

    // Start is called before the first frame update
    void Start()
    {
        levelAt = PlayerPrefs.GetInt("p_levelAt"); // Retrieves the highest unlocked level from saved data
        lastLevel = PlayerPrefs.GetInt("p_lastLevel"); // Retrieves the last played level from saved data
    }

    // Update is called once per frame
    void Update()
    {
        if (levelAt < 1) // If no level progress is found, disable the Load button
        {
            btnLoad.interactable = false;
        }

        for (int x = 1; x <= levelAt; x++) // Activates load buttons for unlocked levels
        {
            loadButtons[x].gameObject.SetActive(true);
        }
    }
}
