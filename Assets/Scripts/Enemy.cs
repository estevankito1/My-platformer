using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack, Dead }

    [SerializeField]
    private EnemyState currentState = EnemyState.Patrol;

    // -------------------------------------------------------
    // SALUD
    // -------------------------------------------------------
    [Header("Salud")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;

    // -------------------------------------------------------
    // HEALTH BAR
    // -------------------------------------------------------
    [Header("Health Bar")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0f, 1.2f, 0f);
    private Slider healthSlider;
    private GameObject healthBarInstance;

    // -------------------------------------------------------
    // DETECCIÓN
    // -------------------------------------------------------
    [Header("Detección")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private LayerMask playerLayer;

    // -------------------------------------------------------
    // MOVIMIENTO
    // -------------------------------------------------------
    [Header("Movimiento")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private LayerMask groundLayer;

    // -------------------------------------------------------
    // PATRULLA MANUAL (opcional)
    // Si asignas patrolPoints en el Inspector los usa
    // Si no, genera patrulla automática desde su posición
    // -------------------------------------------------------
    [Header("Patrulla Manual (opcional)")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform edgeDetectionPoint;
    [SerializeField] private float edgeDetectionRange = 0.1f;
    [SerializeField] private float patrolStopDistance = 0.5f;
    private Transform currentPoint;
    private int patrolPointIndex;

    // -------------------------------------------------------
    // PATRULLA AUTOMÁTICA
    // -------------------------------------------------------
    [Header("Patrulla Automática")]
    [SerializeField] private float autoPatrolDistance = 3f;
    private Vector2 autoPointA, autoPointB;
    private bool useAutoPatrol = false;

    private float patrolDirection = 1f;

    // -------------------------------------------------------
    // ATAQUE
    // -------------------------------------------------------
    [Header("Ataque")]
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 1.2f;
    private float lastAttackTime;

    // -------------------------------------------------------
    // REFERENCIAS
    // -------------------------------------------------------
    [Header("Referencias")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 startPosition;
    private EnemySpawner spawner;

    // -------------------------------------------------------
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("Enemy: No encontró un GameObject con tag 'Player'");

        // Si no tiene patrol points asignados ? patrulla automática
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            useAutoPatrol = true;
            autoPointA = startPosition + Vector2.left * autoPatrolDistance;
            autoPointB = startPosition + Vector2.right * autoPatrolDistance;
        }
        else
        {
            useAutoPatrol = false;
            patrolPointIndex = patrolPoints.Length - 1;
            currentPoint = patrolPoints[patrolPointIndex];
        }

        SpawnHealthBar();
    }

    // -------------------------------------------------------
    // HEALTH BAR
    // -------------------------------------------------------
    void SpawnHealthBar()
    {
        if (healthBarPrefab == null) return;

        healthBarInstance = Instantiate(healthBarPrefab, transform.position + healthBarOffset, Quaternion.identity);
        healthBarInstance.transform.SetParent(transform);
        healthBarInstance.transform.localPosition = healthBarOffset;

        healthSlider = healthBarInstance.GetComponentInChildren<Slider>();
        if (healthSlider != null)
            healthSlider.value = 1f;
    }

    void UpdateHealthBar()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;
    }

    // -------------------------------------------------------
    void Update()
    {
        if (currentState == EnemyState.Dead || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
            currentState = EnemyState.Attack;
        else if (dist <= detectionRange)
            currentState = EnemyState.Chase;
        else
            currentState = EnemyState.Patrol;

        switch (currentState)
        {
            case EnemyState.Patrol: DoPatrol(); break;
            case EnemyState.Chase: DoChase(); break;
            case EnemyState.Attack: DoAttack(); break;
        }

        UpdateAnimations();
    }

    // -------------------------------------------------------
    // PATRULLA
    // -------------------------------------------------------
    void DoPatrol()
    {
        rb.linearVelocity = new Vector2(patrolSpeed * patrolDirection, rb.linearVelocity.y);

        if (useAutoPatrol)
        {
            // Patrulla automática: va y viene entre autoPointA y autoPointB
            float distToA = Vector2.Distance(transform.position, autoPointA);
            float distToB = Vector2.Distance(transform.position, autoPointB);

            if (patrolDirection > 0 && distToB <= 0.3f)
                patrolDirection = -1f;
            else if (patrolDirection < 0 && distToA <= 0.3f)
                patrolDirection = 1f;

            // Detectar borde si tiene edgeDetectionPoint
            if (edgeDetectionPoint != null)
            {
                if (!Physics2D.Raycast(edgeDetectionPoint.position, Vector3.down, edgeDetectionRange, groundLayer))
                    patrolDirection *= -1f;
                Debug.DrawRay(edgeDetectionPoint.position, Vector3.down * edgeDetectionRange, Color.red);
            }
        }
        else
        {
            // Patrulla manual con patrol points
            float distanceToDestination = Vector2.Distance(transform.position, currentPoint.position);
            if (distanceToDestination <= patrolStopDistance)
                UpdatePatrolPoint();

            if (edgeDetectionPoint != null)
            {
                if (!Physics2D.Raycast(edgeDetectionPoint.position, Vector3.down, edgeDetectionRange, groundLayer))
                    UpdatePatrolPoint();
                Debug.DrawRay(edgeDetectionPoint.position, Vector3.down * edgeDetectionRange, Color.red);
            }
        }

        Flip(patrolDirection);
    }

    void UpdatePatrolPoint()
    {
        patrolPointIndex++;
        if (patrolPointIndex >= patrolPoints.Length)
            patrolPointIndex = 0;

        currentPoint = patrolPoints[patrolPointIndex];
        patrolDirection *= -1f;
    }

    // -------------------------------------------------------
    // PERSECUCIÓN
    // -------------------------------------------------------
    void DoChase()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(chaseSpeed * dir, rb.linearVelocity.y);
        Flip(dir);
    }

    // -------------------------------------------------------
    // ATAQUE
    // -------------------------------------------------------
    void DoAttack()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            animator?.SetTrigger("attack");

            Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
            if (hit != null)
                hit.GetComponent<PlayerMovement>()?.TakeDamage(attackDamage, transform.position);
        }
    }

    // -------------------------------------------------------
    // RECIBIR DAÑO
    // -------------------------------------------------------
    public void TakeDamage(float amount)
    {
        if (currentState == EnemyState.Dead) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        UpdateHealthBar();

        // Actualizar barra del enemy en el HUD
        UIManager.Instance?.UpdateEnemyHealthBar(currentHealth, maxHealth);

        animator?.SetTrigger("hurt");

        if (currentHealth <= 0f)
            StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        currentState = EnemyState.Dead;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;

        if (healthBarInstance != null)
            Destroy(healthBarInstance);

        // Ocultar barra del HUD
        UIManager.Instance?.HideEnemyHealthBarNow();

        animator?.SetTrigger("die");
        spawner?.OnEnemyDied();

        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
    }

    // -------------------------------------------------------
    // SPAWNER
    // -------------------------------------------------------
    public void SetSpawner(EnemySpawner s) => spawner = s;

    // -------------------------------------------------------
    // ANIMACIONES
    // -------------------------------------------------------
    void UpdateAnimations()
    {
        if (animator == null) return;
        animator.SetBool("isRunning", currentState != EnemyState.Attack);
        animator.SetBool("isChasing", currentState == EnemyState.Chase);
    }

    // -------------------------------------------------------
    // FLIP SPRITE
    // -------------------------------------------------------
    void Flip(float direction)
    {
        if (direction == 0) return;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    // -------------------------------------------------------
    // GIZMOS
    // -------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Mostrar rango de patrulla automática
        Vector2 origin = Application.isPlaying ? startPosition : (Vector2)transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin + Vector2.left * autoPatrolDistance, 0.15f);
        Gizmos.DrawWireSphere(origin + Vector2.right * autoPatrolDistance, 0.15f);
        Gizmos.DrawLine(origin + Vector2.left * autoPatrolDistance, origin + Vector2.right * autoPatrolDistance);
    }
}