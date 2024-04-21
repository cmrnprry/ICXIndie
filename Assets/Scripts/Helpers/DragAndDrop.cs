using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDrop : DraggableImage, IPointerDownHandler, IPointerUpHandler
{
    protected RectTransform rect_transform;
    protected bool isDrag;

    protected virtual void Awake()
    {
        rect_transform = GetComponent<RectTransform>();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        isDrag = true;
    }

    public virtual void OnPointerUp(PointerEventData pointerEventData)
    {
        isDrag = false;
    }
}
