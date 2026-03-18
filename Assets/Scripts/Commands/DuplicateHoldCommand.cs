using UnityEngine;

public class DuplicateHoldCommand : ICommand
{
    readonly GameObject prefab;
    readonly Vector3 position;
    readonly Quaternion rotation;
    readonly Vector3 scale;
    readonly Transform parent;
    readonly float rotationAngle;
    readonly Vector3 wallNormal;
    readonly string instanceSaveId;
    readonly PlacementManager pm;

    GameObject spawned;
    public GameObject SpawnedHold => spawned;

    // Fallback constructor: hold is already instantiated by the caller.
    public DuplicateHoldCommand(GameObject alreadySpawned, PlacementManager pm)
    {
        spawned = alreadySpawned;
        this.pm = pm;
    }

    public DuplicateHoldCommand(
        GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale,
        Transform parent, float rotationAngle, Vector3 wallNormal, PlacementManager pm)
    {
        this.prefab = prefab;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.parent = parent;
        this.rotationAngle = rotationAngle;
        this.wallNormal = wallNormal;
        instanceSaveId = System.Guid.NewGuid().ToString();
        this.pm = pm;
    }

    public void Execute()
    {
        if (spawned == null)
        {
            spawned = Object.Instantiate(prefab, position, rotation);
            spawned.transform.localScale = scale;
            spawned.transform.SetParent(parent, true);
            var b = spawned.GetComponent<HoldBehavior>();
            b.isLocked = false;
            b.rotationAngle = rotationAngle;
            b.lastWallNormal = wallNormal;
            b.instanceSaveId = instanceSaveId;
        }
        else
        {
            spawned.SetActive(true);
        }
    }

    public void Undo()
    {
        if (pm.SelectedHold == spawned) pm.ForceDeselect();
        spawned.SetActive(false);
    }
}
