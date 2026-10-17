using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class VirtualTerminalCommandParser : MonoBehaviour
    {
        private const string HelpOutput =
            "Available commands:\n" +
            "  help\n" +
            "  help chmod\n" +
            "  ls -l\n" +
            "  cat id_card\n" +
            "  chmod u=rwx id_card\n" +
            "  clear";

        private const string ChmodHelpOutput =
            "chmod changes virtual file permissions.\n" +
            "u selects the owner. r = read, w = write, x = execute.\n" +
            "Required target: chmod u=rwx id_card";

        private const string UnsupportedCommandOutput =
            "command not found or unsupported. Type 'help' for available commands.";

        [SerializeField] private GameplayEventHub eventHub;
        [SerializeField] private Quest2ObjectiveTracker quest2Tracker;

        public string PermissionValue { get; private set; } = QuestGameplayIds.GuestPermission;
        public string RoleId { get; private set; } = QuestGameplayIds.GuestRole;
        public bool HasModifiedAccessRole { get; private set; }

        public bool TryExecuteCommand(string command, out string output)
        {
            output = string.Empty;

            if (eventHub == null || quest2Tracker == null)
            {
                output = "Terminal is unavailable: required Quest 2 references are missing.";
                Debug.LogError(output, this);
                return false;
            }

            if (!quest2Tracker.HasConnectedHardware)
            {
                output = "Access hardware must be connected before using this terminal.";
                return false;
            }

            if (command == null)
            {
                output = "Invalid input. Type 'help' for available commands.";
                return false;
            }

            switch (command)
            {
                case "help":
                    output = HelpOutput;
                    return true;
                case "help chmod":
                    output = ChmodHelpOutput;
                    return true;
                case "ls -l":
                    output = $"-{PermissionValue} 1 {RoleId} id_card";
                    return true;
                case "cat id_card":
                    output = $"id_card\npermission: {PermissionValue}\nrole: {RoleId}";
                    return true;
                case "chmod u=rwx id_card":
                    return ApplyAdministratorRole(out output);
                case "clear":
                    output = string.Empty;
                    return true;
                default:
                    output = UnsupportedCommandOutput;
                    return false;
            }
        }

        private bool ApplyAdministratorRole(out string output)
        {
            if (HasModifiedAccessRole)
            {
                output = "id_card is already configured as Control Room Administrator.";
                return false;
            }

            PermissionValue = QuestGameplayIds.ControlRoomAdministratorPermission;
            RoleId = QuestGameplayIds.ControlRoomAdministratorRole;
            HasModifiedAccessRole = true;
            eventHub.PublishAccessRoleModified(
                QuestGameplayIds.IdCardAccessSubject,
                PermissionValue,
                RoleId);

            output = "id_card permissions updated: rwx";
            Debug.Log("Virtual ID Card role updated to Control Room Administrator.", this);
            return true;
        }

        private void Awake()
        {
            if (eventHub == null)
            {
                Debug.LogError(
                    $"{nameof(VirtualTerminalCommandParser)} requires a GameplayEventHub reference.",
                    this);
            }

            if (quest2Tracker == null)
            {
                Debug.LogError(
                    $"{nameof(VirtualTerminalCommandParser)} requires a Quest 2 objective tracker reference.",
                    this);
            }
        }
    }
}
