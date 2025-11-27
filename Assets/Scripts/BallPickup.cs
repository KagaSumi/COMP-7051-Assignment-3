using UnityEngine;

public class BallPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object colliding is the player
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            // Tell the player they now have the ball
            player.EquipBall();

            // Destroy this pickup object so it disappears from the ground
            Destroy(gameObject);
        }
    }
}