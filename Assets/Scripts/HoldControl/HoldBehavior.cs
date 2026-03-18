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
    public float rotationAngle = 0f;
    public bool isLocked = false;
    public Vector3 lastWallNormal = Vector3.forward;
    [SerializeField] Material highlightMaterial;

    Renderer rend;
    Material originalMaterial;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        originalMaterial = rend.material;
        if (string.IsNullOrEmpty(instanceSaveId))
            instanceSaveId = System.Guid.NewGuid().ToString();
    }

    public void SetHighlight(bool highlighted)
    {
        rend.material = highlighted ? highlightMaterial : originalMaterial;
    }
}
