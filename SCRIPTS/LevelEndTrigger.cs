using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    public GameObject levelCompletePanel;  // Assign in inspector
    public Transform respawnPoint;         // Assign the level's starting point
    private Player player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        player = other.GetComponent<Player>();
        if (player != null)
        {
            if (player.hasParasol)
            {
                levelCompletePanel.SetActive(true);
                player.canMove = false;
                Time.timeScale = 0f; // Pause the game
            }
            else
            {
                player.transform.position = respawnPoint.position;
                player.GetComponent<Rigidbody2D>().velocity = Vector2.zero; // <-- Reset velocity after teleporting
            }
        }
    }

    // UI Button Hooks
    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Use your actual main menu scene name
    }
}
