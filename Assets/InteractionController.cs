using System.Collections;
using TMPro;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public NewScriptableObjectScript testo;

    public TextMeshProUGUI textBox;

    public float displayTime=2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if(collision.CompareTag("Player")){
            textBox.text = testo.testo;
            textBox.enabled = true;
            WaitForDisappear();
  
        }
        
    }

    IEnumerator WaitForDisappear(){
        yield return new WaitForSeconds(5f);
        textBox.enabled = false;
        
    }

    void Start()
    {
        textBox.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
            
    }
}
