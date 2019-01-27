using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 14f;							// Amount of force added when the player jumps.
    [SerializeField] private float m_LowJumpMul = 2f;                           // Gravity multiplier if jumpkey is released
    [SerializeField] private float m_JumpEndMul = 2.5f;                         // Gravity multiplier for the downward part of the jump
    [Range(0, 1000f)] [SerializeField] private float m_HorizontalSpeed = 8f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)] [SerializeField] private float m_GroundSmoothing = .05f;	// How much to smooth out the movement
    [Range(0, .3f)] [SerializeField] private float m_AirSmoothing = .1f;        // How much to smooth out the movement
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private float m_GroundCheckDistance = 0.03f;                  // The distance to the ground check the ground has to be
    [SerializeField] private float m_GroundCheckWidth = 0.6f;                   // The width of the rectangular ground check
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    [SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching
    [SerializeField] private float m_MinRagdolVel = 1f;                         // velocity at which ragdolling ends

    [SerializeField] private PhysicsMaterial2D m_RagdollMaterial;                // The material used for ragdolling
    public GameObject m_Corpse;                                                 //This character's corpse

    [Header("Events")]
    [Space]

    public static UnityEvent OnLandEvent;
    public static UnityEvent OnDeathEvent;
    public static UnityEvent OnJumpEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public static BoolEvent OnCrouchEvent;
    [HideInInspector] public bool IsTouchingSpikes = false;

    private bool m_AirControl = true;							// Whether or not a player can steer while jumping;
    const float m_GroundedRadius = .2f;                 // Radius of the overlap circle to determine if grounded
    protected bool m_Grounded;                            // Whether or not the player is grounded.
    const float k_CeilingRadius = .2f;                  // Radius of the overlap circle to determine if the player can stand up
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;                  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;
    protected float m_PlayerGravity = 3f;                   // for storing the original player gravity
    protected bool m_CanJump = true;                        // can the character jump again
    private Vector2 m_LastFrameVelocity;                // the velocity during the last frame.
    private LevelManager m_Level;                       // the level manager
    private float m_RagdollStartTime;                   // the time when the ragdoll has started
    private float m_RagdollTimeout = 5f;                // The timeout for the ragdoll
    private bool m_IsRagdollOver = true;
    private Transform m_SpriteFollowerTrans;            // the transform of the sprite follower

    private bool m_wasCrouching = false;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        m_PlayerGravity = m_Rigidbody2D.gravityScale;

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();

        if (OnDeathEvent == null)
            OnDeathEvent = new UnityEvent();

        if (OnJumpEvent == null)
            OnJumpEvent = new UnityEvent();

        // get level manager
        m_Level = GameObject.Find("Level").GetComponent<LevelManager>();

        // get sprite follower
        m_SpriteFollowerTrans = transform.Find("SpriteFollower").GetComponent<Transform>();
    }

    private void FixedUpdate()
    {
        // store last frame veolicty for later
        m_LastFrameVelocity = m_Rigidbody2D.velocity;

        GroundCheck();

        if (!m_IsRagdollOver)
        {
            // check if ragdoll has ended
            bool slowed = m_Rigidbody2D.velocity.magnitude < m_MinRagdolVel;
            if ((slowed || Time.timeSinceLevelLoad > m_RagdollStartTime + m_RagdollTimeout) && (IsTouchingSpikes|| m_Grounded))
            {
                m_IsRagdollOver = true;
            }
        }
    }

    // a ground check that is a lot more precise
    private void GroundCheck()
    {
        // Raycast check
        // RaycastHit2D results = Physics2D.Raycast(m_PreciseGroundCheck.position, -m_PreciseGroundCheck.up, m_PreciseCheckDist, m_WhatIsGround);
        // return results.collider != null;

        // Point check
        // return Physics2D.OverlapPoint(m_PreciseGroundCheck.position, m_WhatIsGround) != null;
       
        // Box check
        Collider2D results = Physics2D.OverlapArea((Vector2)m_GroundCheck.position - new Vector2(m_GroundCheckWidth / 2, 0f), (Vector2)m_GroundCheck.position + new Vector2(m_GroundCheckWidth / 2, -m_GroundCheckDistance), m_WhatIsGround);

        bool wasGrounded = m_Grounded;
        m_Grounded = results != null;
        if (!wasGrounded && m_Grounded)
        {
            OnLandEvent.Invoke();
        }

    }

    public virtual void Move(float move, bool crouch, bool jump)
    {
        // If crouching, check to see if the character can stand up
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        //only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {

            // If crouching
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= m_CrouchSpeed;

                // Disable one of the colliders when crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * m_HorizontalSpeed, m_Rigidbody2D.velocity.y);
            float movementSmoothing;

            if (m_Grounded)
            {
                movementSmoothing = m_GroundSmoothing;
            }
            else
            {
                movementSmoothing = m_AirSmoothing;
            }

            // And then smoothing it out and applying it to the character. (This gives better responsivenes then using acceleration)
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, movementSmoothing);
            // Debug.Log(m_Rigidbody2D.velocity.x);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                // Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                // Flip();
            }
        }

        // If the player should jump...
        if (m_Grounded && jump && m_CanJump)
        {
            Jump();   
        }

        // If not grounded and not jumping increase Gravity
        m_Rigidbody2D.gravityScale = m_PlayerGravity;
        if (!m_Grounded)
        if (!m_Grounded)
        if (!m_Grounded)
        {
            if (m_Rigidbody2D.velocity.y < 0)  // going downwards
            {
                m_Rigidbody2D.gravityScale = m_PlayerGravity * m_JumpEndMul;
            }
            else if (!jump)  // low jump
            {
                m_Rigidbody2D.gravityScale = m_PlayerGravity * m_LowJumpMul;
            }
        }


        // Reset jump ability if you release space bar
        if (!jump)
        {
            m_CanJump = true;
        }
    }

    private void Jump()
    {
        // Add a vertical force to the player.
        m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce), ForceMode2D.Impulse);
        m_CanJump = false;

        OnJumpEvent.Invoke();
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public bool IsGrouned()
    {
        return m_Grounded;
    }

    public void StartRagdoll()
    {
        gameObject.GetComponent<Collider2D>().sharedMaterial = m_RagdollMaterial;
        m_IsRagdollOver = false;
        m_RagdollStartTime = Time.timeSinceLevelLoad;
    }

    public void OnRagdollEnd()
    {
        LeaveCorpse();
    }

    // leaves corpse at current position
    public void LeaveCorpse()
    {
        float zRot = m_SpriteFollowerTrans.localEulerAngles.z;

        // leave corpse and round rotation to nearest 90 degrees
        GameObject corpse = Instantiate(m_Corpse, transform.position, Quaternion.Euler(0f, 0f, Mathf.Round(zRot / 90) * 90), m_Level.m_Corpses.transform);
    }

    public bool HasRagdollEnded()
    {
        return m_IsRagdollOver;
    }

    public Vector2 GetLastFrameVelocity()
    {
        return m_LastFrameVelocity;
    }

    // Kill the player
    public virtual void Kill()
    {
        // Don't die if already dead (Is ragdoll over)
        if (m_IsRagdollOver && m_Level.m_LevelState == LevelManager.LevelState.Playing)
        {
            OnDeathEvent.Invoke();
        }
    }
}