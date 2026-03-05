using UnityEngine;

public class Saw : MonoBehaviour
{
    public enum SawType { Static, MovingHorizontal, MovingVertical, MovingPendulum }

    [Header("Tipo")]
    public SawType sawType = SawType.MovingHorizontal;

    [Header("Rotación")]
    [SerializeField] private float rotationSpeed = 300f;

    [Header("Movimiento")]
    [SerializeField] private float moveDistance = 2.5f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Péndulo")]
    [SerializeField] private float pendulumAngle = 45f;
    [SerializeField] private float pendulumSpeed = 1.5f;
    [SerializeField] private float pendulumRadius = 2f;

    [Header("Daño")]
    [SerializeField] private float damage = 9999f;
    [SerializeField] private bool killInstantly = true;

    [Header("Visual opcional")]
    [SerializeField] private LineRenderer chain;

    private Vector2 startPosition;
    private float elapsedTime;

    // -------------------------------------------------------
    void Start()
    {
        // Guardar posición exacta en la escena — nunca se teletransporta
        startPosition = transform.position;
        elapsedTime = 0f;
    }

    // -------------------------------------------------------
    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Siempre rotar
        transform.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);

        switch (sawType)
        {
            case SawType.MovingHorizontal: DoHorizontal(); break;
            case SawType.MovingVertical: DoVertical(); break;
            case SawType.MovingPendulum: DoPendulum(); break;
                // Static: solo gira
        }

        // Cadena para péndulo
        if (sawType == SawType.MovingPendulum && chain != null)
        {
            chain.SetPosition(0, startPosition);
            chain.SetPosition(1, transform.position);
        }
    }

    // -------------------------------------------------------
    void DoHorizontal()
    {
        float t = Mathf.PingPong(elapsedTime * moveSpeed, 1f);
        float newX = Mathf.Lerp(startPosition.x - moveDistance, startPosition.x + moveDistance, t);
        transform.position = new Vector2(newX, startPosition.y);
    }

    void DoVertical()
    {
        float t = Mathf.PingPong(elapsedTime * moveSpeed, 1f);
        float newY = Mathf.Lerp(startPosition.y, startPosition.y + moveDistance, t);
        transform.position = new Vector2(startPosition.x, newY);
    }

    void DoPendulum()
    {
        float angle = Mathf.Sin(elapsedTime * pendulumSpeed) * pendulumAngle;
        float rad = angle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(
             Mathf.Sin(rad) * pendulumRadius,
            -Mathf.Cos(rad) * pendulumRadius);
        transform.position = startPosition + offset;
    }

    // -------------------------------------------------------
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        float dmg = killInstantly ? 99999f : damage;
        other.GetComponent<PlayerMovement>()?.TakeDamage(dmg, transform.position);
    }

    // -------------------------------------------------------
    // GIZMOS
    // -------------------------------------------------------
    void OnDrawGizmos()
    {
        Vector2 origin = Application.isPlaying ? startPosition : (Vector2)transform.position;

        if (sawType == SawType.MovingHorizontal)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin + Vector2.left * moveDistance, origin + Vector2.right * moveDistance);
            Gizmos.DrawWireSphere(origin + Vector2.left * moveDistance, 0.1f);
            Gizmos.DrawWireSphere(origin + Vector2.right * moveDistance, 0.1f);
        }
        else if (sawType == SawType.MovingVertical)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + Vector2.up * moveDistance);
            Gizmos.DrawWireSphere(origin, 0.1f);
            Gizmos.DrawWireSphere(origin + Vector2.up * moveDistance, 0.1f);
        }
        else if (sawType == SawType.MovingPendulum)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, 0.15f);
            Gizmos.DrawLine(origin, origin + Vector2.down * pendulumRadius);
        }
    }
}