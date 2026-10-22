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

    public string InteractionText => interactionText != null ? interactionText.text : string.Empty;

    public void ClearInteractionText()
    {
        if (interactionText != null)
        {
            interactionText.text = string.Empty;
        }
    }

    public void SetInteractionText(string text)
    {
        if (interactionText != null)
        {
            interactionText.text = text ?? string.Empty;
        }
    }

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
        if(Mouse.current.leftButton.wasPressedThisFrame && currentTargetedInteractable != null)
        {
            currentTargetedInteractable.Interact(this);
        }
    }
}   
