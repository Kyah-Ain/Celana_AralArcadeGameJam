using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region === Public Config ===

    [Header("MOVEMENT")]
    public float moveSpeed = 8f; // Normal movement speed
    public float crouchMoveSpeed = 4f; // Movement speed when crouching
    public float acceleration = 15f; // Acceleration when changing speed
    public float deceleration = 20f; // Deceleration when stopping
    public float airControlMultiplier = 0.5f; // Control in the air is reduced
    public bool canMove = true; // Toggle to lock/unlock player movement

    [Header("JUMPING")]
    public float jumpForce = 12f; // Upward force when jumping
    public float coyoteTime = 0.1f; // Grace period after falling off a ledge
    public float maxFallSpeed = -20f; // Clamp downward velocity

    [Header("GROUNDCHECK")]
    public Transform groundCheck; // Point to check if grounded
    public LayerMask groundLayer; // What counts as ground
    public float groundCheckRadius = 0.1f; // Radius of the ground check

    [Header("CEILING CHECK")]
    public Transform ceilingCheck; // Point to check for ceiling when standing
    public float ceilingCheckRadius = 0.1f;

    [Header("COLLIDER SETTINGS")]
    public Collider2D standingCollider; // Collider used when standing
    public Collider2D crouchingCollider; // Collider used when crouching

    [Header("PUSH CHECK")]
    public Transform pushCheck; // Position for checking pushable objects
    public float pushCheckRadius = 0.2f;
    public LayerMask pushableLayer; // Layer of pushable objects

    [Header("KEY ITEMS")]
    public bool hasParasol = false; // Whether player has parasol
    public GameObject parasolEffect; // Effect when parasol is picked up
    public bool hasCane = false; // Whether player has cane
    public GameObject caneEffect; // Effect when cane is picked up
    public bool caneActive = false; // Whether cane is currently in use

    [Header("INVENTORY")]
    public Inventory playerInventory; // Player's inventory component

    [Header("AUDIO")]
    public float footstepSpeed = 0.5f; // Delay between footstep sounds

    [Header("Death")]
    public GameObject deathEffect; // Assign in Inspector
    public Transform respawnPoint; // Drag your empty Respawn Point here
    private SpriteRenderer spriteRenderer;

    #endregion

    #region === Private State ===

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool isCrouching;
    private bool isFacingRight = true;
    private bool isPushing = false;
    private bool parasolActive = false; // Whether parasol is currently in use
    
    private float coyoteTimeCounter; // Timer for coyote time
    private float moveInput;
    private bool canStand = true; // Used to check if standing up is possible
    private float originalPushCheckX; // For flipping push check with sprite
    private float defaultMoveSpeed; // For restoring after crouch
    private bool playingFootsteps = false; // Footstep sound toggle

    #endregion

    #region === Unity Methods ===

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        originalPushCheckX = pushCheck.localPosition.x;
        defaultMoveSpeed = moveSpeed;

        if (playerInventory == null)
            playerInventory = GetComponent<Inventory>(); // Auto-assign if missing
    }

    private void Update()
    {
        if (canMove)
        {
            moveInput = Input.GetAxisRaw("Horizontal");

            // Toggle crouch and uncrouch with 'S'
            if (Input.GetKeyDown(KeyCode.S) && !isCrouching && canStand)
                Crouch();
            else if (Input.GetKeyDown(KeyCode.S) && isCrouching && !IsCeilingAbove())
                Stand();

            // Jump with spacebar if within coyote time window
            if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                coyoteTimeCounter = 0f;
                SoundEffectManager.Play("Jump"); // Play jump sound
            }

            // Clamp falling speed
            if (rb.velocity.y < maxFallSpeed)
                rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);

            // Toggle parasol with 'F' if player has it
            if (hasParasol && Input.GetKeyDown(KeyCode.F))
            {
                parasolActive = !parasolActive;
                if (caneActive && parasolActive)
                {
                    caneActive = false;
                }
                Debug.Log("Parasol " + (parasolActive ? "equipped" : "unequipped"));
            }

            if (hasCane && Input.GetKeyDown(KeyCode.G))
            {
                caneActive = !caneActive;
                if (caneActive && parasolActive)
                {
                    parasolActive = false;
                }
                Debug.Log("Cane " + (caneActive ? "equipped" : "unequipped"));
            }

            // Slow falling with parasol if active
            if (hasParasol && parasolActive && rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, -5f);
            }
        }

        // Update animation states
        animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isJumping", !isGrounded);
        animator.SetBool("isCrouching", isCrouching);
        animator.SetBool("parasolActive", parasolActive);

        CheckForPushable(); // Handle pushing logic

        FlipSprite(moveInput); // Flip sprite based on direction
    }

    private void FixedUpdate()
    {
        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime; // Reset coyote time on ground
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        // Smooth horizontal movement with acceleration/deceleration
        float targetSpeed = moveInput * moveSpeed;

        if (canMove)
        {
            if (isGrounded || Mathf.Abs(moveInput) > 0.01f)
            {
                float speedDiff = targetSpeed - rb.velocity.x;
                float accelRate = (Mathf.Abs(targetSpeed) > 0.01f)
                    ? (isGrounded ? acceleration : acceleration * airControlMultiplier)
                    : (isGrounded ? deceleration : deceleration * airControlMultiplier);

                float movement = accelRate * speedDiff;
                rb.AddForce(Vector2.right * movement);
            }
        }
    }

    #endregion

    #region === Crouch/Stand ===

    private void Crouch()
    {
        isCrouching = true;
        moveSpeed = crouchMoveSpeed;
        standingCollider.enabled = false;
        crouchingCollider.enabled = true;
        ceilingCheck.gameObject.SetActive(true); // Enable ceiling check
    }

    private void Stand()
    {
        if (!IsCeilingAbove())
        {
            isCrouching = false;
            moveSpeed = defaultMoveSpeed;
            standingCollider.enabled = true;
            crouchingCollider.enabled = false;
            ceilingCheck.gameObject.SetActive(false);
        }
    }

    private bool IsCeilingAbove()
    {
        return Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayer);
    }

    #endregion

    #region === Pushables & Footsteps ===

    private void CheckForPushable()
    {
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            Collider2D pushable = Physics2D.OverlapCircle(pushCheck.position, pushCheckRadius, pushableLayer);

            if (pushable != null && pushable.CompareTag("Pushable"))
            {
                isPushing = true;
                animator.SetBool("isPushing", true);
            }
            else
            {
                isPushing = false;
                animator.SetBool("isPushing", false);
            }

            if (!playingFootsteps)
                startFootsteps(); // Begin footstep sounds
        }
        else
        {
            isPushing = false;
            animator.SetBool("isPushing", false);
            stopFootsteps(); // Stop footstep sounds
        }
    }

    private void startFootsteps()
    {
        playingFootsteps = true;
        InvokeRepeating(nameof(playFootsteps), 0f, footstepSpeed);
    }

    private void stopFootsteps()
    {
        playingFootsteps = false;
        CancelInvoke(nameof(playFootsteps));
    }

    private void playFootsteps()
    {
        Debug.Log("Playing Footstep Sound");
        SoundEffectManager.Play("Footsteps");
    }

    #endregion

    #region === Inventory & Items ===

    public void PickupItem(ItemPickup item)
    {
        playerInventory.AddItem(item);
        Debug.Log(item.itemName + " picked up!");

        if (item.itemName == "Parasol")
        {
            hasParasol = true;

            if (parasolEffect != null)
                Instantiate(parasolEffect, transform.position, Quaternion.identity);
        }

        if (item.itemName == "Cane")
        {
            hasCane = true;

            if (caneEffect != null)
                Instantiate(caneEffect, transform.position, Quaternion.identity);
        }
    }

    public void EquipParasol()
    {
        hasParasol = true;
        Debug.Log("Parasol equipped!");
    }

    public void EquipCane()
    {
        hasCane = true;
        Debug.Log("Cane equipped!");
    }

    #endregion

    #region === Sprite & Gizmos ===

    private void FlipSprite(float moveInput)
    {
        // Flip sprite and push check when changing direction
        if ((isFacingRight && moveInput < 0f) || (!isFacingRight && moveInput > 0f))
        {
            isFacingRight = !isFacingRight;
            GetComponent<SpriteRenderer>().flipX = !isFacingRight;

            Vector3 pushPos = pushCheck.localPosition;
            pushPos.x = Mathf.Abs(originalPushCheckX) * (isFacingRight ? 1f : -1f);
            pushCheck.localPosition = pushPos;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show debug gizmos for ground, ceiling, and push checks
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (ceilingCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
        }

        if (pushCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pushCheck.position, pushCheckRadius);
        }
    }

    public void DieAndRespawn()
    {
        StartCoroutine(HandleDeath());
    }

    private IEnumerator HandleDeath()
    {
        canMove = false;
        rb.velocity = Vector2.zero;
        spriteRenderer.enabled = false; // Hide player

        if (deathEffect != null)
        {
            deathEffect.SetActive(true);
        }

        yield return new WaitForSeconds(1f); // Wait for effect to play

        Respawn();

        if (deathEffect != null)
        {
            deathEffect.SetActive(false);
        }

        spriteRenderer.enabled = true; // Show player again
        canMove = true;
    }

    public void Respawn()
{
    rb.velocity = Vector2.zero; // Reset velocity to avoid continued momentum
    transform.position = respawnPoint.position; // Move player to respawn point
}

    #endregion
}
