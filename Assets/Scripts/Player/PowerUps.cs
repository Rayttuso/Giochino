using UnityEngine;

public class PowerUps : MonoBehaviour
{
    public PlayerMovementStats Movement;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && this.gameObject.tag == "DoubleJump")
        {
            Movement.NumberOfJumpsAllowed = 2;
        }
    }
}

    
