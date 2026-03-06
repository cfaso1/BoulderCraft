using UnityEngine;

public class WallPanel : MonoBehaviour
{
    [SerializeField] float panelWidth = 121.92f;   // 4 ft
    [SerializeField] float panelHeight = 121.92f;   // 4 ft
    [SerializeField] float colSpacing = 20.32f;   // 8 inches horizontal
    [SerializeField] float rowSpacing = 10.16f;   // 4 inches vertical
    [SerializeField] bool showGizmos = true;
    // Which local axis points outward from the wall face.
    // Default is -Z (forward). Change to match your panel's orientation if
    // gizmos appear on the wrong plane. Options: Forward, Back, Up, Down, Left, Right
    [SerializeField] LocalAxis wallNormalAxis = LocalAxis.Back;

    public enum LocalAxis { Forward, Back, Up, Down, Left, Right }

    Vector3[] boltPositions;

    void Awake() => GenerateBoltPositions();

    // Returns the two local axes that form the wall surface plane
    // based on whichever axis is the wall normal
    void GetPlaneAxes(out Vector3 right, out Vector3 up)
    {
        switch (wallNormalAxis)
        {
            case LocalAxis.Forward:
            case LocalAxis.Back:
                right = Vector3.right;
                up    = Vector3.up;
                break;
            case LocalAxis.Up:
            case LocalAxis.Down:
                right = Vector3.right;
                up    = Vector3.forward;
                break;
            case LocalAxis.Left:
            case LocalAxis.Right:
                right = Vector3.forward;
                up    = Vector3.up;
                break;
            default:
                right = Vector3.right;
                up    = Vector3.up;
                break;
        }
    }

    // Returns the local-space depth offset along the normal axis (always 0 — on the surface)
    Vector3 NormalOffset() => Vector3.zero;

    void GenerateBoltPositions()
    {
        int cols = Mathf.RoundToInt(panelWidth  / colSpacing) + 1;
        int rows = Mathf.RoundToInt(panelHeight / rowSpacing) + 1;

        // Odd rows have one fewer bolt because the half-spacing offset
        // pushes the last bolt outside the panel boundary
        int totalBolts = 0;
        for (int row = 0; row < rows; row++)
            totalBolts += (row % 2 == 0) ? cols : cols - 1;

        boltPositions = new Vector3[totalBolts];

        GetPlaneAxes(out Vector3 rightAxis, out Vector3 upAxis);

        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            bool  oddRow  = row % 2 == 1;
            float xOffset = oddRow ? colSpacing * 0.5f : 0f;
            int   rowCols = oddRow ? cols - 1 : cols;
            float y       = row * rowSpacing - panelHeight * 0.5f;

            for (int col = 0; col < rowCols; col++)
            {
                float x = col * colSpacing + xOffset - panelWidth * 0.5f;
                boltPositions[index++] = rightAxis * x + upAxis * y;
            }
        }
    }

    // Returns the world-space position of the nearest bolt to worldPoint
    public Vector3 GetNearestBolt(Vector3 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        Vector3 nearest = boltPositions[0];
        float nearestSqr = (localPoint - nearest).sqrMagnitude;

        for (int i = 1; i < boltPositions.Length; i++)
        {
            float sqr = (localPoint - boltPositions[i]).sqrMagnitude;
            if (sqr < nearestSqr)
            {
                nearestSqr = sqr;
                nearest = boltPositions[i];
            }
        }

        return transform.TransformPoint(nearest);
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        if (boltPositions == null) GenerateBoltPositions();

        Gizmos.color = Color.yellow;
        float radius = colSpacing * 0.15f;
        foreach (Vector3 pos in boltPositions)
            Gizmos.DrawSphere(transform.TransformPoint(pos), radius);
    }
}
