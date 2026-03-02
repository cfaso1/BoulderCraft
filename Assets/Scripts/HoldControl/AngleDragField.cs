using UnityEngine;
using UnityEngine.EventSystems;

public class AngleDragField : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] float sensitivity = 1f;
    [SerializeField] Texture2D rotateCursor;

    public event System.Action<float> OnDelta;
    public bool IsDragging { get; private set; }

    static readonly Vector2 cursorHotspot = new Vector2(16, 16);
    Vector2 lastPointerPos;

    public void OnPointerDown(PointerEventData e)
    {
        IsDragging = true;
        lastPointerPos = e.position;
        Cursor.SetCursor(rotateCursor, cursorHotspot, CursorMode.Auto);
    }

    public void OnDrag(PointerEventData e)
    {
        float delta = (e.position.x - lastPointerPos.x) * sensitivity;
        lastPointerPos = e.position;
        OnDelta?.Invoke(delta);
    }

    public void OnPointerUp(PointerEventData e)
    {
        IsDragging = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
