using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class InteractionCompletedPickupGate : PickupAcquisitionGate
    {
        [SerializeField] private HiddenMessagePrototypeInteraction hiddenMessage;

        public override string DeniedPrompt => "Inspect Hidden Message first";

        public override bool CanAcquire(out string denialReason)
        {
            if (hiddenMessage == null)
            {
                hiddenMessage = FindFirstObjectByType<HiddenMessagePrototypeInteraction>();
            }

            if (hiddenMessage != null && hiddenMessage.HasBeenFound)
            {
                denialReason = null;
                return true;
            }

            denialReason = "Hidden Message must be found first.";
            return false;
        }

        private void Awake()
        {
            if (hiddenMessage == null)
            {
                hiddenMessage = FindFirstObjectByType<HiddenMessagePrototypeInteraction>();
            }
        }
    }
}
