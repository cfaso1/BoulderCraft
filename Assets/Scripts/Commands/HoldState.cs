using UnityEngine;

public struct HoldState
{
    public Vector3 position;
    public Quaternion rotation;
    public Transform parent;
    public float rotationAngle;
    public Vector3 wallNormal;

    public HoldState(Transform t, HoldBehavior b)
    {
        position     = t.position;
        rotation     = t.rotation;
        parent       = t.parent;
        rotationAngle = b.rotationAngle;
        wallNormal   = b.lastWallNormal;
    }
}
