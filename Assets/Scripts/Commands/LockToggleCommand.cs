public class LockToggleCommand : ICommand
{
    readonly HoldBehavior behavior;
    readonly HoldToolbar toolbar;
    readonly PlacementManager pm;
    readonly bool previousState;

    public LockToggleCommand(HoldBehavior behavior, HoldToolbar toolbar, PlacementManager pm)
    {
        this.behavior = behavior;
        this.toolbar = toolbar;
        this.pm = pm;
        previousState = behavior.isLocked;
    }

    public void Execute() => Apply(!previousState);
    public void Undo() => Apply(previousState);

    void Apply(bool locked)
    {
        behavior.isLocked = locked;
        if (pm.SelectedHold == behavior.gameObject) toolbar.UpdateLockState(locked);
    }
}
