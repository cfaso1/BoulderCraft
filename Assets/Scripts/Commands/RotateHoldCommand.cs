using UnityEngine;

public class RotateHoldCommand : ICommand
{
    readonly HoldBehavior behavior;
    readonly PlacementManager pm;
    readonly float oldAngle, newAngle;

    public RotateHoldCommand(HoldBehavior behavior, float oldAngle, float newAngle, PlacementManager pm)
    {
        this.behavior = behavior;
        this.pm = pm;
        this.oldAngle = oldAngle;
        this.newAngle = newAngle;
    }

    public void Execute() => Apply(newAngle);
    public void Undo()    => Apply(oldAngle);

    void Apply(float angle)
    {
        behavior.rotationAngle = angle;
        behavior.transform.rotation = HoldBehavior.ComputeRotation(angle, behavior.lastWallNormal);
        if (pm.SelectedHold == behavior.gameObject) pm.SyncSelectedState();
    }
}
