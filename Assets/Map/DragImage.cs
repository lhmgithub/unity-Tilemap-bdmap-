using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
[RequireComponent(typeof(Image))]
public class DragImage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Font font;
    public Action<Vector2> dragEvent;
    private Vector2 pos;
    private Vector2 top;
    private Vector2 currentpos;

    private void Awake()
    {
        pos = transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition;
        top = new Vector2(0, 0);
        transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = top;
    }
    public void ResetPos()
    {
        transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = top;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        SetDraggedPosition(eventData, true);
        currentpos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetDraggedPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragEvent != null)
            dragEvent.Invoke(eventData.position - currentpos);
    }
    Vector3 interval;
    private void SetDraggedPosition(PointerEventData eventData, bool flag = false)
    {      
        var rt = transform.GetChild(0).GetComponent<RectTransform>();
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out globalMousePos))
        {
            if (flag)
            {
                interval = globalMousePos - rt.position;
            }
            else
            {
                rt.position = globalMousePos - interval;
            }
        }
    }
    public void RegisterDragEvent(Action<Vector2> ac)
    {
        dragEvent += ac;
    }
}

