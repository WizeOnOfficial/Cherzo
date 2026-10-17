using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class InventoryPickupAcquisition : MonoBehaviour
    {
        [SerializeField] private InventoryCompatibilityAdapter inventoryAdapter;
        [SerializeField] private int amount = 1;

        public bool TryAcquire(InventoryItemData itemDefinition)
        {
            if (itemDefinition == null)
            {
                Debug.LogError($"{nameof(InventoryPickupAcquisition)} received a null item definition.", this);
                return false;
            }

            if (inventoryAdapter == null)
            {
                Debug.LogError($"{nameof(InventoryPickupAcquisition)} requires an InventoryCompatibilityAdapter reference.", this);
                return false;
            }

            var result = inventoryAdapter.AddItem(itemDefinition.id, amount);
            if (result != InventoryMutationResult.Success)
            {
                Debug.LogError(
                    $"Could not acquire item '{itemDefinition.id}'. Inventory result: {result}.",
                    this);
                return false;
            }

            var currentAmount = inventoryAdapter.CountItem(itemDefinition.id);
            Debug.Log(
                $"Inventory pickup succeeded. Item: '{itemDefinition.id}', Added: {amount}, Current amount: {currentAmount}.",
                this);
            return true;
        }

        private void Awake()
        {
            if (amount <= 0)
            {
                Debug.LogError($"{nameof(InventoryPickupAcquisition)} amount must be greater than zero.", this);
            }
        }
    }
}
