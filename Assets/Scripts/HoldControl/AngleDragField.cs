using UnityEngine;
using UnityEngine.EventSystems;

public class AngleDragField : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] float sensitivity = 1f;
    [SerializeField] Texture2D rotateCursor;

    public bool IsDragging { get; private set; }

    public event System.Action<float> OnDelta;

    static readonly Vector2 cursorHotspot = new Vector2(16, 16);
    Vector2 lastPointerPos;

    // Toolbar drag (left-click on the button)
    public void OnPointerDown(PointerEventData e)
    {
        lastPointerPos = e.position;
        Cursor.SetCursor(rotateCursor, cursorHotspot, CursorMode.Auto);
        IsDragging = true;
    }

    public void OnDrag(PointerEventData e)
    {
        float delta = (e.position.x - lastPointerPos.x) * sensitivity;
        lastPointerPos = e.position;
        OnDelta?.Invoke(delta);
    }

    public void OnPointerUp(PointerEventData e)
    {
        Cursor.SetCursor(PlayerCam.DefaultCursor, new Vector2(5, 1), CursorMode.Auto);
        IsDragging = false;
    }

    // Global right-click drag (works anywhere on screen)
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            IsDragging = true;
            lastPointerPos = Input.mousePosition;
            Cursor.SetCursor(rotateCursor, cursorHotspot, CursorMode.Auto);
        }

        if (Input.GetMouseButton(1))
        {
            float delta = (Input.mousePosition.x - lastPointerPos.x) * sensitivity;
            lastPointerPos = Input.mousePosition;
            OnDelta?.Invoke(delta);
        }

        if (Input.GetMouseButtonUp(1))
        {
            IsDragging = false;
            Cursor.SetCursor(PlayerCam.DefaultCursor, new Vector2(5, 1), CursorMode.Auto);
        }
    }
}
