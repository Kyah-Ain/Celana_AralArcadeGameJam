using UnityEngine;
using UnityEngine.SceneManagement;

public class JackInTheBox : MonoBehaviour
{
    public string requiredItem = "Key";
    public Transform launchTarget;
    public float launchForce = 20f;
    public Animator jackAnimator;
    public GameObject launchEffect;
    public float delayBeforeLaunch = 1.5f;
    public string endSceneName; // Optional: Set this in Inspector if changing scenes

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();

            if (player != null && player.playerInventory.HasItem(requiredItem))
            {
                activated = true;

                if (jackAnimator != null)
                    jackAnimator.SetTrigger("Activate");

                StartCoroutine(LaunchSequence(player));
            }
        }
    }

    private System.Collections.IEnumerator LaunchSequence(Player player)
    {
        SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();

        // Short delay before hiding
        yield return new WaitForSeconds(0.2f);
        if (playerRenderer != null) playerRenderer.enabled = false;

        // Wait for animation
        yield return new WaitForSeconds(delayBeforeLaunch);

        // Launch effect
        if (launchEffect != null)
            Instantiate(launchEffect, transform.position, Quaternion.identity);

        // Launch player
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.zero;
        player.canMove = false;

        Vector2 direction = (launchTarget.position - player.transform.position).normalized;
        rb.AddForce(direction * launchForce, ForceMode2D.Impulse);

        // Re-show player
        yield return new WaitForSeconds(0.1f);
        if (playerRenderer != null) playerRenderer.enabled = true;

        // Optional: End game or load next scene
        yield return new WaitForSeconds(1f);
        Debug.Log("Game End Triggered");

        if (!string.IsNullOrEmpty(endSceneName))
        {
            SceneManager.LoadScene(endSceneName);
        }
    }
}
