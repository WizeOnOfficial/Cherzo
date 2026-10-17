using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class Quest2ObjectiveTracker : MonoBehaviour
    {
        [SerializeField] private GameplayEventHub eventHub;

        public bool HasRepairedElectricalShort { get; private set; }
        public bool HasReceivedRoleDenial { get; private set; }
        public bool HasFoundItGuide { get; private set; }
        public bool HasConnectedHardware { get; private set; }
        public bool HasModifiedAccessRole { get; private set; }

        private void OnEnable()
        {
            if (eventHub == null)
            {
                Debug.LogError($"{nameof(Quest2ObjectiveTracker)} requires a GameplayEventHub reference.", this);
                return;
            }

            eventHub.InteractionCompleted += HandleInteractionCompleted;
            eventHub.InteractionCompleted += HandleHardwareConnection;
            eventHub.AccessResolved += HandleAccessResolved;
            eventHub.ItemAdded += HandleItemAdded;
            eventHub.AccessRoleModified += HandleAccessRoleModified;
        }

        private void OnDisable()
        {
            if (eventHub != null)
            {
                eventHub.InteractionCompleted -= HandleInteractionCompleted;
                eventHub.InteractionCompleted -= HandleHardwareConnection;
                eventHub.AccessResolved -= HandleAccessResolved;
                eventHub.ItemAdded -= HandleItemAdded;
                eventHub.AccessRoleModified -= HandleAccessRoleModified;
            }
        }

        private void HandleInteractionCompleted(InteractionEvent interactionEvent)
        {
            if (interactionEvent.InteractionId != QuestGameplayIds.BreakerPanel ||
                interactionEvent.Result != GameplayInteractionResult.Success)
            {
                return;
            }

            if (HasRepairedElectricalShort)
            {
                return;
            }

            HasRepairedElectricalShort = true;
            Debug.Log("Quest 2 objective completed: electrical short repaired.", this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest2,
                QuestGameplayIds.Quest2RepairElectricalShort);
        }

        private void HandleAccessResolved(AccessEvent accessEvent)
        {
            if (accessEvent.AccessPointId != QuestGameplayIds.ControlRoom ||
                accessEvent.Result != GameplayInteractionResult.Denied ||
                !HasRepairedElectricalShort ||
                HasReceivedRoleDenial)
            {
                return;
            }

            HasReceivedRoleDenial = true;
            Debug.Log("Quest 2 objective completed: ID Card role denied.", this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest2,
                QuestGameplayIds.Quest2RoleDenied);
        }

        private void HandleItemAdded(ItemChangedEvent itemChanged)
        {
            if (itemChanged.ItemId != QuestGameplayIds.GuideItem || HasFoundItGuide)
            {
                return;
            }

            HasFoundItGuide = true;
            Debug.Log("Quest 2 objective completed: IT Guide found.", this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest2,
                QuestGameplayIds.Quest2FindItGuide);
        }

        private void HandleHardwareConnection(InteractionEvent interactionEvent)
        {
            if (interactionEvent.InteractionId != QuestGameplayIds.HardwareConnection ||
                interactionEvent.Result != GameplayInteractionResult.Success ||
                !HasFoundItGuide ||
                HasConnectedHardware)
            {
                return;
            }

            HasConnectedHardware = true;
            Debug.Log("Quest 2 objective completed: access hardware connected.", this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest2,
                QuestGameplayIds.Quest2ConnectHardware);
        }

        private void HandleAccessRoleModified(AccessRoleModifiedEvent roleEvent)
        {
            if (roleEvent.AccessSubjectId != QuestGameplayIds.IdCardAccessSubject ||
                roleEvent.PermissionValue != QuestGameplayIds.ControlRoomAdministratorPermission ||
                roleEvent.RoleId != QuestGameplayIds.ControlRoomAdministratorRole ||
                !HasConnectedHardware ||
                HasModifiedAccessRole)
            {
                return;
            }

            HasModifiedAccessRole = true;
            Debug.Log("Quest 2 objective completed: ID Card access role modified.", this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest2,
                QuestGameplayIds.Quest2ModifyAccessRole);
        }
    }
}
