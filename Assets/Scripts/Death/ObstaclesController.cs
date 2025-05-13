using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstaclesController : MonoBehaviour
{
    Vector3 checkpointPos;
    SpriteRenderer spriteRenderer;

    Rigidbody2D playerRb;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerRb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        checkpointPos=transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Obstacle")){
            Die();
        }
    }

    public void UpdateCheckpoint(Vector3 pos)
    {
        checkpointPos=pos;
    } 

    void Die(){
        StartCoroutine(Respawn(0.5f));
    }

    IEnumerator Respawn(float duration){
        playerRb.simulated=false;
        spriteRenderer.enabled=false;
        yield return new WaitForSeconds(duration);
        transform.position=checkpointPos;
        spriteRenderer.enabled=true;
        playerRb.simulated=true;
    }
    
}
