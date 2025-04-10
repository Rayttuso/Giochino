using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Platforms : MonoBehaviour{
    public bool isOnPlatform;
    public bool isFalling;

    void Update(){
        
        if(Input.GetKey(KeyCode.S) && isOnPlatform==true && isFalling==false){
            transform.GetComponent<PlatformEffector2D>().rotationalOffset = 180;
            isOnPlatform=false;
            isFalling=true;
            //temp.SetActive(false);
        }

        
    }

    /*
    private void OnCollisionEnter2D(Collision2D collision){
        temp=this.gameObject;

        if(this.gameObject != temp){
            temp.SetActive(true);
        }
        
    }
    */
    private void OnCollisionStay2D(Collision2D collision){

        /*
        if(collision.gameObject.GetComponentInChildren<Collider2D>().tag == "Feet" && isFalling==false){
            isOnPlatform=true;

        }
        */

        if(collision.gameObject.tag == "Player" && isFalling==false){
            isOnPlatform=true;

        }

        
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        transform.GetComponent<PlatformEffector2D>().rotationalOffset = 0;
        isFalling=false;
        isOnPlatform=false;
    }

}
