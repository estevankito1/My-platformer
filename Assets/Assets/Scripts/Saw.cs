using UnityEngine;

public class Saw : MonoBehaviour
{
    public enum SawType { Static, MovingVertical, MovingHorizontal, MovingPendulum }

    [Header("Tipo de sierra")]
    public SawType sawType = SawType.MovingVertical;

    [Header("Rotación")]
    public float rotationSpeed = 300f;

    [Header("Movimiento")]
    [Tooltip("Cuánto se mueve desde su posición inicial")]
    public float moveDistance = 2f;     // distancia que recorre
    public float moveSpeed = 2f;        // velocidad

    [Header("Péndulo")]
    public float pendulumAngle = 45f;
    public float pendulumSpeed = 1f;
    public float pendulumRadius = 2f;

    [Header("Daño")]
    public float damage = 999f;
    public bool killInstantly = true;

    [Header("Visual (opcional)")]
    [SerializeField] private LineRenderer chain;
    [SerializeField] private ParticleSystem hitParticles;

    // Posición inicial — se guarda al arrancar
    private Vector2 startPosition;
    private float pendulumTime;

    private float elapsedTime;

    // -------------------------------------------------------
    void Start()
    {
        // Guardar la posición donde está en la escena
        startPosition = transform.position;
        elapsedTime = 0f;
    }

    // -------------------------------------------------------
    void Update()
    {
        elapsedTime += Time.deltaTime;

        // Siempre girar
        transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);

        switch (sawType)
        {
            case SawType.MovingVertical: DoVertical(); break;
            case SawType.MovingHorizontal: DoHorizontal(); break;
            case SawType.MovingPendulum: DoPendulum(); break;
                // Static: solo gira
        }

        // Cadena visual para péndulo
        if (sawType == SawType.MovingPendulum && chain != null)
        {
            chain.SetPosition(0, startPosition);
            chain.SetPosition(1, transform.position);
        }
    }

    // -------------------------------------------------------
    // VERTICAL: sube y baja desde su posición inicial
    // -------------------------------------------------------
    void DoVertical()
    {
        float t = Mathf.PingPong(elapsedTime * moveSpeed, 1f);
        float newY = Mathf.Lerp(startPosition.y, startPosition.y + moveDistance, t);
        transform.position = new Vector2(startPosition.x, newY);
    }

    // -------------------------------------------------------
    // HORIZONTAL: va y viene horizontalmente
    // -------------------------------------------------------
    void DoHorizontal()
    {
        float t = Mathf.PingPong(elapsedTime * moveSpeed, 1f);
        float newX = Mathf.Lerp(startPosition.x - moveDistance, startPosition.x + moveDistance, t);
        transform.position = new Vector2(newX, startPosition.y);
    }

    // -------------------------------------------------------
    // PÉNDULO
    // -------------------------------------------------------
    void DoPendulum()
    {
        pendulumTime += Time.deltaTime * pendulumSpeed;
        float angle = Mathf.Sin(pendulumTime) * pendulumAngle;
        float rad = angle * Mathf.Deg2Rad;

        Vector2 offset = new Vector2(
             Mathf.Sin(rad) * pendulumRadius,
            -Mathf.Cos(rad) * pendulumRadius);

        transform.position = startPosition + offset;
    }

    // -------------------------------------------------------
    // DAÑO
    // -------------------------------------------------------
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (hitParticles != null)
            Instantiate(hitParticles, other.transform.position, Quaternion.identity);

        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm == null) return;

        pm.TakeDamage(killInstantly ? 99999f : damage);
    }

    // -------------------------------------------------------
    // GIZMOS — muestra el recorrido en el editor
    // -------------------------------------------------------
    void OnDrawGizmos()
    {
        Vector2 origin = Application.isPlaying ? startPosition : (Vector2)transform.position;

        if (sawType == SawType.MovingVertical)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + Vector2.up * moveDistance);
            Gizmos.DrawWireSphere(origin, 0.1f);
            Gizmos.DrawWireSphere(origin + Vector2.up * moveDistance, 0.1f);
        }

        if (sawType == SawType.MovingHorizontal)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                origin + Vector2.left * moveDistance,
                origin + Vector2.right * moveDistance);
            Gizmos.DrawWireSphere(origin + Vector2.left * moveDistance, 0.1f);
            Gizmos.DrawWireSphere(origin + Vector2.right * moveDistance, 0.1f);
        }

        if (sawType == SawType.MovingPendulum)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, 0.1f);
            Gizmos.DrawLine(origin, origin + Vector2.down * pendulumRadius);
        }
    }
}