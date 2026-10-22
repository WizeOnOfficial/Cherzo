using System.Collections.Generic;
using UnityEngine;
using DefaultNamespace;

namespace GameDevelopmentProject.Environment
{
    /// <summary>
    /// Automatically generates exact, fitted MeshColliders for all environment furniture,
    /// props, tables, cabinets, and machines across all facility rooms.
    /// Eliminates phantom solid walls, preserves open walkways/aisles, and ensures 100% exact
    /// collision matching the 3D model geometry without manual boilerplate or bloated scene YAML.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-50)]
    public class EnvironmentPropColliderManager : MonoBehaviour
    {
        [Header("Room Root GameObjects")]
        [SerializeField] private GameObject[] roomRoots;

        [Header("Configuration")]
        [SerializeField] private bool autoGenerateOnAwake = true;

        private void Awake()
        {
            if (autoGenerateOnAwake)
            {
                GenerateAllPropColliders();
            }
        }

        [ContextMenu("Generate Colliders Now")]
        public void GenerateAllPropColliders()
        {
            EnsureRoomRoots();

            int collidersAdded = 0;
            int totalRoomsProcessed = 0;

            foreach (var room in roomRoots)
            {
                if (room == null) continue;
                totalRoomsProcessed++;

                int roomColliders = 0;
                var meshFilters = room.GetComponentsInChildren<MeshFilter>(true);
                foreach (var mf in meshFilters)
                {
                    if (mf == null) continue;

                    if (TryAddExactCollider(mf.gameObject))
                    {
                        roomColliders++;
                    }
                }

                collidersAdded += roomColliders;
                Debug.Log($"[EnvironmentPropColliderManager] Room '{room.name}': Added {roomColliders} exact MeshColliders.");
            }

            Debug.Log($"[EnvironmentPropColliderManager] Finished! Added {collidersAdded} exact MeshColliders across {totalRoomsProcessed} facility rooms.");
        }

        private void EnsureRoomRoots()
        {
            if (roomRoots != null && roomRoots.Length > 0) return;

            var list = new HashSet<GameObject>();

            // 1. Scan children of "Map" container
            var mapGo = GameObject.Find("Map");
            if (mapGo != null)
            {
                foreach (Transform child in mapGo.transform)
                {
                    if (child == null) continue;
                    string cName = child.name.ToLower();
                    // Exclude structural elements that already have solid colliders
                    if (cName.Contains("tanah") || cName.Contains("tembok") || cName.Contains("pintu") || cName.Contains("atap"))
                    {
                        continue;
                    }

                    list.Add(child.gameObject);
                }
            }

            // 2. Ensure standard named rooms are included
            string[] defaultRoomNames = new string[]
            {
                "MainLobby", "lab", "janitor", "ITCenter", "ControlPanel", "room1"
            };

            foreach (var rName in defaultRoomNames)
            {
                var rGo = GameObject.Find(rName);
                if (rGo != null)
                {
                    list.Add(rGo);
                }
            }

            roomRoots = new GameObject[list.Count];
            list.CopyTo(roomRoots);
        }

        private bool TryAddExactCollider(GameObject target)
        {
            if (target == null) return false;

            // Skip if this object or any parent/child already has ANY collider
            // (e.g. Doors, Walls, Ground, Ceiling, Server terminal, Switch, scanner, keycard, Fuse, Triggers)
            if (target.GetComponent<Collider>() != null ||
                target.GetComponentInParent<Collider>() != null ||
                target.GetComponentInChildren<Collider>() != null)
            {
                return false;
            }

            // Skip if part of an interactive quest object
            if (target.GetComponentInParent<IInteractable>() != null ||
                target.GetComponentInChildren<IInteractable>() != null)
            {
                return false;
            }

            // Skip non-prop entities: lights, lamps, cameras, signs, triggers, billboards, exit markers
            string nameLower = target.name.ToLower();
            if (nameLower.Contains("light") || nameLower.Contains("lampu") ||
                nameLower.Contains("sign") || nameLower.Contains("trigger") ||
                nameLower.Contains("camera") || nameLower.Contains("billboard") ||
                nameLower.Contains("exit"))
            {
                return false;
            }

            var mf = target.GetComponent<MeshFilter>();
            var mr = target.GetComponent<MeshRenderer>();
            if (mf == null || mf.sharedMesh == null || mr == null || !mr.enabled)
            {
                return false;
            }

            // Must have valid geometric triangles
            if (mf.sharedMesh.vertexCount < 3)
            {
                return false;
            }

            // Add exact MeshCollider matching the 3D model geometry
            MeshCollider mc = target.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;
            mc.convex = false;

            // SPATIAL SAFETY GUARD: Verify this new collider does not intersect Player spawn position!
            var playerScript = Object.FindFirstObjectByType<PlayerInputScript>();
            if (playerScript != null)
            {
                Vector3 playerPos = playerScript.transform.position;
                Bounds spawnBounds = new Bounds(playerPos, new Vector3(1.2f, 2.2f, 1.2f));
                if (mc.bounds.Intersects(spawnBounds))
                {
                    Debug.LogWarning($"[EnvironmentPropColliderManager] Skipping collider on '{target.name}' because it intersects player spawn at {playerPos}!");
                    Destroy(mc);
                    return false;
                }
            }

            return true;
        }
    }
}
