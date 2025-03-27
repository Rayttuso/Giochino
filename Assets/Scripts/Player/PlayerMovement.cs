using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class PlayerMoviment : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyoll;

    private Rigidbody2D _rb;

    //moviment var
    //private Vector2 _moveVelocity;
    private float _moveVelocityX;
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
    private bool _useWallMoveStats;
    private bool _isWallJumping;
    private float _wallJumpTime;
    private bool _isWallJumpFastFalling;
    private bool  _isWallJumpFalling;
    private float _wallJumpFastFallTime;
    private float _wallJumpFastFallReleaseSpeed;

    private float _wallJumpPostBuffertimer; 
    private float _wallJumpApexPoint;
    private float _timePastWalljumpApexThreshold;
    private bool _isPastWallJumpApexThreshold;  

    private bool _isDashing;
    private bool _isAirDashing;
    private float _dashTimer;
    private float _dashOnGroundTimer;
     private int _numberOfDashesUsed;
    private Vector2 _dashDirection;
    private bool _IsDashFastFalling;
    private float _dashFastFallTime;
    private float _dashFastFaulReLeaseSpeedi;


    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CountTimers();
        JumpChecks();
        LandCheck();
    }

    void FixedUpdate()
    {
        Debug.Log(InputManager.Moviment);
        CollisionChecks();
        Jump();
        Fall();

        if(_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDecelaration, InputManager.Moviment);
        }
        else{
            Move(MoveStats.AirAcceleration, MoveStats.AirDecelaration, InputManager.Moviment);
        }
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

    #region Moviment

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        //DA RIGUARDARE LINEARVELOCITY SOVRASCRIVE SALTO
        // if(moveInput != Vector2.zero)
        // {
        //     TurnCheck(moveInput);

        //     Vector2 targetVeocity = Vector2.zero;
        //     if(InputManager.RunIsHeld)
        //     {
        //         targetVeocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;

        //     }else
        //     {
        //         targetVeocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
        //     }

        //     _moveVelocity = Vector2.Lerp(_moveVelocity, targetVeocity, acceleration * Time.fixedDeltaTime);
        //     _rb.linearVelocity = new Vector2(_moveVelocity.x, _moveVelocity.y);
        // }
        // else if(moveInput == Vector2.zero)
        // {
        //     _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        //     _rb.linearVelocity = new Vector2(_moveVelocity.x, _moveVelocity.y);
        // }
        if(moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if(InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f).normalized * MoveStats.MaxRunSpeed;

            }else
            {
                targetVelocity = new Vector2(moveInput.x, 0f).normalized * MoveStats.MaxWalkSpeed;
            }

            _moveVelocityX = Mathf.Lerp(_moveVelocityX, targetVelocity.x, acceleration * Time.fixedDeltaTime);
            _rb.linearVelocityX = _moveVelocityX;
        }
        else if(moveInput == Vector2.zero)
        {
            _moveVelocityX = Mathf.Lerp(_moveVelocityX, 0, deceleration * Time.fixedDeltaTime);
            _rb.linearVelocityX = _moveVelocityX;
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
        if((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f )
        {   
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;
        }
    } 
    private void Fall()
    {
        //NORMAL GRAVITY WHILE FALLING
        if(!_isGrounded && !_isJumping)
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

    private void JumpChecks()
    {
        //WHEN PRESS JUMP BUTTON
        if(InputManager.JumpWasPressed)
        {
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
        else if(_jumpBufferTime > 0f && _isJumping && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {    //IF PRESS JUMP AND JUMPING AND HAVE JUMPS LEFT JUMP
            InitiateJump(1);
            _isFastFalling = false;
        }

        //AIR JUMP AFTER COYOTE TIME LAPSED
        if(_jumpBufferTime > 0f && _isFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
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
        
        _jumpBufferTime = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = MoveStats.InitialjumpVelocity;
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
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialjumpVelocity, 0f, VerticalVelocity);
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
                else
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
                if(_isFalling)
                {
                    _isFalling = true;
                }
            }
        }

        //JUMP CUT(Bounce if press jump before landing)
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

        

        //CLAMP FALL SPEED
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);

        //DA RIGUARDARE LINEARVELOCITY SOVRASCRIVE CAMMINATA
        //_rb.linearVelocity = new Vector2(_rb.linearVelocity.x, VerticalVelocity);
        _rb.linearVelocityY = VerticalVelocity;
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
        Vector2 velocity = new Vector2(speed, MoveStats.InitialjumpVelocity);

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

    private void isGrounded()
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
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodyoll.bounds.max.y);
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

    private void CollisionChecks()
    {
        isGrounded();
        BumpedHead();
    }
    #endregion

    #region Timers

    private void CountTimers(){
        _jumpBufferTime -= Time.deltaTime;
        if(!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else{ _coyoteTimer = MoveStats.JumpCoyoteTime; }
    }

    #endregion
}
