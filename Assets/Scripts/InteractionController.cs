using DefaultNamespace;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI interactionText;
    [SerializeField] float interactionDistance = 5f;
    [SerializeField] private InventoryUI inventoryUI;

    IInteractable currentTargetedInteractable;
    private bool isInventoryOpen;
    private float suppressInteractionUntilTime;

    public string InteractionText => interactionText != null ? interactionText.text : string.Empty;

    public void SuppressInteraction(float duration = 0.25f)
    {
        suppressInteractionUntilTime = Time.time + duration;
        currentTargetedInteractable = null;
        ClearInteractionText();
    }

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

    private void Awake()
    {
        if (inventoryUI == null)
        {
            inventoryUI = FindFirstObjectByType<InventoryUI>();
        }

        if (interactionText == null)
        {
            var texts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var t in texts)
            {
                if (t != null && t.gameObject.name.ToLower().Contains("interaction"))
                {
                    interactionText = t;
                    break;
                }
            }
        }
    }

    private void OnEnable()
    {
        if (inventoryUI == null)
        {
            inventoryUI = FindFirstObjectByType<InventoryUI>();
        }

        if (inventoryUI != null)
        {
            inventoryUI.OnInventoryOpened -= HandleInventoryOpened;
            inventoryUI.OnInventoryOpened += HandleInventoryOpened;
            inventoryUI.OnInventoryClosed -= HandleInventoryClosed;
            inventoryUI.OnInventoryClosed += HandleInventoryClosed;
        }
    }

    private void OnDisable()
    {
        if (inventoryUI != null)
        {
            inventoryUI.OnInventoryOpened -= HandleInventoryOpened;
            inventoryUI.OnInventoryClosed -= HandleInventoryClosed;
        }
    }

    private void HandleInventoryOpened()
    {
        isInventoryOpen = true;
        currentTargetedInteractable = null;
        ClearInteractionText();
    }

    private void HandleInventoryClosed()
    {
        isInventoryOpen = false;
        suppressInteractionUntilTime = Time.time + 0.25f;
        ClearInteractionText();
    }

    private void OnGUI()
    {
        if (isInventoryOpen || Cursor.visible) return;

        // Draw crosshair at center
        float chSize = 6f;
        var chRect = new Rect((Screen.width - chSize) / 2f, (Screen.height - chSize) / 2f, chSize, chSize);
        var oldColor = GUI.color;
        GUI.color = currentTargetedInteractable != null ? Color.green : Color.white;
        GUI.Box(chRect, GUIContent.none);
        GUI.color = oldColor;

        if ((interactionText == null || !interactionText.gameObject.activeInHierarchy) &&
            currentTargetedInteractable != null &&
            !string.IsNullOrWhiteSpace(currentTargetedInteractable.InteractMsg))
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = Color.yellow;
            float width = 380f;
            float height = 44f;
            var rect = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f + 50f, width, height);
            GUI.Box(rect, currentTargetedInteractable.InteractMsg, style);
        }
    }

    public void Update() 
    {
        if (isInventoryOpen || Time.time < suppressInteractionUntilTime)
        {
            currentTargetedInteractable = null;
            ClearInteractionText();
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible)
        {
            currentTargetedInteractable = null;
            ClearInteractionText();
            return;
        }

        UpdateCurrentInteractable();

        UpdateInteractionText();

        CheckForInteractionInput();
    }

    void UpdateCurrentInteractable()
    {
        if (Camera.main == null) return;
        var ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));

        currentTargetedInteractable = null;

        // 1. Precise ray first, 2. forgiving sphere (0.3m) as fallback.
        currentTargetedInteractable =
            FindFirstActiveInteractable(Physics.RaycastAll(ray, interactionDistance)) ??
            FindFirstActiveInteractable(Physics.SphereCastAll(ray, 0.3f, interactionDistance));
    }

    // Returns the nearest interactable that currently has a prompt.
    // An empty InteractMsg means "not interactable right now", so it is skipped.
    static IInteractable FindFirstActiveInteractable(RaycastHit[] hits)
    {
        if (hits == null || hits.Length == 0) return null;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null && !string.IsNullOrWhiteSpace(interactable.InteractMsg))
            {
                return interactable;
            }
        }

        return null;
    }

    void UpdateInteractionText()
    {
        if (interactionText == null) return;

        if (currentTargetedInteractable == null)
        {
            interactionText.text = string.Empty;
            return;
        } 
        interactionText.text = currentTargetedInteractable.InteractMsg;
    }

    void CheckForInteractionInput()
    {
        if (isInventoryOpen || Time.time < suppressInteractionUntilTime)
        {
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible)
        {
            return;
        }

        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        bool interactPressed = (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
                               (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (interactPressed)
        {
            if (currentTargetedInteractable != null)
            {
                currentTargetedInteractable.Interact(this);
            }
            else if (Camera.main != null)
            {
                var ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));
                if (Physics.Raycast(ray, out var hit, interactionDistance))
                {
                    Debug.Log($"[InteractionController] Raycast hit '{hit.collider.name}' (no IInteractable found on it or parent).", hit.collider);
                }
                else
                {
                    Debug.Log($"[InteractionController] Raycast did not hit any collider within {interactionDistance}m.", this);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Camera.main != null)
        {
            var ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            Gizmos.color = currentTargetedInteractable != null ? Color.green : Color.red;
            Gizmos.DrawRay(ray.origin, ray.direction * interactionDistance);
        }
    }
}   
