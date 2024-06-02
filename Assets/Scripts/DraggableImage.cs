using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableImage : MonoBehaviour, IDragHandler
{
    public bool LockMovement = false;
    public Vector3 PositiveIgnore, NegativeIgnore;
    public virtual void OnDrag(PointerEventData eventData)
    {
        Vector3 currentPostion = transform.position;
        Vector3 screenPoint = Input.mousePosition;
        screenPoint.z = 10.0f; //distance of the plane from the camera

        Vector3 LockedPosition = Camera.main.ScreenToWorldPoint(screenPoint);
        if (LockMovement)
        {
            LockedPosition = new Vector3(LockedPosition.x, transform.position.y, LockedPosition.z);
        }

        if (LockedPosition.y >= PositiveIgnore.y)
        {
            LockedPosition = new Vector3(LockedPosition.x, currentPostion.y, LockedPosition.z);
        }
        if (LockedPosition.y <= NegativeIgnore.y)
        {
            LockedPosition = new Vector3(LockedPosition.x, currentPostion.y, LockedPosition.z);
        }
        if (LockedPosition.x >= PositiveIgnore.x)
        {
            LockedPosition = new Vector3(currentPostion.x, LockedPosition.y, LockedPosition.z);
        }
        if (LockedPosition.x <= NegativeIgnore.x)
        {
            LockedPosition = new Vector3(currentPostion.x, LockedPosition.y, LockedPosition.z);
        }

        transform.position = LockedPosition;

    }
}
