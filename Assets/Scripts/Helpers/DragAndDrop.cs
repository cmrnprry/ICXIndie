using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    protected RectTransform rect_transform;
    protected bool isDrag;

    protected virtual void Awake()
    {
        rect_transform = GetComponent<RectTransform>();
    }

    protected virtual void Update()
    {
        if (isDrag)
        {
            var pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            rect_transform.position = pos;
        }
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
