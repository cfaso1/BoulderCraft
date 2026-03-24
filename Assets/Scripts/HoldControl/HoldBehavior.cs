using UnityEngine;

public enum HoldType { Jug, Crimp, Sloper, Pinch, Pocket, Foothold, Volume }
public enum HoldSize { Small, Medium, Large }

public class HoldBehavior : MonoBehaviour
{
    public string holdId;
    public string instanceSaveId = "";
    public HoldType holdType;
    public HoldSize holdSize;
    public Color holdColor = Color.white;
    public float rotationAngle;
    public bool isLocked;
    public Vector3 lastWallNormal = Vector3.forward;
    [SerializeField] Material highlightMaterial;

    Renderer rend;
    Material originalMaterial;

    public Material OriginalMaterial => originalMaterial;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        originalMaterial = rend.material;
        if (string.IsNullOrEmpty(instanceSaveId))
            instanceSaveId = System.Guid.NewGuid().ToString();
    }

    public void SetOriginalMaterial(Material mat) { originalMaterial = mat; }

    public void SetHighlight(bool on)
    {
        rend.material = on ? highlightMaterial : originalMaterial;
    }

    public static Quaternion ComputeRotation(float angle, Vector3 normal)
    {
        Vector3 refUp = Vector3.ProjectOnPlane(Vector3.up, normal).normalized;
        if (refUp.sqrMagnitude < 0.01f)
            refUp = Vector3.ProjectOnPlane(Vector3.forward, normal).normalized;
        Quaternion baseOrientation = Quaternion.LookRotation(refUp, normal) * Quaternion.Euler(-90f, 0f, 0f);
        return Quaternion.AngleAxis(angle, normal) * baseOrientation;
    }
}
