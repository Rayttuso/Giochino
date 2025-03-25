using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public PlayerMovimentStats MoveStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;

    private Rigidbody2D _rb;

    //movement vars
    private Vector2 _moveVelocity;  
    private bool _isFacingRight;

    //collision check vars
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    void Awake()
    {
        _isFacingRight=true;

        _rb=GetComponent<Rigidbody2D>();
    }

    #region movement

    private void Move(float acceleratione , float decelleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            targeVelocity = new Vector2(moveInput.x, 0f)
        }
    }




}

