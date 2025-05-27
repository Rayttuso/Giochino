using System.Collections;
using TMPro;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    public NewScriptableObjectScript testo;

    public TextMeshProUGUI textBox;

    public float displayTime=2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if(collision.CompareTag("Player")){
            textBox.text = testo.testo;
            textBox.enabled = true;
            Destroy(gameObject);
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
