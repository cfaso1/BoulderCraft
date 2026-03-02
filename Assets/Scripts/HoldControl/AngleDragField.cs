using UnityEngine;
using UnityEngine.EventSystems;

// Attach to a UI element with an Image component.
// Fires OnDelta with the horizontal drag delta in degrees each frame while dragging.
public class AngleDragField : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] float sensitivity = 1f;

    public event System.Action<float> OnDelta;

    Vector2 lastPointerPos;

    public void OnPointerDown(PointerEventData e) => lastPointerPos = e.position;

    public void OnDrag(PointerEventData e)
    {
        float delta = (e.position.x - lastPointerPos.x) * sensitivity;
        lastPointerPos = e.position;
        OnDelta?.Invoke(delta);
    }
}
