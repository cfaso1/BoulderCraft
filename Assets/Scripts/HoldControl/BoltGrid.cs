using UnityEngine;

public class BoltGrid : MonoBehaviour
{
    [SerializeField] float searchRadius = 0.15f;
    [SerializeField] LayerMask boltLayer;

    readonly Collider[] buffer = new Collider[32];

    public Vector3 FindNearestBolt(Vector3 wallPoint, Vector3 wallNormal, Transform exclude = null)
    {
        int count = Physics.OverlapSphereNonAlloc(wallPoint, searchRadius, buffer, boltLayer);

        if (count == 0) return wallPoint;

        float bestDist = float.MaxValue;
        Vector3 bestPos = wallPoint;

        for (int i = 0; i < count; i++)
        {
            if (exclude != null && buffer[i].transform.IsChildOf(exclude))
                continue;

            Vector3 boltPos = buffer[i].bounds.center;
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
