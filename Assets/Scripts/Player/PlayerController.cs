using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Netcode.Components;

public class PlayerController : NetworkBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D feetCol;
    [SerializeField] private Collider2D bodyCol;

    private Rigidbody2D rb;
    private NetworkRigidbody2D rb2;

    //movement vars;
    private Vector2 moveVelocity;
    [HideInInspector]
    public bool isFacingRight;
    private bool isFacingUp;
    private bool isFacingDown;
    public bool isOnRope;

    //collision check vars
    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;
    private bool isGrounded;
    private bool bumpedHead;

    //Hitting
    public bool isSwinging;
    public GameObject swingObjectPrefab;
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

    //Character Selection
    public NetworkVariable<bool> isScout = new NetworkVariable<bool>();
    public GameObject RopeAbility;
    public GameObject LightAbility;
    public int AbilityUses = 3;
    private bool isCharging = false;
    public GameObject CirlceAnim;

    public LayerMask RopeLayer;

    private void Awake()
    {
        isFacingRight = true;
        rb = GetComponent<Rigidbody2D>();
        rb2 = GetComponent<NetworkRigidbody2D>();
        CirlceAnim.SetActive(false);
    }

    private void Update()
    {
                if (IsLocalPlayer)
        {
            isScout.Value = true;
        }
        else
        {
            isScout.Value = false;
        }
        CountTimers();
        JumpChecks();
        AbilityCheck();
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
        if (isJumping)
        {
            if (bumpedHead)
            {
                isFastFalling = true;
            }
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

                else
                {
                    if (!isOnRope)
                    {
                        VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                        if (isPastApexThreshold)
                        {
                            isPastApexThreshold = false;
                        }
                    }
                }
            }


            else if (!isFastFalling && !isOnRope)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }

            else if (VerticalVelocity < 0f)
            {
                if (!isFalling && !isOnRope)
                {
                    isFalling = true;
                }
            }


        }
        if (isFastFalling && !isOnRope)
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

        if (!isGrounded && !isJumping && !isOnRope)
        {
            if (isFalling)
            {
                isFalling = true;
            }

            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }

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

        if (!isGrounded)
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
                if (isOnRope)
                {
                    targetVelocity = new Vector2(moveInput.x, moveInput.y) * MoveStats.MaxRunSpeed;
                }
                else
                {
                    targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
                }
            }
            else
            {
                if (isOnRope)
                {
                    targetVelocity = new Vector2(moveInput.x, moveInput.y) * MoveStats.MaxWalkSpeed;
                }
                else
                {
                    targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
                }
            }

            moveVelocity = Vector2.Lerp(moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            if (!isOnRope)
            {
                rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(moveVelocity.x, moveVelocity.y);
            }
        }
        else if (moveInput == Vector2.zero)
        {
            moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            if (!isOnRope)
            {
                rb.linearVelocity = new Vector2(moveVelocity.x, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(moveVelocity.x, moveVelocity.y);
            }
        }

    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (isFacingRight && moveInput.x < 0)
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

            Vector2 inputDirection = InputManager.Movement.normalized;
            Quaternion swingRotation = Quaternion.identity;

            if (inputDirection == Vector2.zero)
            {
                inputDirection = isFacingRight ? Vector2.right : Vector2.left;
            }

            if (inputDirection == Vector2.down)
            {
                swingRotation = Quaternion.Euler(0, 0, -90);
            }
            else if (inputDirection == Vector2.up)
            {
                swingRotation = Quaternion.Euler(0, 0, 90);
            }
            else if (inputDirection == Vector2.left)
            {
                swingRotation = Quaternion.Euler(0, 180, 0);
            }

            SpawnSwingObjectServerRpc(swingRotation);
            StartCoroutine(ResetSwing());
        }
    }

    [Rpc(SendTo.Server)]
    private void SpawnSwingObjectServerRpc(Quaternion swingRotation)
    {
        GameObject swingEffect = Instantiate(swingObjectPrefab, transform.position, swingRotation);
        if (!isScout.Value)
        {
            swingEffect.GetComponentInChildren<SwingAttack>().damage = 2;
        }
        else
        {
            swingEffect.GetComponentInChildren<SwingAttack>().damage = 1;
        }

        NetworkObject networkObject = swingEffect.GetComponent<NetworkObject>();
        networkObject.Spawn();

        //swingEffect.transform.parent = this.transform;
    }

    private IEnumerator ResetSwing()
    {
        yield return new WaitForSeconds(0.2f);
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
        if (headHit.collider != null && headHit.collider.GetComponent<DestructibleTile>() != null)
        {
            bumpedHead = true;
        }
        else
        {
            bumpedHead = false;
        }
    }

    private void IsOnRope()
    {
        Vector2 boxCastOrigin = new Vector2(bodyCol.bounds.center.x, bodyCol.bounds.min.y);
        Vector2 boxCastSize = new Vector2(bodyCol.bounds.size.x, bodyCol.bounds.size.y);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.zero, 0.5f, RopeLayer);
        if (groundHit.collider != null)
        {
            isOnRope = true;
        }
        else
        {
            isOnRope = false;
        }
    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
        IsOnRope();
    }

    #endregion

    #region Abilitys

    public void AbilityCheck()
    {
        if (InputManager.AbilityWasPressed && AbilityUses > 0)
        {
            PerformAbilityClientRpc();
        }
    }

    public IEnumerator RechargeAbility()
    {
        CirlceAnim.SetActive(true);
        CirlceAnim.GetComponent<Animator>().SetTrigger("Ability");
        isCharging = true;
        if (isScout.Value)
        {
            yield return new WaitForSeconds(3f);
        }
        else
        {
            yield return new WaitForSeconds(10f);
        }
        AbilityUses++;
        CirlceAnim.SetActive(false);
        isCharging = false;
        if (!isScout.Value)
        {
            if (AbilityUses < 3)
            {
                if (!isCharging)
                {
                    StartCoroutine(RechargeAbility());
                }
            }
        }
        else
        {
            if (AbilityUses < 1)
            {
                if (!isCharging)
                {
                    StartCoroutine(RechargeAbility());
                }
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void PerformAbilityClientRpc()
    {
        AbilityUses--; ;
        if (!isScout.Value)
        {
            GameObject effect = Instantiate(RopeAbility, transform.position, Quaternion.identity);
            effect.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            GameObject effect = Instantiate(LightAbility, transform.position, Quaternion.identity);
            effect.GetComponent<NetworkObject>().Spawn();
            if (isFacingRight)
            {
                effect.GetComponent<Rigidbody2D>().AddForce(new Vector2(0.65f, 0.65f) * 5f, ForceMode2D.Impulse);
            }
            else
            {
                effect.GetComponent<Rigidbody2D>().AddForce(new Vector2(-0.65f, 0.65f) * 5f, ForceMode2D.Impulse);
            }
            effect.GetComponent<Rigidbody2D>().AddTorque(Random.Range(-0.35f, 0.35f), ForceMode2D.Impulse);
        }
        if (!isCharging)
        {
            StartCoroutine(RechargeAbility());
        }
    }

    #endregion
}