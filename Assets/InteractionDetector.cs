using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IsInteractable interactableInRange = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out IsInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out IsInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            
        }
    }
}
