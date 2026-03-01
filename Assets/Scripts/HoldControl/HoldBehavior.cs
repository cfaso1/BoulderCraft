using UnityEngine;

public enum HoldType { Jug, Crimp, Sloper, Pinch, Pocket, Volume }
public enum HoldSize { Small, Medium, Large }

public class HoldBehavior : MonoBehaviour
{
    public string holdId;
    public HoldType holdType;
    public HoldSize holdSize;
    public Color holdColor = Color.white;

    public void SetHighlight(bool highlighted)
    {
        // TODO: swap to highlight material when highlighted, restore original when not
    }
}
