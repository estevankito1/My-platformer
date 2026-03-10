using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // -------------------------------------------------------
    // MOVIMIENTO
    // -------------------------------------------------------
    [Header("Movimiento")]
    [SerializeField] private float speed = 6f;
    private float horizontalInput;
    private float facingDirection = 1f;

    // -------------------------------------------------------
    // SALTO
    // -------------------------------------------------------
    [Header("Salto")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 1.5f;
    private bool canDoubleJump = false;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.4f, 0.05f);
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    // -------------------------------------------------------
    // DASH
    // -------------------------------------------------------
    [Header("Dash")]
    [SerializeField] private float dashForce = 18f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.9f;
    [SerializeField] private TrailRenderer dashTrail;
    private bool isDashing;
    private bool canDash = true;

    // -------------------------------------------------------
    // ATAQUE
    // -------------------------------------------------------
    [Header("Ataque")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackCooldown = 0.45f;
    [SerializeField] private float attackRange = 1.1f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayer;
    private bool isAttacking;
    private float lastAttackTime;

    // -------------------------------------------------------
    // SALUD
    // -------------------------------------------------------
    [Header("Salud")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invincibleTime = 1.2f;
    [SerializeField] private float knockbackForce = 6f;
    private float currentHealth;
    private bool isInvincible;

    // -------------------------------------------------------
    // REFERENCIAS
    // -------------------------------------------------------
    [Header("Referencias")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    private Rigidbody2D rb;

    // -------------------------------------------------------
    // INICIO
    // -------------------------------------------------------
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        UIManager.Instance?.UpdateHealthBar(currentHealth, maxHealth);
    }

    // -------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------
    void Update()
    {
        if (isDashing) return;

        CheckGround();
        HandleInput();
        FlipSprite();
        UpdateAnimations();
    }

    // -------------------------------------------------------
    // GROUND CHECK
    // -------------------------------------------------------
    void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        // Aterriza: resetear doble salto
        if (isGrounded && !wasGrounded)
            canDoubleJump = false;
    }

    // -------------------------------------------------------
    // INPUT
    // -------------------------------------------------------
    void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Movimiento
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

        // Salto
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                PerformJump();
                canDoubleJump = true;
            }
            else if (canDoubleJump)
            {
                PerformJump();
                canDoubleJump = false;
            }
        }

        // Caída con mejor game feel
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            StartCoroutine(DoDash());

        // Ataque
        if ((Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Fire1")) && !isAttacking)
            if (Time.time >= lastAttackTime + attackCooldown)
                StartCoroutine(DoAttack());
    }

    // -------------------------------------------------------
    // SALTO
    // -------------------------------------------------------
    void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        animator.SetBool("isJumping", false);
        animator.SetBool("isJumping", true);
        animator.SetBool("isGrounded", false);
    }

    // -------------------------------------------------------
    // DASH
    // -------------------------------------------------------
    IEnumerator DoDash()
    {
        canDash = false;
        isDashing = true;

        float gravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(facingDirection * dashForce, 0f);

        if (dashTrail) dashTrail.emitting = true;
        animator.SetBool("isDashing", true);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = gravity;
        if (dashTrail) dashTrail.emitting = false;
        animator.SetBool("isDashing", false);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // -------------------------------------------------------
    // ATAQUE
    // -------------------------------------------------------
    IEnumerator DoAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Esperar el momento del golpe
        yield return new WaitForSeconds(0.12f);

        // Calcular posición del hitbox
        Vector2 hitPos = attackPoint != null
            ? (Vector2)attackPoint.position
            : (Vector2)transform.position + Vector2.right * facingDirection * 0.8f;

        // Detectar enemigos
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, attackRange, enemyLayer);
        foreach (Collider2D hit in hits)
            hit.GetComponent<Enemy>()?.TakeDamage(attackDamage);

        yield return new WaitForSeconds(0.33f);
        isAttacking = false;
    }

    // -------------------------------------------------------
    // RECIBIR DAÑO
    // -------------------------------------------------------
    public void TakeDamage(float amount, Vector2 damageSourcePos)
    {
        if (isInvincible || isDashing) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        UIManager.Instance?.UpdateHealthBar(currentHealth, maxHealth);

        // Knockback
        Vector2 knockDir = ((Vector2)transform.position - damageSourcePos).normalized;
        knockDir.y = 0.4f; // pequeño impulso hacia arriba
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);

        animator.SetTrigger("hurt");

        if (currentHealth <= 0f)
            Die();
        else
            StartCoroutine(InvincibilityFrames());
    }

    // Sobrecarga sin posición (para daño de sierra u otras fuentes)
    public void TakeDamage(float amount)
    {
        TakeDamage(amount, transform.position + Vector3.left);
    }

    void Die()
    {
        rb.linearVelocity = Vector2.zero;
        this.enabled = false;
        StartCoroutine(CallGameOver());
    }

    // Muerte instantánea — ignora invencibilidad (para DeathZone)
    public void InstantDie()
    {
        isInvincible = false;
        currentHealth = 0f;
        UIManager.Instance?.UpdateHealthBar(0f, maxHealth);
        Die();
    }

    IEnumerator CallGameOver()
    {
        yield return new WaitForSeconds(0.6f);
        GameManager.Instance?.PlayerDied();
    }

    IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.25f);
            yield return new WaitForSeconds(0.08f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            elapsed += 0.16f;
        }
        spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isInvincible = false;
        this.enabled = true;
        UIManager.Instance?.UpdateHealthBar(currentHealth, maxHealth);
    }

    // -------------------------------------------------------
    // ANIMACIONES
    // -------------------------------------------------------
    void UpdateAnimations()
    {
        animator.SetBool("isRunning", horizontalInput != 0 && isGrounded);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
        if (isGrounded) animator.SetBool("isJumping", false);
    }

    // -------------------------------------------------------
    // FLIP SPRITE
    // -------------------------------------------------------
    void FlipSprite()
    {
        if (horizontalInput > 0)
        {
            spriteRenderer.flipX = false;
            facingDirection = 1f;
        }
        else if (horizontalInput < 0)
        {
            spriteRenderer.flipX = true;
            facingDirection = -1f;
        }
    }



    // -------------------------------------------------------
    // GIZMOS
    // -------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}

