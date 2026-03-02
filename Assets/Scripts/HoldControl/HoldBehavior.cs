using UnityEngine;

public enum HoldType { Jug, Crimp, Sloper, Pinch, Pocket, Volume }
public enum HoldSize { Small, Medium, Large }

public class HoldBehavior : MonoBehaviour
{
    public string holdId;
    public HoldType holdType;
    public HoldSize holdSize;
    public Color holdColor = Color.white;
    [SerializeField] Material highlightMaterial;

    Renderer rend;
    Material originalMaterial;

    void Start()
    {
        rend = GetComponentInChildren<Renderer>();
        originalMaterial = rend.material;
    }

    public void SetHighlight(bool highlighted)
    {
        rend.material = highlighted ? highlightMaterial : originalMaterial;
    }
}
