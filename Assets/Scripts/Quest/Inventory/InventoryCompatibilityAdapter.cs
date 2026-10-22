using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class InventoryCompatibilityAdapter : MonoBehaviour, IInventoryQuery, IInventoryCommand
    {
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private GameplayEventHub eventHub;
        [SerializeField] private InventoryItemData[] itemDefinitions;

        public bool HasItem(string itemId, int requiredAmount = 1)
        {
            return CountItem(itemId) >= requiredAmount;
        }

        public int CountItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                Debug.LogError($"{nameof(InventoryCompatibilityAdapter)} received an empty item ID.", this);
                return 0;
            }

            if (inventorySystem == null)
            {
                Debug.LogError($"{nameof(InventoryCompatibilityAdapter)} has no InventorySystem reference.", this);
                return 0;
            }

            if (inventorySystem.inventory == null)
            {
                Debug.LogError("InventorySystem inventory is not initialized.", inventorySystem);
                return 0;
            }

            var total = 0;

            foreach (var inventoryItem in inventorySystem.inventory)
            {
                if (inventoryItem == null || inventoryItem.Data == null)
                {
                    continue;
                }

                if (inventoryItem.Data.id == itemId)
                {
                    total += inventoryItem.StackSize;
                }
            }

            return total;
        }

        public InventoryMutationResult AddItem(string itemId, int amount = 1)
        {
            if (!TryValidateMutation(itemId, amount, out var itemDefinition, out var result))
            {
                return result;
            }

            if (inventorySystem.inventory == null)
            {
                Debug.LogError("InventorySystem inventory is not initialized.", inventorySystem);
                return InventoryMutationResult.NotInitialized;
            }

            for (var index = 0; index < amount; index++)
            {
                inventorySystem.Add(itemDefinition);
            }

            eventHub.PublishItemAdded(itemId, amount);
            return InventoryMutationResult.Success;
        }

        public InventoryMutationResult RemoveItem(string itemId, int amount = 1)
        {
            if (!TryValidateMutation(itemId, amount, out var itemDefinition, out var result))
            {
                return result;
            }

            if (inventorySystem.inventory == null)
            {
                Debug.LogError("InventorySystem inventory is not initialized.", inventorySystem);
                return InventoryMutationResult.NotInitialized;
            }

            if (CountItem(itemId) < amount)
            {
                return InventoryMutationResult.InsufficientAmount;
            }

            for (var index = 0; index < amount; index++)
            {
                inventorySystem.Remove(itemDefinition);
            }

            eventHub.PublishItemRemoved(itemId, amount);
            return InventoryMutationResult.Success;
        }

        private void Awake()
        {
            if (inventorySystem == null)
            {
                Debug.LogError($"{nameof(InventoryCompatibilityAdapter)} requires an InventorySystem reference.", this);
            }

            if (eventHub == null)
            {
                Debug.LogError($"{nameof(InventoryCompatibilityAdapter)} requires a GameplayEventHub reference.", this);
            }
        }

        private bool TryValidateMutation(
            string itemId,
            int amount,
            out InventoryItemData itemDefinition,
            out InventoryMutationResult result)
        {
            itemDefinition = null;
            result = InventoryMutationResult.Success;

            if (string.IsNullOrWhiteSpace(itemId))
            {
                Debug.LogError($"{nameof(InventoryCompatibilityAdapter)} received an empty item ID.", this);
                result = InventoryMutationResult.InvalidItem;
                return false;
            }

            if (amount <= 0)
            {
                Debug.LogError($"{nameof(InventoryCompatibilityAdapter)} received an invalid amount.", this);
                result = InventoryMutationResult.InvalidAmount;
                return false;
            }

            if (inventorySystem == null)
            {
                Debug.LogError($"{nameof(InventoryCompatibilityAdapter)} has no InventorySystem reference.", this);
                result = InventoryMutationResult.NotInitialized;
                return false;
            }

            if (eventHub == null)
            {
                Debug.LogError($"{nameof(InventoryCompatibilityAdapter)} has no GameplayEventHub reference.", this);
                result = InventoryMutationResult.NotInitialized;
                return false;
            }

            if (itemDefinitions == null)
            {
                Debug.LogError($"{nameof(InventoryCompatibilityAdapter)} has no item definitions configured.", this);
                result = InventoryMutationResult.InvalidItem;
                return false;
            }

            foreach (var definition in itemDefinitions)
            {
                if (definition != null && definition.id == itemId)
                {
                    itemDefinition = definition;
                    return true;
                }
            }

            Debug.LogError($"No InventoryItemData is configured for item ID '{itemId}'.", this);
            result = InventoryMutationResult.MissingItem;
            return false;
        }
    }
}
