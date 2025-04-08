using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Moviment")]

public class PlayerMovementStats : ScriptableObject
{
    [Header("Walk")]
    [Range(0f, 1f)] public float MovementThreshold = 0.25f;
    [Range(1f,100f)] public float MaxWalkSpeed = 12.5f;
    [Range(0.25f,50f)] public float GroundAcceleration = 5f;
    [Range(0.25f,50f)] public float GroundDecelaration = 20f;
    [Range(0.25f,50f)] public float AirAcceleration = 5f;
    [Range(0.25f,50f)] public float AirDecelaration = 5f;

    [Header("Run")]
    [Range(1f,100f)] public float MaxRunSpeed = 12.5f;

    [Header("Grounded/Collision Checks")]
    public LayerMask GroundLayer;
    public float GroundDetectionRayLenght = 0.02f;
    public float HeadDetectionRayLength = 0.02f;
    [Range(0f,1f)] public float HeadWidth = 0.75f;

    [Header("Jump")]
    public float JumpHeight = 6.5f;
    [Range(1f,1.1f)] public float JumpheightCompensationFactor = 1.054f; 
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1,5)] public int NumberOfJumpsAllowed = 2;

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.27f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;

    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.125f;

    [Header("Jump Coyote time")]
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;

    [Header("Debug")]
    public bool DebugShowIsGroundedBox = false;
    public bool DebugShowHeadBumpBox= false;

    [Header("Jump visualization tool")]
    public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int VisualizationSteps = 90;

    public float Gravity{ get; private set; }
    public float InitialjumpVelocity{ get; private set; }
    public float AdjustedJumpheight{ get; private set; }

    private void OValidate()
    {
        CalculateValues();
    }

    void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        AdjustedJumpheight = JumpHeight * JumpheightCompensationFactor;
        Gravity = -(2f * JumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        Debug.Log("Gravity " + Gravity);

        InitialjumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    }
}
