using UnityEngine;

public class InteractableItem : MonoBehaviour, IsInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        if(CanInteract()){

            return;

        }
    }
}
