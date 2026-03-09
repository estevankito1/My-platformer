using UnityEngine;

public class DeathZone : MonoBehaviour
{
    // Mata al player instantáneamente al tocar esta zona
    // Ponlo debajo del nivel como un trigger invisible

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Matar instantáneamente ignorando invencibilidad
        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null)
            pm.TakeDamage(99999f);
    }
}










