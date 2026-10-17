using System;

namespace GameDevelopmentProject.Quest
{
    public static class QuestGameplayIds
    {
        public const string IdCardItem = "item.id_card";
        public const string ExternalFuseItem = "item.external_fuse";
        public const string GuideItem = "item.it_guide";
        public const string HardwareItem = "item.access_hardware";
        public const string IdCardAccessSubject = "access_subject.id_card";
        public const string GuestPermission = "r--";
        public const string ControlRoomAdministratorPermission = "rwx";
        public const string GuestRole = "role.guest";
        public const string ControlRoomAdministratorRole = "role.control_room_administrator";

        public const string MaintenanceWing = "area.maintenance_wing";
        public const string MaintenanceWingAccess = "access.maintenance_wing";
        public const string ControlRoom = "access.control_room";
        public const string ControlPanel = "interaction.control_panel";
        public const string ControlPanelInspection = "interaction.control_panel_inspection";
        public const string ControlPanelFuseInstallation =
            "interaction.control_panel_fuse_installation";
        public const string HiddenMessage = "interaction.hidden_message";
        public const string BreakerPanel = "interaction.breaker_panel";
        public const string HardwareConnection = "interaction.hardware_connection";
        public const string CardReader = "interaction.control_room_card_reader";
        public const string Desktop = "interaction.it_desktop";
        public const string Quest1 = "quest.1";
        public const string Quest2 = "quest.2";
        public const string Quest1FindIdCard = "quest.1.find_id_card";
        public const string Quest1TryInitialControlPanelAccess =
            "quest.1.try_initial_control_panel_access";
        public const string Quest1InspectControlPanel = "quest.1.inspect_control_panel";
        public const string Quest1FindHiddenMessage = "quest.1.find_hidden_message";
        public const string Quest1FindExternalFuse = "quest.1.find_external_fuse";
        public const string Quest1InstallExternalFuse = "quest.1.install_external_fuse";
        public const string Quest1ScanIdCardAfterFuse = "quest.1.scan_id_card_after_fuse";
        public const string Quest1EnterMaintenanceWing = "quest.1.enter_maintenance_wing";
        public const string Quest2RepairElectricalShort = "quest.2.repair_electrical_short";
        public const string Quest2RoleDenied = "quest.2.role_denied";
        public const string Quest2FindItGuide = "quest.2.find_it_guide";
        public const string Quest2ConnectHardware = "quest.2.connect_hardware";
        public const string Quest2ModifyAccessRole = "quest.2.modify_access_role";
    }

    public enum GameplayInteractionResult
    {
        Success,
        Denied,
        MissingRequirement,
        AlreadyCompleted,
        PowerUnavailable,
        InvalidState
    }

    public enum InventoryMutationResult
    {
        Success,
        MissingItem,
        InsufficientAmount,
        InvalidItem,
        InvalidAmount,
        NotInitialized,
        Rejected
    }

    public readonly struct ItemChangedEvent
    {
        public ItemChangedEvent(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }

        public string ItemId { get; }
        public int Amount { get; }
    }

    public readonly struct InteractionEvent
    {
        public InteractionEvent(string interactionId, GameplayInteractionResult result, string reason = null)
        {
            InteractionId = interactionId;
            Result = result;
            Reason = reason;
        }

        public string InteractionId { get; }
        public GameplayInteractionResult Result { get; }
        public string Reason { get; }
    }

    public readonly struct AccessEvent
    {
        public AccessEvent(string accessPointId, GameplayInteractionResult result, string reason = null)
        {
            AccessPointId = accessPointId;
            Result = result;
            Reason = reason;
        }

        public string AccessPointId { get; }
        public GameplayInteractionResult Result { get; }
        public string Reason { get; }
    }

    public readonly struct AreaEnteredEvent
    {
        public AreaEnteredEvent(string areaId)
        {
            AreaId = areaId;
        }

        public string AreaId { get; }
    }

    public readonly struct QuestObjectiveEvent
    {
        public QuestObjectiveEvent(string questId, string objectiveId)
        {
            QuestId = questId;
            ObjectiveId = objectiveId;
        }

        public string QuestId { get; }
        public string ObjectiveId { get; }
    }

    public readonly struct AccessRoleModifiedEvent
    {
        public AccessRoleModifiedEvent(
            string accessSubjectId,
            string permissionValue,
            string roleId)
        {
            AccessSubjectId = accessSubjectId;
            PermissionValue = permissionValue;
            RoleId = roleId;
        }

        public string AccessSubjectId { get; }
        public string PermissionValue { get; }
        public string RoleId { get; }
    }

    public interface IInventoryQuery
    {
        bool HasItem(string itemId, int requiredAmount = 1);
        int CountItem(string itemId);
    }

    public interface IInventoryCommand
    {
        InventoryMutationResult AddItem(string itemId, int amount = 1);
        InventoryMutationResult RemoveItem(string itemId, int amount = 1);
    }

    public interface IGameplayEventSource
    {
        event Action<ItemChangedEvent> ItemAdded;
        event Action<ItemChangedEvent> ItemRemoved;
        event Action<InteractionEvent> InteractionCompleted;
        event Action<AccessEvent> AccessResolved;
        event Action<AreaEnteredEvent> AreaEntered;
        event Action<QuestObjectiveEvent> QuestObjectiveCompleted;
        event Action<AccessRoleModifiedEvent> AccessRoleModified;
    }
}
