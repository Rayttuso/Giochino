using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D _rb;
    float speed = 5f;
    float inputHorizontal;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb=gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        inputHorizontal=Input.GetAxisRaw("Horizontal");

        if(inputHorizontal != 0){
            _rb.AddForce(new Vector2(inputHorizontal * speed, 0f));
        }
    }
}
