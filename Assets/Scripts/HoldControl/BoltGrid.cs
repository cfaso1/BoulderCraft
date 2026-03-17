using UnityEngine;

/// Requirements: bolt GameObjects must be on the layer assigned to boltLayer
/// and have any Collider component (a SphereCollider with radius ~0.01 works).
public class BoltGrid : MonoBehaviour
{
    [SerializeField] float searchRadius = 0.15f;
    [SerializeField] LayerMask boltLayer;

    readonly Collider[] _buffer = new Collider[32];

    public Vector3 FindNearestBolt(Vector3 wallPoint, Vector3 wallNormal, Transform exclude = null)
    {
        int count = Physics.OverlapSphereNonAlloc(wallPoint, searchRadius, _buffer, boltLayer);

        if (count == 0) return wallPoint;

        float bestDist = float.MaxValue;
        Vector3 bestPos = wallPoint;

        for (int i = 0; i < count; i++)
        {
            if (exclude != null && _buffer[i].transform.IsChildOf(exclude))
                continue;

            Vector3 boltPos = _buffer[i].bounds.center;
            Vector3 projected = Vector3.ProjectOnPlane(boltPos - wallPoint, wallNormal) + wallPoint;
            float dist = (projected - wallPoint).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                bestPos = boltPos;
            }
        }

        return bestPos;
    }
}
