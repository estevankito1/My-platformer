using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
   //movimiento
    [SerializeField] private float speed = 5f;
    private float movingInput;
    private Vector2 movement;
    [SerializeField] private float jumpForce = 5f;


    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;


    public bool isGrounded = false;


    private Rigidbody2D rb;
    private Vector2 screenBounds;
    private float playerHalfWidth;
    private float xPosLastFrame;


    private void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        playerHalfWidth = spriteRenderer.bounds.extents.x;
    }


    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        ClampMovement();
        FlipCharacterX();

    }

    private void FlipCharacterX() 
    {
        float input = Input.GetAxisRaw("Horizontal");
        if (input > 0 && (transform.position.x > xPosLastFrame)) 
        {
            spriteRenderer.flipX = false;
        } 
       else if (input < 0 && (transform.position.x < xPosLastFrame)) 
       {
            spriteRenderer.flipX = true;
       }

       xPosLastFrame = transform.position.x;

    }

    private void ClampMovement()
    {
        float clampedX = Mathf.Clamp(transform.position.x, -screenBounds.x + playerHalfWidth, screenBounds.x - playerHalfWidth);
        Vector2 pos = transform.position;
        pos.x = clampedX;
        transform.position = pos;
    }

    private void HandleMovement()
    {
        float input = Input.GetAxisRaw("Horizontal");
        movement.x = input * speed * Time.deltaTime;
        transform.Translate(movement);
        if (input != 0) 
        {
            animator.SetBool("isRunning", true);
        } 
        else 
        {
            animator.SetBool("isRunning", false);
        }
    }
}
