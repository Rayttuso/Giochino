using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 0.005f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey("d")){
            float translation = Input.GetAxis("Horizontal")*speed;
             transform.Translate(translation, 0, 0);

        }
        
        else if(Input.GetKey("a")){
            float translation = Input.GetAxis("Horizontal")*speed;
             transform.Translate(translation, 0, 0);

        }

        else if(Input.GetKey("w")){
            float translation = Input.GetAxis("Vertical")*speed;
             transform.Translate(0, translation, 0);

        }

        else if(Input.GetKey("s")){
            float translation = Input.GetAxis("Vertical")*speed;
             transform.Translate(0, translation, 0);

        }

    }
}
