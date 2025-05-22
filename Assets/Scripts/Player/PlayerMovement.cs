using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoviment : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodcoll;
    private Animator animator;

    #region Variables

    private Rigidbody2D _rb;

    //moviment var
    //private Vector2 _moveVelocity;
    private float HorizontalVelocity;
    private bool _isFacingRight;

    //collision check var
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private RaycastHit2D _wallHit;
    private RaycastHit2D _lastWallHit;
    
    private bool _isGrounded;
    private bool _bumpedHead;
    private bool _isTouchingWall;

    //Jump Vars
    public float VerticalVelocity{get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    //Apex vars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    //jump buffer vars
    private float _jumpBufferTime;
    private bool _jumpReleasedDuringBuffer;

    //coyote time
    private float _coyoteTimer;
    // wall slide
    private bool _isWallSliding;
    private bool _isWallSlideFalling;
    // wall jump
    private bool _useWallJumpMoveStats;
    private bool _isWallJumping;
    private float _wallJumpTime;
    private bool _isWallJumpFastFalling;
    private bool  _isWallJumpFalling;
    private float _wallJumpFastFallTime;
    private float _wallJumpFastFallReleaseSpeed;

    private float _wallJumpPostBufferTimer; 
    private float _wallJumpApexPoint;
    private float _timePastWalljumpApexThreshold;
    private bool _isPastWallJumpApexThreshold;  

    private bool _isDashing;
    private bool _isAirDashing;
    private float _dashTimer;
    private float _dashOnGroundTimer;
     private int _numberOfDashesUsed;
    private Vector2 _dashDirection;
    private bool _isDashFastFalling;
    private float _dashFastFallTime;
    private float _dashFastFaulReLeaseSpeedi;

    #endregion

    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        CountTimers();
        JumpChecks();
        LandCheck();
        WallSlideCheck();
        if(MoveStats.canwalljump){
            WallJumpCheck();
        }
        HandleAnimations(InputManager.Moviment);
    }

    void FixedUpdate()
    {
        CollisionChecks();
        Jump();
        Fall();
        WallSlide();
        if(MoveStats.canwalljump){
            WallJump();
        }

        if (_isGrounded)
            {
                Move(MoveStats.GroundAcceleration, MoveStats.GroundDecelaration, InputManager.Moviment);
            }
            else
            {
                //wall jumping
                if (_useWallJumpMoveStats)
                {
                    Move(MoveStats.WallJumpMoveAcelaration, MoveStats.WallJumpMoveDecelaration, InputManager.Moviment);
                }

                //airborne
                else
                {
                    Move(MoveStats.AirAcceleration, MoveStats.AirDecelaration, InputManager.Moviment);
                }
            }
        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        //CLAMP FALL SPEED
        if(!_isDashing)
        {
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);
        }

        else
        {
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -50f, 50f);
        }

        //DA RIGUARDARE LINEARVELOCITY SOVRASCRIVE CAMMINATA
        _rb.linearVelocity = new Vector2(HorizontalVelocity, VerticalVelocity);
    }

    void OnDrawGizmos()
    {
        if(MoveStats.ShowWalkJumpArc)
        {
            DrawJumpArc(MoveStats.MaxWalkSpeed, Color.white);
        }

        if(MoveStats.ShowRunJumpArc)
        {
            DrawJumpArc(MoveStats.MaxRunSpeed, Color.red);
        }
    }

    #region Animation

    void HandleAnimations(Vector2 moveInput)
    {
        if (_isGrounded && Mathf.Abs(moveInput.x) >= MoveStats.MoveTheshold)
        {
            animator.Play("RunAnimation");
        }
    }

    #endregion

    #region Moviment

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (!_isDashing)
        {
            if (Mathf.Abs(moveInput.x) >= MoveStats.MoveTheshold)
            {
                TurnCheck(moveInput);

                float targetVelocity = 0;
                if (InputManager.RunIsHeld)
                {
                    targetVelocity = moveInput.x * MoveStats.MaxRunSpeed;

                }
                else
                {
                    targetVelocity = moveInput.x * MoveStats.MaxWalkSpeed;
                }

                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
            else if (Mathf.Abs(moveInput.x) <= MoveStats.MoveTheshold)
            {
                HorizontalVelocity = Mathf.Lerp(HorizontalVelocity, 0, deceleration * Time.fixedDeltaTime);
            }
        }

    }

    private void TurnCheck(Vector2 moveInput)
    {
        if(_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if(!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if(turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f,180f,0f);
        }else
        {
            _isFacingRight = false;
            transform.Rotate(0f,-180f,0f);
        }
    }
    #endregion

    #region Land/Fall


    private void LandCheck()
    {
        //LANDED
        if((_isJumping || _isFalling || _isWallJumpFalling || _isWallJumping || _isWallSlideFalling || _isWallSliding || _isDashFastFalling) && _isGrounded && VerticalVelocity <= 0f )
        {   
            ResetJumpValues();
            StopWallSlide();
            ResetWallJumpValues();
            ResetDashValues();
            
            ResetDashes();
            ResetJumps();
            
            VerticalVelocity = Physics2D.gravity.y;


            //use in case of animations after dash that shuldnt be triggered while grouded
            if(_isDashFastFalling && _isGrounded)
            {
                return;
            }
        }
    } 
    
    private void Fall()
    {
        //NORMAL GRAVITY WHILE FALLING
        if(!_isGrounded && !_isJumping && !_isWallSliding && !_isWallJumping && !_isDashing && !_isDashFastFalling)
        {
            if(!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }
    }

    #endregion

    #region Jump

    private void ResetJumpValues()
    {
        _isJumping = false;
        _isFalling = false;
        _isFastFalling = false;
        _fastFallTime = 0f;
        _isPastApexThreshold = false;
    }

    private void ResetJumps()
    {
        _numberOfJumpsUsed = 0;
    }

    private void JumpChecks()
    {
        //WHEN PRESS JUMP BUTTON
        if(InputManager.JumpWasPressed)
        {   
            if(_isWallSliding && _wallJumpPostBufferTimer >= 0)
            { return; }
            else if(_isWallSliding || (_isTouchingWall && !_isGrounded))
            { return; }
            _jumpBufferTime = MoveStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }
        //WHEN RELEASE JUMP BUTTON
        if(InputManager.JumpWasReleased)
        {
            //IF JUMP IS RELEASED BEFORE REACHING APEX OF JUMP PLAYER FALLS
            if(_jumpBufferTime > 0f)
            {
                _jumpReleasedDuringBuffer = true;
            }

            if(_isJumping && VerticalVelocity > 0f)
            {   
                if(_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }
        //INITIATE JUMP BUFFERING AND COYOTE TIME
        if(_jumpBufferTime > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {   //IF PRESS JUMP WHILE ON GROUND OR WHILE WITHIN COYOTE TIME JUMP
            InitiateJump(1);

            if(_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        
        //DOUBLE JUMP
        else if(_jumpBufferTime > 0f && (_isJumping || _isWallJumping || _isWallSlideFalling || _isAirDashing || _isDashFastFalling) && !_isTouchingWall && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {    //IF PRESS JUMP AND JUMPING AND HAVE JUMPS LEFT JUMP
            InitiateJump(1);
            _isFastFalling = false;

            if(_isDashFastFalling)
            {
                _isDashFastFalling = false;
            }
        }

        //AIR JUMP AFTER COYOTE TIME LAPSED
        if(_jumpBufferTime > 0f && _isFalling && !_isWallSlideFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
        {   //IF JUMP ON AIR USE DOUBLE JUMP
            InitiateJump(2);
            _isFastFalling = false;
        }


    }
 
    private void InitiateJump(int numberOfJumpsUsed)
    {
        if(!_isJumping)
        {
            _isJumping = true;
        }

        ResetWallJumpValues();

        _jumpBufferTime = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = MoveStats.InitialJumpVelocity;
    }

    private void Jump()
    {
        //APPLY GRAVITY WHILE JUMPING 
        if(_isJumping)
        {
            //CHECK FOR HEAD BUMPS
            if(_bumpedHead)
            {
                _isFastFalling = true;
            }

            //GRAVITY ON ASCENDING
            if(VerticalVelocity >= 0)
            {
                //APEX CONTROLS
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);
                //if MoveStats.ApexThreshold == 1 no hang time
                if(_apexPoint > MoveStats.ApexThreshold)
                {
                    if(!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if(_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if(_timePastApexThreshold < MoveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }
            
                //GRAVITY ON ASCENDING BUT NOT PAST APEX THESHOLD
                else if(!_isFastFalling)
                {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                    if(_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }

            }

            //GRAVITY ON DECENDING
            else if(!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if(VerticalVelocity < 0f)
            {
                if(!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        //JUMP CUT
        if(_isFastFalling)
        {
            if(_fastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if(_fastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, ( _fastFallTime/ MoveStats.TimeForUpwardsCancel ));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }

    }

    #endregion

    #region WallSlide

    private void WallSlideCheck()
    {
        if(_isTouchingWall && !_isGrounded && !_isDashing)
        {
            if(VerticalVelocity < 0f && !_isWallSliding)
            {
                ResetJumpValues();
                ResetWallJumpValues();
                ResetDashValues();

                _isWallSlideFalling = false;
                _isWallSliding = true;

                if(MoveStats.ResetJumpsOnWallSide)
                {
                    ResetJumps();
                }
                if(MoveStats.ResetDashOnWallSlide)
                {
                    ResetDashes();
                }
            }
        }

        else if(_isWallSliding && !_isTouchingWall && !_isGrounded && !_isWallSlideFalling)
        {
            _isWallJumpFalling = true;
            StopWallSlide();
        }

        else
        {
            StopWallSlide();
        }
    }
    
    private void StopWallSlide()
    {
        if(_isWallSliding)
        {
            _numberOfJumpsUsed++;

            _isWallSliding = false;
        }
    }

    private void WallSlide()
    {
        if(_isWallSliding)
        {
            VerticalVelocity = Mathf.Lerp(VerticalVelocity, -MoveStats.WallSlideSpeed, MoveStats.WallSlideDecelerationSpeed * Time.deltaTime);
        }
    }

    #endregion

    #region WallJump

    private void WallJumpCheck()
    {
        if(ShouldApplyPostWallJumpBuffer())
        {
            _wallJumpPostBufferTimer = MoveStats.WallJumpPostBufferTime;
        }

        //wall jump fast falling
        if(InputManager.JumpWasReleased && !_isWallSliding && !_isTouchingWall && _isWallJumping)
        {
            if(VerticalVelocity > 0f)
            {
                if(_isPastWallJumpApexThreshold)
                {
                    _isPastWallJumpApexThreshold = false;
                    _isWallJumpFastFalling = true;
                    _wallJumpFastFallTime = MoveStats.TimeForUpwardsCancel;

                    VerticalVelocity = 0f;
                }
                else
                {
                    _isWallJumpFastFalling = true;
                    _wallJumpFastFallReleaseSpeed = VerticalVelocity; 
                }
            }
        }
        
        //actual wall jump with jump buffer time
        if(InputManager.JumpWasPressed && _wallJumpPostBufferTimer > 0f)
        {
            InitiateWallJump();
        }
    }

    private void InitiateWallJump()
    {   
        if(!_isWallJumping)
        {
            _isWallJumping = true;
            _useWallJumpMoveStats = true;
        }

        StopWallSlide();
        ResetJumpValues();
        _wallJumpTime = 0f;

        VerticalVelocity = MoveStats.InitialWallJumpVelocity;

        int dirMultiplier = 0;
        Vector2 hitPoint = _lastWallHit.collider.ClosestPoint(_bodcoll.bounds.center);

        if(hitPoint.x > transform.position.x)
        {
            dirMultiplier = -1;
        }
        else{ dirMultiplier = 1; }

        HorizontalVelocity = Mathf.Abs(MoveStats.WallJumpDirection.x) * dirMultiplier;
    }

    private void WallJump()
    {
        //APPLY WALL JUMP GRAVITY
        if(_isWallJumping)
        {
            //TIME TO TAKE OVER MOVEMENT CONTROLS
            _wallJumpTime += Time.fixedDeltaTime;
            if(_wallJumpTime >= MoveStats.TimeTillJumpApex)
            {
                _useWallJumpMoveStats = false;
            }

            //HIT HEAD
            if(_bumpedHead)
            {
                _isWallJumpFastFalling = true;
                _useWallJumpMoveStats = false;
            }

            //GRAVITY ON ACENDING
            if(VerticalVelocity >= 0f)
            {
                //APEX CONTROLS
                _wallJumpApexPoint = Mathf.InverseLerp(MoveStats.WallJumpDirection.y, 0f, VerticalVelocity);

                if(_wallJumpApexPoint > MoveStats.ApexThreshold)
                {
                    if(!_isPastWallJumpApexThreshold)
                    {
                        _isPastWallJumpApexThreshold = true;
                        _timePastWalljumpApexThreshold = 0f;
                    }

                    if(_isPastWallJumpApexThreshold)
                    {
                        _timePastWalljumpApexThreshold += Time.fixedDeltaTime;
                        if(_timePastWalljumpApexThreshold < MoveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }

                //GRAVITY ON ACENDING BUT NOT PAST APEX POINT
                else if(!_isWallJumpFastFalling)
                {
                    VerticalVelocity += MoveStats.WallJumpGravity * Time.fixedDeltaTime;

                    if(_isPastWallJumpApexThreshold)
                    {
                        _isPastWallJumpApexThreshold = false;
                    }
                }
            }

            //GRAVITY ON DECENDING
            else if(!_isWallJumpFastFalling)
            {
                VerticalVelocity += MoveStats.WallJumpGravity * Time.fixedDeltaTime;
            }

            else if(VerticalVelocity < 0f)
            {
                if(!_isWallJumpFalling)
                {
                    _isWallJumpFalling = true;
                }
            }
        }

        //HANDLE WALL JUMP CUT TIME
        if(_isWallJumpFastFalling)
        {
            if(_wallJumpFastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += MoveStats.WallJumpGravity * MoveStats.WallJumpGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if(_wallJumpFastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_wallJumpFastFallReleaseSpeed, 0f, (_wallJumpFastFallTime / MoveStats.TimeForUpwardsCancel));
            }

            _wallJumpFastFallTime += Time.fixedDeltaTime;
        }
    }

    private bool ShouldApplyPostWallJumpBuffer() 
    {
        if(!_isGrounded && (_isTouchingWall || _isWallSliding))
        {
            return true;
        }
        else{ return false; }

    }

    private void ResetWallJumpValues()
    {
        _isWallSlideFalling = false;
        _useWallJumpMoveStats = false;
        _isWallJumping = false;
        _isWallJumpFalling = false;
        _isWallJumpFastFalling = false;
        _isPastWallJumpApexThreshold = false;

        _wallJumpFastFallTime = 0f;
        _wallJumpTime = 0f;
    }

    #endregion

    #region Dash

    private void ResetDashValues()
    {
        _isDashFastFalling = false;
        _dashOnGroundTimer = -0.01f;
    }

    private void ResetDashes()
    {
        _numberOfDashesUsed = 0;
    }

    #endregion

    #region Jump Visualization

    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        Vector2 startPosition = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 previusPosition = startPosition;
        float speed = 0f;
        if(MoveStats.DrawRight)
        {
            speed = moveSpeed;
        }
        else{ speed = -moveSpeed; }
        Vector2 velocity = new Vector2(speed, MoveStats.InitialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * MoveStats.TimeTillJumpApex / MoveStats.ArcResolution; //time step for arc simulation
        //float totalTime = (2 * MoveStats.TimeTillJumpApex) + MoveStats.ApexHangTime; //total time of the arc

        for(int i = 0; i < MoveStats.VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacment;
            Vector2 drawPoint;

            if(simulationTime < MoveStats.TimeTillJumpApex) //acending
            {
                displacment = velocity * simulationTime + 0.5f * new Vector2(0, MoveStats.Gravity) * simulationTime * simulationTime;
            }
            else if(simulationTime < MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime) //apex hang time
            {
                float apexTime = simulationTime - MoveStats.TimeTillJumpApex;
                displacment = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0, MoveStats.Gravity) * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;
                displacment += new Vector2(speed, 0) * apexTime;

            }
            else //decending
            {   
                float decendTime = simulationTime - (MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime);
                displacment = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0, MoveStats.Gravity) * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;
                displacment += new Vector2(speed, 0) * MoveStats.ApexHangTime;
                displacment += new Vector2(speed, 0) * decendTime + 0.5f * new Vector2(0, MoveStats.Gravity) * decendTime * decendTime;
            }

            drawPoint = startPosition + displacment;

            if(MoveStats.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previusPosition, drawPoint - previusPosition, Vector2.Distance(previusPosition, drawPoint), MoveStats.GroundLayer);
                if(hit.collider != null)
                {
                    //if a hit is detected stop drawing  the arc at the point of hit
                    Gizmos.DrawLine(previusPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previusPosition, drawPoint);
            previusPosition = drawPoint;
        } 

    }

    #endregion

    #region Collision Checks

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveStats.GroundDetectionRayLenght);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLenght, MoveStats.GroundLayer);
        if(_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }

        #region Debug Visualization

        if(MoveStats.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if(_isGrounded)
            {
                rayColor = Color.green;
            }else{ rayColor = Color.red;}

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectionRayLenght, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - MoveStats.GroundDetectionRayLenght), Vector2.right * boxCastSize.x, rayColor);
        }

        #endregion
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodcoll.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLength);
    
        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        if(_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else{ _bumpedHead = false; }

        #region  Debug Visualization

        if(MoveStats.DebugShowHeadBumpBox)
        {
            float HeadWidth = MoveStats.HeadWidth;

            Color rayColor;
            if(_bumpedHead)
            {
                rayColor = Color.green;
            }
            else{ rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * HeadWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor );
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * HeadWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor );
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * HeadWidth, boxCastOrigin.y + MoveStats.HeadDetectionRayLength), Vector2.right * boxCastSize.x * HeadWidth, rayColor );
        }
        
        #endregion
    }

    private void IsTuchingWall()
    {
        float originEndPoint = 0f;
        if(_isFacingRight)
        {
            originEndPoint = _bodcoll.bounds.max.x;
        }
        else{ originEndPoint = _bodcoll.bounds.min.x; }

        float adjustedHeight = _bodcoll.bounds.size.y * MoveStats.WallDetectionRayHeightMultiplier;

        Vector2 boxCastOrigin = new Vector2(originEndPoint, _bodcoll.bounds.center.y);
        Vector2 boxCastSize = new Vector2(MoveStats.WallDetectionRayLength, adjustedHeight);
        
        //if we want only walls to get recognised make a variable for layer mask wall and apply
        _wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, MoveStats.WallDetectionRayLength, MoveStats.GroundLayer);

        if(_wallHit.collider != null)
        {
            _lastWallHit = _wallHit;
            _isTouchingWall = true;
        }
        else{ _isTouchingWall = false; }

        #region Debug Visualization

        if(MoveStats.DebugShowWallHitBox)
        {
            Color rayColor;
            if(_isTouchingWall)
            {
                rayColor = Color.green;
            }
            else{ rayColor = Color.red; }

            Vector2 boxBottomLeft = new Vector2(boxCastOrigin.x - boxCastSize.x/2, boxCastOrigin.y - boxCastSize.y/2);
            Vector2 boxBottomRight = new Vector2(boxCastOrigin.x + boxCastSize.x/2, boxCastOrigin.y - boxCastSize.y/2);
            Vector2 boxTopLeft = new Vector2(boxCastOrigin.x - boxCastSize.x/2, boxCastOrigin.y + boxCastSize.y/2);
            Vector2 boxTopRight = new Vector2(boxCastOrigin.x + boxCastSize.x/2, boxCastOrigin.y + boxCastSize.y/2);

            Debug.DrawLine(boxBottomLeft, boxBottomRight, rayColor);
            Debug.DrawLine(boxBottomLeft, boxTopLeft, rayColor);
            Debug.DrawLine(boxBottomRight , boxTopRight , rayColor);
            Debug.DrawLine(boxTopLeft, boxTopRight, rayColor);
        }

        #endregion

    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
        IsTuchingWall();
    }
    #endregion

    #region Timers

    private void CountTimers(){
        //jump buffer
        //(Makes player jump eaven if player presses jump before landing)
        _jumpBufferTime -= Time.deltaTime;

        //Cayote time
        if(!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else{ _coyoteTimer = MoveStats.JumpCoyoteTime; }

        //wall jump buffer time
        if(!ShouldApplyPostWallJumpBuffer())
        {
            _wallJumpPostBufferTimer -= Time.deltaTime;
        }

    }

    #endregion
}
