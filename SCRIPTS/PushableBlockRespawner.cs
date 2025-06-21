using UnityEngine;

public class PushableBlockRespawner : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float respawnHeight = -10f;           // Y-position where block respawns if it falls below this
    public Transform respawnPoint;               // Where the block respawns (usually its starting position)

    private Rigidbody2D rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Store the original position and rotation
        initialPosition = respawnPoint != null ? respawnPoint.position : transform.position;
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (transform.position.y < respawnHeight)
        {
            RespawnBlock();
        }
    }

    void RespawnBlock()
    {
        // Reset position, rotation, and velocity
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}
