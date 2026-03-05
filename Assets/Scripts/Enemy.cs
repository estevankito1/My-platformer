using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack, Dead }
    private EnemyState currentState = EnemyState.Patrol;

    // -------------------------------------------------------
    // SALUD
    // -------------------------------------------------------
    [Header("Salud")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;

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
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private LayerMask groundLayer;

    // -------------------------------------------------------
    // ATAQUE
    // -------------------------------------------------------
    [Header("Ataque")]
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float knockbackForce = 5f;
    private float lastAttackTime;

    // -------------------------------------------------------
    // REFERENCIAS
    // -------------------------------------------------------
    [Header("Referencias")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 startPosition;
    private float patrolDirection = 1f;

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

        float distFromStart = transform.position.x - startPosition.x;
        if (distFromStart >= patrolDistance) patrolDirection = -1f;
        else if (distFromStart <= -patrolDistance) patrolDirection = 1f;

        // Detectar borde de plataforma
        if (groundLayer != 0)
        {
            Vector2 edgeCheck = new Vector2(
                transform.position.x + patrolDirection * 0.6f,
                transform.position.y - 1f);
            if (!Physics2D.OverlapCircle(edgeCheck, 0.1f, groundLayer))
                patrolDirection *= -1f;
        }

        Flip(patrolDirection);
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
            {
                // Pasar posición del enemigo para knockback
                hit.GetComponent<PlayerMovement>()?.TakeDamage(attackDamage, transform.position);
            }
        }
    }

    // -------------------------------------------------------
    // RECIBIR DAÑO
    // -------------------------------------------------------
    public void TakeDamage(float amount)
    {
        if (currentState == EnemyState.Dead) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
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

        animator?.SetTrigger("die");

        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
    }

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
    }
}