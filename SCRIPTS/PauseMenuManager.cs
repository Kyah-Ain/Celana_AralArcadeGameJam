using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuPanel; // Assign this in the Inspector

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        pauseMenuPanel.SetActive(isPaused);

        // Optional: Freeze or resume the game time
        Time.timeScale = isPaused ? 0f : 1f;

        // Optional: Disable player movement if you want during pause
        Player player = FindObjectOfType<Player>();
        if (player != null)
            player.canMove = !isPaused;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;

        Player player = FindObjectOfType<Player>();
        if (player != null)
            player.canMove = true;
    }

    public void ResetScene()
    {
        var player = FindObjectOfType<Player>();
        if (player != null)
            player.canMove = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Reset time
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // Replace with your menu scene name
    }
}
