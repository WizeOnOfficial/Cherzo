using DefaultNamespace;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI interactionText;
    [SerializeField] float interactionDistance = 5f;

    IInteractable currentTargetedInteractable;

    public void Update() 
    {
        UpdateCurrentInteractable();

        UpdateInteractionText();

        CheckForInteractionInput();
    }

    void UpdateCurrentInteractable()
    {
        var ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));

        Physics.Raycast(ray, out var hit, interactionDistance); 

        currentTargetedInteractable = hit.collider?.GetComponent<IInteractable>();
    }

    void UpdateInteractionText()
    {
        if(currentTargetedInteractable == null)
        {
            interactionText.text = string.Empty;
            return;
        } 
        interactionText.text = currentTargetedInteractable.InteractMsg;
    }

    void CheckForInteractionInput()
    {
        if(Keyboard.current.eKey.wasPressedThisFrame && currentTargetedInteractable != null)
        {
            currentTargetedInteractable.Interact(this);
        }
    }
}   
