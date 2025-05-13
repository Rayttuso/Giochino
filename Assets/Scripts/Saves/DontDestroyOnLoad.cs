using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DontDestroyonLoad : MonoBehaviour{
    void Awake(){
        DontDestroyOnLoad(this.gameObject);
    }

}
