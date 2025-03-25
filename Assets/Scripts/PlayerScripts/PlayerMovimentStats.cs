using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName="Player Movement")]
public class PlayerMovimentStats : ScriptableObject
{
    [Header("Walk")] 
    [Range(1f,100f)] public float MaxWalkSpeed=12.5f;
    [Range(0.25f,50f)] public float GroundAccelleration=5f;
    [Range(0.25f,50f)] public float GroundDecelleration=20f;
    [Range(0.25f,50f)] public float AirAccelleration=5f;
    [Range(0.25f,50f)] public float AirDecelleration=5f;

    [Header("Run")] 
    [Range(1f,100f)] public float MaxRunSpeed=20f;

    [Header("Grounded/Collision Checks")] 
    public LayerMask GroundLayer;
    public float GroundDetectionRayLength=0.02f;
    public float HeadDetectionRayLength=0.02f;
    [Range(0f,1f)] public float HeadWidth=0.75f;
}
