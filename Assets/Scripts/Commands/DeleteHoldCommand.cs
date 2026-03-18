using UnityEngine;

public class DeleteHoldCommand : ICommand
{
    readonly GameObject hold;
    readonly PlacementManager pm;
    readonly HoldToolbar toolbar;
    readonly Transform parent;
    readonly Vector3 position;
    readonly Quaternion rotation;
    readonly float rotationAngle;
    readonly bool wasLocked;
    readonly Vector3 wallNormal;

    public DeleteHoldCommand(GameObject hold, PlacementManager pm, HoldToolbar toolbar)
    {
        this.hold = hold;
        this.pm = pm;
        this.toolbar = toolbar;
        var b = hold.GetComponent<HoldBehavior>();
        parent = hold.transform.parent;
        position = hold.transform.position;
        rotation = hold.transform.rotation;
        rotationAngle = b.rotationAngle;
        wasLocked = b.isLocked;
        wallNormal = b.lastWallNormal;
    }

    public void Execute()
    {
        // PM deselects before Do() on first run; this guard handles the redo case.
        if (pm.SelectedHold == hold) pm.ForceDeselect();
        hold.SetActive(false);
    }

    public void Undo()
    {
        hold.SetActive(true);
        hold.transform.SetParent(parent, true);
        hold.transform.position = position;
        hold.transform.rotation = rotation;
        var b = hold.GetComponent<HoldBehavior>();
        b.rotationAngle = rotationAngle;
        b.isLocked = wasLocked;
        b.lastWallNormal = wallNormal;
    }
}
