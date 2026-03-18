using UnityEngine;

public class MoveHoldCommand : ICommand
{
    readonly GameObject hold;
    readonly PlacementManager pm;
    readonly Vector3 fromPos, toPos;
    readonly Quaternion fromRot, toRot;
    readonly Transform fromParent, toParent;
    readonly float fromAngle, toAngle;
    readonly Vector3 fromNormal, toNormal;

    public MoveHoldCommand(
        GameObject hold, PlacementManager pm,
        Vector3 fromPos, Quaternion fromRot, Transform fromParent, float fromAngle, Vector3 fromNormal,
        Vector3 toPos,   Quaternion toRot,   Transform toParent,   float toAngle,   Vector3 toNormal)
    {
        this.hold = hold;
        this.pm = pm;
        this.fromPos = fromPos; this.fromRot = fromRot; this.fromParent = fromParent; this.fromAngle = fromAngle; this.fromNormal = fromNormal;
        this.toPos   = toPos;   this.toRot   = toRot;   this.toParent   = toParent;   this.toAngle   = toAngle;   this.toNormal   = toNormal;
    }

    public void Execute() => Apply(toPos,   toRot,   toParent,   toAngle,   toNormal);
    public void Undo()    => Apply(fromPos, fromRot, fromParent, fromAngle, fromNormal);

    void Apply(Vector3 pos, Quaternion rot, Transform parent, float angle, Vector3 normal)
    {
        hold.transform.SetParent(parent, true);
        hold.transform.position = pos;
        hold.transform.rotation = rot;
        var b = hold.GetComponent<HoldBehavior>();
        b.rotationAngle = angle;
        b.lastWallNormal = normal;
        if (pm.SelectedHold == hold) pm.SyncSelectedState();
    }
}
