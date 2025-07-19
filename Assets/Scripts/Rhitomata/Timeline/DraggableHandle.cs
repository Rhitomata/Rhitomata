using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableHandle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform rectToMove;
    public Image image;
    public Color hoveredColor = Color.gray;
    public Color pressedColor = Color.white;
    public Color normalColor = Color.black;

    private bool isHovered;
    private bool isPressed;

    private void Start() {
        normalColor = image.material.color;
        image.material = null;
        image.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isHovered = true;
        UpdateColor();
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovered = false;
        UpdateColor();
    }

    public void OnPointerDown(PointerEventData eventData) {
        isPressed = true;
        UpdateColor();
    }

    public void OnPointerUp(PointerEventData eventData) {
        isPressed = false;
        UpdateColor();
    }

    public void OnDrag(PointerEventData eventData) {
        if (rectToMove == null)
            return;

        rectToMove.anchoredPosition += new Vector2(GetLocalDelta(eventData).x, 0);
    }

    private Vector2 GetLocalDelta(PointerEventData data) {
        return GetLocalPoint(data.position) - GetLocalPoint(data.position - data.delta);
    }

    private Vector2 GetLocalPoint(Vector2 screenPosition) {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectToMove, screenPosition, null, out var localPoint);
        return localPoint;
    }

    public void UpdateColor() {
        if (isPressed) {
            image.color = pressedColor;
        } else if (isHovered) {
            image.color = hoveredColor;
        } else {
            image.color = normalColor;
        }
    }
}
