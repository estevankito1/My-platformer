using UnityEngine;

public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Muerte instantánea ignorando invencibilidad
        other.GetComponent<PlayerMovement>()?.InstantDie();
    }
}





















