using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D feetCol;
    [SerializeField] private Collider2D bodyCol;

    private Rigidbody2D rb;

    //movement vars;
    private Vector2 moveVelocity;
    private bool isFacingRight;
    private bool isFacingUp;
    private bool isFacingDown;

    //collision check vars
    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;
    private bool isGrounded;
    private bool bumpedHead;

    //Hitting
    public bool isSwinging;
    public GameObject swingObject;
    public Animator anim;

    //Jump Vars
    public float VerticalVelocity { get; private set; }
    private bool isJumping;
    private bool isFastFalling;
    private bool isFalling;
    private float fastFallTime;
    private float fastFallReleaseSpeed;
    private int numberOfJumpsUsed;

    //Apex Vars
    private float apexPoint;
    private float timePastApexThreshold;
    private bool isPastApexThreshold;

    //Jump Buffer Vars
    private float jumpBufferTime;
    private bool jumpReleasedDuringBuffer;

    //Coyote Time Vars
    private float coyoteTimer;


    private void Awake()
    {
        isFacingRight = true;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();
        InitiateSwing();

        if (isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
        }


    }

    #region Jump

    private void Jump()
    {
        //Apply Gravity While Falling
        if (isJumping)
        {
            if (bumpedHead)
            {
                isFastFalling = true;
            }
            //Gravity on Ascending
            if (VerticalVelocity >= 0f)
            {
                apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (apexPoint > MoveStats.ApexThreshold)
                {
                    if (!isPastApexThreshold)
                    {
                        isPastApexThreshold = true;
                        timePastApexThreshold = 0f;
                    }

                    if (isPastApexThreshold)
                    {
                        timePastApexThreshold += Time.deltaTime;
                        if (timePastApexThreshold < MoveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }

                //Gravity on Ascending but not past apex Threshold
                else
                {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                    if (isPastApexThreshold)
                    {
                        isPastApexThreshold = false;
                    }
                }
            }


            //Gravity on Descending
            else if (!isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }

            else if (VerticalVelocity < 0f)
            {
                if (!isFalling)
                {
                    isFalling = true;
                }
            }


        }
        //Jump Cut
        if (isFastFalling)
        {
            if (fastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (fastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f, (fastFallTime / MoveStats.TimeForUpwardsCancel));
            }

            fastFallTime += Time.fixedDeltaTime;
        }
        //Normal Gravity While Falling
        if (!isGrounded && !isJumping)
        {
            if (isFalling)
            {
                isFalling = true;
            }

            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }

        //Clamp Fall Speed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, VerticalVelocity);
    }

    private void InitiateJump(int numJumpsUsed)
    {
        if (!isJumping)
        {
            isJumping = true;
        }

        jumpBufferTime = 0f;
        numberOfJumpsUsed += numJumpsUsed;
        VerticalVelocity = MoveStats.InitialJumpVelocity;
    }

    private void JumpChecks()
    {
        //Jump is Pressed
        if (InputManager.JumpWasPressed)
        {
            jumpBufferTime = MoveStats.JumpBufferTime;
            jumpReleasedDuringBuffer = false;
        }

        //Jump is Released
        if (InputManager.JumpWasReleased)
        {
            if (jumpBufferTime > 0f)
            {
                jumpReleasedDuringBuffer = true;
            }
            if (isJumping && VerticalVelocity > 0f)
            {
                if (isPastApexThreshold)
                {
                    isPastApexThreshold = false;
                    isFastFalling = true;
                    fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    isFastFalling = true;
                    fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        //Inititate Jump with Coyote and Buffer
        if (jumpBufferTime > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (jumpReleasedDuringBuffer)
            {
                isFastFalling = true;
                fastFallReleaseSpeed = VerticalVelocity;
            }
        }

        //Double Jump
        else if (jumpBufferTime > 0f && isJumping && numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            isFastFalling = false;
            InitiateJump(1);
        }

        //Air Jump During Coyote 
        else if (jumpBufferTime > 0f && isFalling && numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2);
            isFastFalling = false;
        }

        //Landed
        if ((isJumping || isFalling) && isGrounded && VerticalVelocity <= 0f)
        {
            isJumping = false;
            isFalling = false;
            isFastFalling = false;
            fastFallTime = 0f;
            isPastApexThreshold = false;
            numberOfJumpsUsed = 0;


            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        jumpBufferTime -= Time.deltaTime;

        if(!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }
        else
        {
            coyoteTimer = MoveStats.JumpCoyoteTime;
        }
    }

    #endregion

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            }
            else
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
            }

            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
        }
        else if (moveInput == Vector2.zero)
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
        }

    }

    private void TurnCheck(Vector2 moveInput)
    {
        if(isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
        if (moveInput.y > 0)
        {
            isFacingUp = true;
            isFacingDown = false;
        }
        else if (moveInput.y == 0)
        {
            isFacingUp = false;
            isFacingDown = false;
        }
        else if (moveInput.y < 0)
        {
            isFacingUp = false;
            isFacingDown = true;
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion

    #region Swinging

    private void InitiateSwing()
    {
        if (isSwinging) return;

        if (InputManager.HitWasPressed)
        {
            isSwinging = true;
            anim.SetTrigger("Hit");

            // Determine swing direction based on input
            Vector2 inputDirection = InputManager.Movement.normalized;
            Quaternion swingRotation = Quaternion.identity; // Default rotation (facing right)

            if (inputDirection == Vector2.zero) // Neutral attack
            {
                inputDirection = isFacingRight ? Vector2.right : Vector2.left;
            }

            if (inputDirection == Vector2.down)
            {
                swingRotation = Quaternion.Euler(0, 0, -90); // Swing down
            }
            else if (inputDirection == Vector2.up)
            {
                swingRotation = Quaternion.Euler(0, 0, 90); // Swing up
            }
            else if (inputDirection == Vector2.left)
            {
                swingRotation = Quaternion.Euler(0, 180, 0); // Swing left
            }
            // Default (Vector2.right) rotation is already handled

            // Instantiate the swing object with the determined rotation
            GameObject swingeffect = Instantiate(swingObject, transform.position, swingRotation);
            swingeffect.transform.parent = this.transform;

            // Reset isSwinging after a delay (to be set in animation or coroutine)
            StartCoroutine(ResetSwing());
        }
    }

    private IEnumerator ResetSwing()
    {
        yield return new WaitForSeconds(0.2f); // Adjust this delay to match your swing animation
        isSwinging = false;
    }


    #endregion

    #region Collison Checks

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(feetCol.bounds.center.x, feetCol.bounds.min.y);
        Vector2 boxCastSize = new Vector2(feetCol.bounds.size.x, MoveStats.GroundDetectionRayLength);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);
        if (groundHit.collider != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(feetCol.bounds.center.x, bodyCol.bounds.max.y);
        Vector2 boxCastSize = new Vector2(feetCol.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLength);

        headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        if (headHit.collider != null)
        {
            bumpedHead = true;
        }
        else
        {
            bumpedHead = false;
        }
    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }

    #endregion
}