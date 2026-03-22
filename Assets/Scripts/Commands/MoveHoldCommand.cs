using UnityEngine;

public class MoveHoldCommand : ICommand
{
    readonly GameObject hold;
    readonly HoldBehavior behavior;
    readonly PlacementManager pm;
    readonly HoldState from;
    readonly HoldState to;

    public MoveHoldCommand(GameObject hold, HoldBehavior behavior, PlacementManager pm, HoldState from, HoldState to)
    {
        this.hold     = hold;
        this.behavior = behavior;
        this.pm       = pm;
        this.from     = from;
        this.to       = to;
    }

    public void Execute() => Apply(to);
    public void Undo()    => Apply(from);

    void Apply(HoldState state)
    {
        hold.transform.SetParent(state.parent, true);
        hold.transform.position = state.position;
        hold.transform.rotation = state.rotation;
        behavior.rotationAngle  = state.rotationAngle;
        behavior.lastWallNormal = state.wallNormal;
        if (pm.SelectedHold == hold) pm.SyncSelectedState();
    }
}
