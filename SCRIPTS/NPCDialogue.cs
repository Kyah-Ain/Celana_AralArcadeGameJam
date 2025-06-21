using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup interactPromptGroup;     // "Press E to interact" prompt
    public CanvasGroup dialogueCanvasGroup;     // Dialogue panel
    public TextMeshProUGUI dialogueText;        // Text component inside the dialogue panel

    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    public string[] dialogueLines;              // Dialogue lines for the NPC
    public float fadeDuration = 0.3f;           // Duration of fade in/out

    private int dialogueIndex = 0;              // Current dialogue line
    private bool playerInRange = false;         // Is player near the NPC
    private bool dialogueActive = false;        // Is dialogue currently active

    private Coroutine promptFadeCoroutine;      // Fade coroutine for prompt
    private Coroutine dialogueFadeCoroutine;    // Fade coroutine for dialogue

    private Player playerController;            // Reference to the player's movement script

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!dialogueActive)
            {
                StartDialogue();
            }
            else
            {
                AdvanceDialogue();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // Show the interact prompt
            if (interactPromptGroup != null)
                ShowCanvasGroup(interactPromptGroup, ref promptFadeCoroutine);

            // Cache reference to player
            if (playerController == null)
                playerController = other.GetComponent<Player>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            dialogueActive = false;
            dialogueIndex = 0;

            // Hide both UI elements
            if (dialogueCanvasGroup != null)
                HideCanvasGroup(dialogueCanvasGroup, ref dialogueFadeCoroutine);

            if (interactPromptGroup != null)
                HideCanvasGroup(interactPromptGroup, ref promptFadeCoroutine);

            // Re-enable movement
            if (playerController != null)
                playerController.canMove = true;
        }
    }

    private void StartDialogue()
    {
        dialogueActive = true;
        dialogueIndex = 0;

        // Hide prompt
        if (interactPromptGroup != null)
            HideCanvasGroup(interactPromptGroup, ref promptFadeCoroutine);

        // Show dialogue box and set first line
        if (dialogueCanvasGroup != null)
        {
            ShowCanvasGroup(dialogueCanvasGroup, ref dialogueFadeCoroutine);
            dialogueText.text = dialogueLines[dialogueIndex];
        }

        // Lock movement
        if (playerController != null)
            playerController.canMove = false;
    }

    private void AdvanceDialogue()
    {
        dialogueIndex++;

        if (dialogueIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[dialogueIndex];
        }
        else
        {
            // End dialogue
            dialogueActive = false;
            if (dialogueCanvasGroup != null)
                HideCanvasGroup(dialogueCanvasGroup, ref dialogueFadeCoroutine);

            if (playerController != null)
                playerController.canMove = true;

            // Optionally, show the prompt again if player stays in range
            if (playerInRange && interactPromptGroup != null)
                ShowCanvasGroup(interactPromptGroup, ref promptFadeCoroutine);
        }
    }

    private void ShowCanvasGroup(CanvasGroup cg, ref Coroutine coroutine)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(FadeCanvasGroup(cg, 0f, 1f));
    }

    private void HideCanvasGroup(CanvasGroup cg, ref Coroutine coroutine)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 0f));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        cg.alpha = startAlpha;

        if (endAlpha > 0f)
            cg.gameObject.SetActive(true);

        while (elapsed < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cg.alpha = endAlpha;
        cg.interactable = endAlpha == 1f;
        cg.blocksRaycasts = endAlpha == 1f;

        if (endAlpha == 0f)
            cg.gameObject.SetActive(false);
    }
}
