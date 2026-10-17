using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public abstract class PickupAcquisitionGate : MonoBehaviour
    {
        public abstract bool CanAcquire(out string denialReason);
        public abstract string DeniedPrompt { get; }
    }
}
