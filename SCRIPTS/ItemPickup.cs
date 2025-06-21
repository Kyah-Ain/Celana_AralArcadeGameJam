using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string itemName = "Parasol";  // Name of the item (defaults to Parasol)
    public Sprite itemSprite;            // Assign the sprite here (in the inspector)
    public GameObject pickupEffect;      // Optional effect when picked up

    private void Start()
    {
        // Set the sprite for the parasol in case it's not set already
        if (GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().sprite = itemSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the player collides with the parasol
        if (other.CompareTag("Player"))
        {
            // Call the PickupItem method in the Player script
            Player playerScript = other.GetComponent<Player>();
            playerScript.PickupItem(this);

            // Optional: Trigger a pickup effect (e.g., particle system)
            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            // Destroy the parasol after it is picked up
            Destroy(gameObject);
        }
    }
}
