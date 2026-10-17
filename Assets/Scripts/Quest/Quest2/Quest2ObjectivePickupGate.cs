using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class Quest2ObjectivePickupGate : PickupAcquisitionGate
    {
        [SerializeField] private Quest2ObjectiveTracker quest2Tracker;

        public override string DeniedPrompt => "Complete the Control Room role denial first";

        public override bool CanAcquire(out string denialReason)
        {
            if (quest2Tracker == null)
            {
                denialReason = "Quest 2 objective tracker is not configured.";
                return false;
            }

            if (!quest2Tracker.HasReceivedRoleDenial)
            {
                denialReason = "Control Room role denial is required first.";
                return false;
            }

            denialReason = null;
            return true;
        }

        private void Awake()
        {
            if (quest2Tracker == null)
            {
                Debug.LogError(
                    $"{nameof(Quest2ObjectivePickupGate)} requires a Quest 2 objective tracker reference.",
                    this);
            }
        }
    }
}
