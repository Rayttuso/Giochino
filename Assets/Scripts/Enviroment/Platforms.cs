using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Platforms : MonoBehaviour{
    private bool isOnPlatform;

    void Update(){
        if(Input.GetKey(KeyCode.S) && isOnPlatform==true){
            transform.GetComponent<PlatformEffector2D>().rotationalOffset = 180;
        }

        else{
            transform.GetComponent<PlatformEffector2D>().rotationalOffset = 0;
            isOnPlatform=false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision){
        if(collision.gameObject.tag=="Player"){
            isOnPlatform=true;
        }
    }
}
