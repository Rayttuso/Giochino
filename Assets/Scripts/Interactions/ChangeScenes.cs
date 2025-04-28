using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ChangeScenes : MonoBehaviour{
    public int sceneBuildIndex;
    public Vector3 position;

    private void OnTriggerEnter2D(Collider2D other){
        if (other.tag=="Player"){
            SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);
        }
    }
}
