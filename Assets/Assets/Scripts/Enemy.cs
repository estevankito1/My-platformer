using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack }
    private EnemyState currentState = EnemyState.Patrol;

    // -------------------------------------------------------
    // SALUD
    // -------------------------------------------------------
    [Header("Health")]
    public float maxHealth = 50f;
    private float currentHealth;

    // -------------------------------------------------------
    // DETECCIÓN
    // -------------------------------------------------------
    [Header("Detección")]
    public float detectionRange = 5f;
    public float attackRange = 1.2f;
    public LayerMask playerLayer;

    // -------------------------------------------------------
    // MOVIMIENTO
    // -------------------------------------------------------
    [Header("Movimiento")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float patrolDistance = 3f;
    public LayerMask groundLayer;

    // -------------------------------------------------------
    // ATAQUE
    // -------------------------------------------------------
    [Header("Ataque")]
    public float attackDamage = 10f;
    public float attackCooldown = 1f;

    [Header("Knockback al player")]
    public bool applyKnockback = true;
    public float knockbackForce = 5f;

    // -------------------------------------------------------
    // REFERENCIAS
    // -------------------------------------------------------
    [Header("Referencias")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Transform player;
    private float lastAttackTime;

    // Patrulla
    private Vector2 startPosition;
    private float patrolDirection = 1f;

    // -------------------------------------------------------
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        currentHealth = maxHealth;

        // Buscar player por tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("Enemy: No encontró un GameObject con tag 'Player'");
    }

    // -------------------------------------------------------
    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Máquina de estados
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

        HandleAnimations();
    }

    // -------------------------------------------------------
    // PATRULLA
    // -------------------------------------------------------
    void DoPatrol()
    {
        // Mover horizontalmente
        rb.linearVelocity = new Vector2(patrolSpeed * patrolDirection, rb.linearVelocity.y);

        // Girar al llegar al límite de patrulla
        float distFromStart = transform.position.x - startPosition.x;
        if (distFromStart >= patrolDistance)
            patrolDirection = -1f;
        else if (distFromStart <= -patrolDistance)
            patrolDirection = 1f;

        // Evitar caer de plataformas
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
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(chaseSpeed * direction, rb.linearVelocity.y);
        Flip(direction);
    }

    // -------------------------------------------------------
    // ATAQUE
    // -------------------------------------------------------
    void DoAttack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            animator?.SetTrigger("attack");

            Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
            if (hit != null)
            {
                hit.GetComponent<PlayerMovement>()?.TakeDamage(attackDamage);

                if (applyKnockback)
                {
                    Rigidbody2D playerRb = hit.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        Vector2 knockDir = (hit.transform.position - transform.position).normalized;
                        playerRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

    // -------------------------------------------------------
    // RECIBIR DAÑO (llamado desde PlayerMovement.DoAttack)
    // -------------------------------------------------------
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        animator?.SetTrigger("hurt");

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        animator?.SetTrigger("die");
        // Desactivar colisión y movimiento al morir
        GetComponent<Collider2D>().enabled = false;
        rb.linearVelocity = Vector2.zero;
        this.enabled = false;
        // Destruir tras un momento para que la animación termine
        Destroy(gameObject, 0.5f);
    }

    // -------------------------------------------------------
    // ANIMACIONES
    // -------------------------------------------------------
    void HandleAnimations()
    {
        if (animator == null) return;
        animator.SetBool("isRunning", currentState == EnemyState.Patrol || currentState == EnemyState.Chase);
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
