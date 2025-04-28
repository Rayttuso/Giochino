using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Checkpoint : MonoBehaviour
{

    ObstaclesController gameController;
    private void Awake()
    {
        gameController= GameObject.FindGameObjectWithTag("Player").GetComponent<ObstaclesController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")){
            gameController.UpdateCheckpoint(transform.position);
        }
    }
}
