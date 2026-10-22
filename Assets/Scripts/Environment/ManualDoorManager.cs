using UnityEngine;
using GameDevelopmentProject.Animation;

namespace GameDevelopmentProject.Environment
{
    /// <summary>
    /// Automatically manages all facility doors across the scene.
    /// Ensures quest doors slide to the left (-1.6m), and equips all non-quest doors
    /// with ManualDoorInteraction (Press [E] to Buka / Tutup) and solid colliders.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-45)]
    public class ManualDoorManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Vector3 slideLeftOffset = new Vector3(-1.6f, 0f, 0f);
        [SerializeField] private float slideDuration = 1.2f;

        private void Awake()
        {
            SetupAllDoors();
        }

        [ContextMenu("Setup All Doors Now")]
        public void SetupAllDoors()
        {
            var rootPintu = GameObject.Find("Pintu");
            if (rootPintu == null) return;

            int manualDoorsSetup = 0;
            int questDoorsUpdated = 0;

            foreach (Transform doorTf in rootPintu.transform)
            {
                if (doorTf == null) continue;
                string dName = doorTf.name;

                // Ensure SlidingDoorController exists and is configured to slide left
                var sdc = doorTf.GetComponent<SlidingDoorController>();
                if (sdc == null)
                {
                    sdc = doorTf.gameObject.AddComponent<SlidingDoorController>();
                }
                sdc.SetSlideDuration(slideDuration);
                sdc.SetOpenOffset(slideLeftOffset);

                // Ensure solid BoxCollider exists
                var col = doorTf.GetComponent<Collider>();
                if (col == null)
                {
                    var box = doorTf.gameObject.AddComponent<BoxCollider>();
                    box.size = new Vector3(0.015513732f, 0.028852351f, 0.020000003f);
                    box.center = new Vector3(0f, 0.0025441863f, 0f);
                    box.isTrigger = false;
                }

                // If not a quest-restricted door (PintuMasuk & PintuConPan), equip with ManualDoorInteraction
                bool isQuestDoor = dName == "PintuMasuk" || dName == "PintuConPan";
                if (!isQuestDoor)
                {
                    var manual = doorTf.GetComponent<ManualDoorInteraction>();
                    if (manual == null)
                    {
                        manual = doorTf.gameObject.AddComponent<ManualDoorInteraction>();
                    }

                    string friendlyName = GetFriendlyDoorName(dName);
                    manual.Initialize(friendlyName, slideLeftOffset);
                    manualDoorsSetup++;
                }
                else
                {
                    questDoorsUpdated++;
                }
            }

            Debug.Log($"[ManualDoorManager] Setup {manualDoorsSetup} manual doors and updated {questDoorsUpdated} quest doors to slide left.");
        }

        private static string GetFriendlyDoorName(string rawName)
        {
            switch (rawName)
            {
                case "PintuJanitor": return "Pintu Janitor";
                case "PintuITRoom": return "Pintu IT Center";
                case "PintuLab": return "Pintu Lab";
                case "PintuRoom2": return "Pintu Meeting Room";
                case "PintuConRoom": return "Pintu Control Room";
                default: return "Pintu";
            }
        }
    }
}
