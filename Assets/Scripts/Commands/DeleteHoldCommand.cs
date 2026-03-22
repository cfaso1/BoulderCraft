using UnityEngine;

public class DeleteHoldCommand : ICommand
{
    readonly GameObject hold;
    readonly HoldBehavior behavior;
    readonly PlacementManager pm;
    readonly HoldState savedState;
    readonly bool wasLocked;

    public DeleteHoldCommand(GameObject hold, HoldBehavior behavior, PlacementManager pm)
    {
        this.hold     = hold;
        this.behavior = behavior;
        this.pm       = pm;
        savedState    = new HoldState(hold.transform, behavior);
        wasLocked     = behavior.isLocked;
    }

    public void Execute()
    {
        if (pm.SelectedHold == hold) pm.Deselect();
        hold.SetActive(false);
    }

    public void Undo()
    {
        hold.SetActive(true);
        hold.transform.SetParent(savedState.parent, true);
        hold.transform.position = savedState.position;
        hold.transform.rotation = savedState.rotation;
        behavior.rotationAngle  = savedState.rotationAngle;
        behavior.lastWallNormal = savedState.wallNormal;
        behavior.isLocked       = wasLocked;
    }
}
