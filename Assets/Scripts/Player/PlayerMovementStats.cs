using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Moviment")]

public class PlayerMovementStats : ScriptableObject
{
    [Header("Walk")]
    [Range(0f, 1f)] public float MoveTheshold = 0.25f;
    [Range(1f,100f)] public float MaxWalkSpeed = 12.5f;
    [Range(0.25f,50f)] public float GroundAcceleration = 5f;
    [Range(0.25f,50f)] public float GroundDecelaration = 20f;
    [Range(0.25f,50f)] public float AirAcceleration = 5f;
    [Range(0.25f,50f)] public float AirDecelaration = 5f;
    [Range(0.25f,50f)] public float WallJumpMoveAcelaration = 5f;
    [Range(0.25f,50f)] public float WallJumpMoveDecelaration = 5f;

    [Header("Run")]
    [Range(1f,100f)] public float MaxRunSpeed = 12.5f;

    [Header("Grounded/Collision Checks")]
    public LayerMask GroundLayer;
    public float GroundDetectionRayLenght = 0.02f;
    public float HeadDetectionRayLength = 0.02f;
    [Range(0f,1f)] public float HeadWidth = 0.75f;
    public float WallDetectionRayLength=0.125f;
    [Range(0.01f,2f)] public float WallDetectionRayHeightMultiplier = 0.9f;



    [Header("Jump")]
    public float JumpHeight = 6.5f;
    [Range(1f,1.1f)] public float JumpheightCompensationFactor = 1.054f; 
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1,5)] public int NumberOfJumpsAllowed = 2;
    [Header("Reset Jump Option")]
    public bool ResetJumpsOnWallSide=true;

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.27f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;

    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.125f;

    [Header("Jump Coyote time")]
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;
     
    [Header("Wall Slide")]
    [Min(0.01f)] public float WallSlideSpeed=5f;
    [Range(0.25f,50f)] public float WallSlideDecelerationSpeed=50f;

    [Header("Wall Jump")]
    public Vector2 WallJumpDirection= new Vector2(-20f,6.5f);
    [Range(0f,1f)] public float WallJumpPostBufferTime =0.125f;
    [Range(0.01f,5f)] public float WallJumpGravityOnReleaseMultiplier=1f;

    [Header("Dash")]
    [Range(0f,1f)] public float DashTime =0.11f;
    [Range(1f,200f)] public float DashSpeed =40f;
    [Range(0f,1f)] public float TimeBtwDashesOnGround =0.255f;
    public bool ResetDashOnWallSlide=true;
    [Range(0,5)] public int NumberOfDashes=2;
    [Range(0f,0.5f)]public float DashDiagonallyBias=0.4f;

    [Header("Dash Cancel Time")]
    [Range(0.01f,5f)] public float DashGravityOnReleaseMultiplier=1f;
    [Range(0.02f,0.3f)] public float DashtimeForUpwardsCancel=0.027f;

    [Header("Debug")]
    public bool DebugShowIsGroundedBox = false;
    public bool DebugShowHeadBumpBox= false;
    public bool DebugShowWallHiotBox= false;

    [Header("Jump visualization tool")]
    public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int VisualizationSteps = 90;

    public readonly Vector2[] DashDirections=new Vector2[]{
        new Vector2(0, 0), //nothing   
        new Vector2(1, 0), // right 
        new Vector2(1, 1).normalized, //TOP-Right 
        new Vector2(0, 1), //Up
        new Vector2(-1, 1).normalized, //TOP-Left
        new Vector2(-1, 0), //Left
        new Vector2(-1, -1).normalized, //BOTTOM-Left   
        new Vector2(0, -1), // Down 
        new Vector2(1, -1).normalized, //BOTTOM-Left   
    };

    public float Gravity{ get; private set; }
    public float InitialjumpVelocity{ get; private set; }
    public float AdjustedJumpheight{ get; private set; }

    public float WallJumpGravity{ get; private set; }
    public float InitialWalljumpVelocity{ get; private set; }
    public float AdjustedWallJumpheight{ get; private set; }



    private void OnValidate()
    {
        CalculateValues();
    }

    void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {    //jump
        AdjustedJumpheight = JumpHeight * JumpheightCompensationFactor;
        Gravity = -(2f * JumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        InitialjumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
        // wall jump
        AdjustedJumpheight = WallJumpDirection.y * JumpheightCompensationFactor;
        WallJumpGravity = -(2f* AdjustedWallJumpheight) / Mathf.Pow(TimeTillJumpApex,2f);
        InitialWalljumpVelocity =  Mathf.Abs(WallJumpGravity) *TimeTillJumpApex;


    }
}
