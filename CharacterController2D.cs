using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float jumpForce = 500f;                          // Amount of force added when the player jumps.

    [SerializeField] private float dashDistance = 100f;

    [Range(0, .3f)] [SerializeField] private float MovementSmoothing = .05f;  // How much to smooth out the movement
    [SerializeField] private bool AirControl = true;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform GroundCheck;                           // A position marking where to check if the player is grounded

    const float GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool isGrounded = true;            // Whether or not the player is grounded.
    private Rigidbody2D rigidBody;
    private bool isFacingRight = true;  // true if facing right, false if facing left
    private Vector3 velocity = Vector3.zero;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    private bool canDoubleJump = true; // becomes false after a double jump. Becomes true once player lands on ground again

    private int airDashCount = 0; // increments everytime player dashes in mid air, resets after landing on ground again
    private int groundDashCount = 0; // increments everytime player dashes on ground, doesn't reset

    private const int maxAirDashes = 2; // airDashCount cannot exceed this constant

    private float dashCooldown = 0.5f; // the cooldown time between each ground dash. gets added to nextDashTime after every 2 ground dashes

    private float nextDashTime = 0f; // the current time must be greater than or equal to this in order to ground dash

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(GroundCheck.position, GroundedRadius, WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                isGrounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }


    public void Move(float move, bool jump, bool dash)
    {
        //only control the player if grounded or airControl is turned on
        if (isGrounded || AirControl)
        {
            // Move the character by finding the target velocity
            Vector3 targetVelocity = Vector2.zero;
            if (!dash)
            {
                targetVelocity = new Vector2(move * 10f, rigidBody.velocity.y);
            }
            else if ((isGrounded && Time.time >= nextDashTime) || (!isGrounded && airDashCount < maxAirDashes))
            {
                targetVelocity = Dash();
                if(!isGrounded && airDashCount < maxAirDashes) {
                    airDashCount++;
                } else if(isGrounded && Time.time >= nextDashTime) {
                    groundDashCount++;
                    // check if 2 dashes have occurred and puts the cooldown
                    if(groundDashCount%2 == 0) {
                        nextDashTime = Time.time + dashCooldown; 
                    }
                }
            }

            // And then smoothing it out and applying it to the character
            rigidBody.velocity = Vector3.SmoothDamp(rigidBody.velocity, targetVelocity, ref velocity, MovementSmoothing);

            // Stops moving left and starts moving right
            if (move > 0 && !isFacingRight)
            {
                Flip();
            }
            // Stops moving right and starts moving left
            else if (move < 0 && isFacingRight)
            {
                Flip();
            }
        }
        // Jump
        if (isGrounded && jump)
        {
            isGrounded = false;
            rigidBody.AddForce(new Vector2(0f, jumpForce));
        }
        // Double Jump
        else if (jump && canDoubleJump)
        {
            rigidBody.velocity = new Vector2(0f, 0f);
            rigidBody.AddForce(new Vector2(0f, jumpForce));
            canDoubleJump = false;
        }

    }


    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        isFacingRight = !isFacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private Vector3 Dash()
    {
        Vector3 vector = Vector2.zero;
        if (isFacingRight)
        {
            vector = new Vector2(dashDistance, rigidBody.velocity.y);
        }
        else
        {
            vector = new Vector2(-1 * dashDistance, rigidBody.velocity.y);
        }
        return vector;
    }

    // Activates once the player lands on the ground
    public void Land()
    {
        canDoubleJump = true;
        airDashCount = 0;
    }

    public bool grounded() { return isGrounded; }

    public bool canDash() { return (isGrounded && Time.time >= nextDashTime) || (!isGrounded && airDashCount < maxAirDashes); }

    public bool FacingRight() { return isFacingRight; }

}
