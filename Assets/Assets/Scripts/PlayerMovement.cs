using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // -------------------------------------------------------
    // MOVIMIENTO
    // -------------------------------------------------------
    [Header("Move Info")]
    [SerializeField] private float speed = 5f;
    private Vector2 movement;
    private float xPosLastFrame;
    private Vector2 screenBounds;
    private float playerHalfWidth;

    // -------------------------------------------------------
    // SALTO
    // -------------------------------------------------------
    [Header("Jump Info")]
    [SerializeField] private float jumpForce = 8f;
    private bool canDoubleJump = false;
    private bool isGrounded = false;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.3f, 0.05f);
    public LayerMask GroundLayer;

    // -------------------------------------------------------
    // ATAQUE
    // -------------------------------------------------------
    [Header("Attack Info")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private Transform attackPoint;   // hijo vacío al lado del player
    [SerializeField] private LayerMask enemyLayer;
    private float lastAttackTime;
    private bool isAttacking;

    // -------------------------------------------------------
    // DASH
    // -------------------------------------------------------
    [Header("Dash Info")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private TrailRenderer dashTrail;
    private bool isDashing;
    private bool canDash = true;
    private float dashDirection = 1f;

    // -------------------------------------------------------
    // SALUD
    // -------------------------------------------------------
    [Header("Health Info")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    [SerializeField] private float invincibleTime = 1.5f;
    private bool isInvincible = false;

    // -------------------------------------------------------
    // REFERENCIAS
    // -------------------------------------------------------
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    private Rigidbody2D rb;

    // -------------------------------------------------------
    // INICIO
    // -------------------------------------------------------
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        playerHalfWidth = spriteRenderer.bounds.extents.x;
        currentHealth = maxHealth;
        UIManager.Instance?.UpdateHealthBar(currentHealth, maxHealth);
    }

    // -------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------
    void Update()
    {
        if (isDashing) return;

        // Ground check con OverlapBox
        bool groundedThisFrame = Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0f,
            GroundLayer);

        // Cuando aterriza: resetear doble salto
        if (groundedThisFrame && !isGrounded)
            canDoubleJump = false;

        isGrounded = groundedThisFrame;

        HandleMovement();
        HandleJump();
        HandleAttack();
        HandleDash();
        ClampMovement();
        FlipCharacterX();
        HandleAnimations();
    }

    // -------------------------------------------------------
    // MOVIMIENTO HORIZONTAL
    // -------------------------------------------------------
    private void HandleMovement()
    {
        float input = Input.GetAxisRaw("Horizontal");
        movement.x = input * speed * Time.deltaTime;
        transform.Translate(movement);

        if (input != 0)
            dashDirection = Mathf.Sign(input);
    }

    // -------------------------------------------------------
    // SALTO + DOBLE SALTO
    // Lógica:
    //   - En el suelo       → primer salto  → habilita doble salto
    //   - En el aire + can  → doble salto   → bloquea más saltos
    //   - Tercer intento    → no pasa nada
    // -------------------------------------------------------
    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                PerformJump();
                canDoubleJump = true;   // habilitar tras primer salto
            }
            else if (canDoubleJump)
            {
                PerformJump();
                canDoubleJump = false;  // bloquear tercer salto
            }
        }

        // Caída más rápida (game feel)
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * 2.5f * Time.deltaTime;
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * 1.5f * Time.deltaTime;
    }

    private void PerformJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        // Apagar y encender para que el Animator detecte el cambio incluso en el aire
        animator.SetBool("isJumping", false);
        animator.SetBool("isJumping", true);
        animator.SetBool("isGrounded", false);
    }

    // -------------------------------------------------------
    // ATAQUE — tecla Z o botón Fire1
    // Hitbox: OverlapCircleAll desde attackPoint
    // -------------------------------------------------------
    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Fire1"))
        {
            if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
            {
                StartCoroutine(DoAttack());
            }
        }
    }

    private IEnumerator DoAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        animator.SetTrigger("attack");

        // Pequeña espera para que la animación llegue al momento del golpe
        yield return new WaitForSeconds(0.15f);

        // Hitbox: detectar enemigos en rango
        Vector2 hitPos = attackPoint != null
            ? (Vector2)attackPoint.position
            : (Vector2)transform.position + Vector2.right * dashDirection * 0.8f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPos, attackRange, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            // Si el enemigo tiene un script Enemy, aplicar daño
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(attackCooldown - 0.15f);
        isAttacking = false;
    }

    // -------------------------------------------------------
    // DASH
    // -------------------------------------------------------
    private void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            StartCoroutine(DoDash());
    }

    private IEnumerator DoDash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashDirection * dashForce, 0f);

        if (dashTrail != null) dashTrail.emitting = true;
        animator.SetBool("isDashing", true);

        yield return new WaitForSeconds(dashDuration);

        if (dashTrail != null) dashTrail.emitting = false;
        animator.SetBool("isDashing", false);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // -------------------------------------------------------
    // SALUD Y DAÑO
    // -------------------------------------------------------
    public void TakeDamage(float amount)
    {
        // Durante dash el player es invulnerable
        if (isInvincible || isDashing) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f); // no bajar de 0

        UIManager.Instance?.UpdateHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            Die();
        else
            StartCoroutine(InvincibilityFrames());
    }

    private void Die()
    {
        GameManager.Instance?.PlayerDied();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UIManager.Instance?.UpdateHealthBar(currentHealth, maxHealth);
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.3f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.2f;
        }

        spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    // -------------------------------------------------------
    // ANIMACIONES
    // -------------------------------------------------------
    private void HandleAnimations()
    {
        float input = Input.GetAxisRaw("Horizontal");
        animator.SetBool("isRunning", input != 0 && isGrounded);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        if (isGrounded)
            animator.SetBool("isJumping", false);
    }

    // -------------------------------------------------------
    // FLIP SPRITE
    // -------------------------------------------------------
    private void FlipCharacterX()
    {
        float input = Input.GetAxisRaw("Horizontal");
        if (input > 0 && transform.position.x > xPosLastFrame)
            spriteRenderer.flipX = false;
        else if (input < 0 && transform.position.x < xPosLastFrame)
            spriteRenderer.flipX = true;

        xPosLastFrame = transform.position.x;
    }

    // -------------------------------------------------------
    // CLAMP EN PANTALLA
    // -------------------------------------------------------
    private void ClampMovement()
    {
        float clampedX = Mathf.Clamp(transform.position.x,
            -screenBounds.x + playerHalfWidth,
             screenBounds.x - playerHalfWidth);

        Vector2 pos = transform.position;
        pos.x = clampedX;
        transform.position = pos;
    }

    // -------------------------------------------------------
    // GIZMOS — hitbox de ataque + ground check
    // -------------------------------------------------------
    private void OnDrawGizmosSelected()
    {
        // Ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }

        // Hitbox de ataque
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
