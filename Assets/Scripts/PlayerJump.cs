using UnityEngine;

/*{
public class PlayerJump : MonoBehaviour

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float jumpForce = 6f;

    [Header("Ground Check")]
    public Transform groundCheck;
    private bool isGrounded = false;
    public LayerMask GroundLayer;

    public float groundCheckRadius = 0.2f;
   



    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, GroundLayer);
        if (Input.GetButtonDown("Jump") && GetComponent<PlayerMovement>().isGrounded)
        {
            Jump();

            
        }
       
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && GetComponent<PlayerMovement>().isGrounded)
        {
           rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
              
              

        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

}*/
