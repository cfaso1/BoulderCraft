using UnityEngine;

public class DuplicateHoldCommand : ICommand
{
    readonly GameObject source;
    readonly Material sourceOriginalMaterial;
    readonly Vector3 spawnPosition;
    readonly Quaternion spawnRotation;
    readonly Vector3 spawnScale;
    readonly Transform spawnParent;
    readonly float rotationAngle;
    readonly Vector3 wallNormal;
    readonly string instanceSaveId;
    readonly PlacementManager pm;

    GameObject spawned;
    public GameObject SpawnedHold => spawned;

    public DuplicateHoldCommand(GameObject sourceHold, HoldBehavior sourceBehavior, Vector3 wallNormal, PlacementManager pm)
    {
        this.wallNormal  = wallNormal;
        this.pm          = pm;
        instanceSaveId   = System.Guid.NewGuid().ToString();
        rotationAngle    = sourceBehavior.rotationAngle;
        spawnRotation    = sourceHold.transform.rotation;
        spawnScale       = sourceHold.transform.lossyScale;
        spawnParent      = sourceHold.transform.parent;

        Vector3 refUp = Vector3.ProjectOnPlane(Vector3.up, wallNormal).normalized;
        if (refUp.sqrMagnitude < 0.01f)
            refUp = Vector3.ProjectOnPlane(Vector3.forward, wallNormal).normalized;
        spawnPosition = sourceHold.transform.position + refUp * 0.1f;

        source = InventoryManager.Instance?.FindItemData(sourceBehavior.holdId)?.holdPrefab ?? sourceHold;
        sourceOriginalMaterial = sourceBehavior.OriginalMaterial;
    }

    public void Execute()
    {
        if (spawned == null)
        {
            spawned = Object.Instantiate(source, spawnPosition, spawnRotation);
            spawned.transform.localScale = spawnScale;
            spawned.transform.SetParent(spawnParent, true);
            var b = spawned.GetComponent<HoldBehavior>();
            b.SetOriginalMaterial(sourceOriginalMaterial);
            b.isLocked       = false;
            b.rotationAngle  = rotationAngle;
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
        if (pm.SelectedHold == spawned) pm.Deselect();
        spawned.SetActive(false);
    }
}
