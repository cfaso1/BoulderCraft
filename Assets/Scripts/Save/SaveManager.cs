using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public string LastSavedPath { get; private set; }

    void Awake() { Instance = this; }

    public void SaveRoute(string path)
    {
        var data = new RouteSaveData { holds = new List<HoldSaveData>() };
        var behaviors = FindObjectsByType<HoldBehavior>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // Ensure every active hold has an instanceSaveId before serializing.
        foreach (var b in behaviors)
            if (b.gameObject.activeInHierarchy && string.IsNullOrEmpty(b.instanceSaveId))
                b.instanceSaveId = Guid.NewGuid().ToString();

        foreach (var b in behaviors)
        {
            if (!b.gameObject.activeInHierarchy) continue;

            // Resolve volume parent: climb up until we find a HoldBehavior ancestor (the volume).
            string parentId = "";
            Transform p = b.transform.parent;
            while (p != null)
            {
                var parentBehavior = p.GetComponent<HoldBehavior>();
                if (parentBehavior != null) { parentId = parentBehavior.instanceSaveId; break; }
                p = p.parent;
            }

            data.holds.Add(new HoldSaveData
            {
                holdId              = b.holdId,
                instanceSaveId      = b.instanceSaveId,
                parentInstanceSaveId = parentId,
                position            = b.transform.position,
                rotation            = b.transform.rotation,
                rotationAngle       = b.rotationAngle,
                isLocked            = b.isLocked,
                lastWallNormal      = b.lastWallNormal
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        LastSavedPath = path;
        Debug.Log($"[SaveManager] Route saved to {path} ({data.holds.Count} holds)");
    }

    public bool LoadRoute(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveManager] File not found: {path}");
            return false;
        }

        string json = File.ReadAllText(path);
        RouteSaveData data = JsonUtility.FromJson<RouteSaveData>(json);
        if (data == null || data.holds == null)
        {
            Debug.LogWarning($"[SaveManager] Failed to parse save file: {path}");
            return false;
        }

        ClearAllHolds();
        UndoRedoManager.Instance?.Clear();
        PlacementManager.Instance?.ForceDeselect();

        // Pass 1: instantiate every hold and index by instanceSaveId.
        var spawnedById = new Dictionary<string, GameObject>();
        var allSpawned  = new List<(HoldSaveData saveData, GameObject go)>();

        foreach (var hd in data.holds)
        {
            ItemData item = FindItemData(hd.holdId);
            if (item == null)
            {
                Debug.LogWarning($"[SaveManager] No ItemData found for holdId '{hd.holdId}' — skipping.");
                continue;
            }

            GameObject go = Instantiate(item.holdPrefab, hd.position, hd.rotation);
            var b = go.GetComponent<HoldBehavior>();
            b.instanceSaveId = hd.instanceSaveId;   // overwrite the Awake-assigned GUID with the saved one
            b.rotationAngle  = hd.rotationAngle;
            b.isLocked       = hd.isLocked;
            b.lastWallNormal = hd.lastWallNormal;

            spawnedById[hd.instanceSaveId] = go;
            allSpawned.Add((hd, go));
        }

        // Pass 2: restore volume parenting.
        foreach (var (hd, go) in allSpawned)
        {
            if (!string.IsNullOrEmpty(hd.parentInstanceSaveId) &&
                spawnedById.TryGetValue(hd.parentInstanceSaveId, out var parentGo))
            {
                go.transform.SetParent(parentGo.transform, true);
            }
        }

        LastSavedPath = path;
        Debug.Log($"[SaveManager] Route loaded from {path} ({data.holds.Count} holds)");
        return true;
    }

    public void NewRoute()
    {
        ClearAllHolds();
        UndoRedoManager.Instance?.Clear();
        PlacementManager.Instance?.ForceDeselect();
        LastSavedPath = null;
    }

    void ClearAllHolds()
    {
        var behaviors = FindObjectsByType<HoldBehavior>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // Only destroy root holds — their children (holds parented to volumes) are destroyed automatically.
        foreach (var b in behaviors)
        {
            if (b == null) continue;
            bool hasHoldAncestor = b.transform.parent != null &&
                                   b.transform.parent.GetComponentInParent<HoldBehavior>() != null;
            if (!hasHoldAncestor)
                Destroy(b.gameObject);
        }
    }

    ItemData FindItemData(string holdId) => InventoryManager.Instance.FindItemData(holdId);
}
