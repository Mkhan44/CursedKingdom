using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CursorManager : MonoBehaviour
{
    public Image defaultCursor;
    public Image hoverCursor;

    public float fadeSpeed = 12f;

    private float targetHoverAlpha = 0f;

    void Start()
    {
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        transform.position = Input.mousePosition;

        bool hovering = IsHoveringInteractable();
        targetHoverAlpha = hovering ? 1f : 0f;

        FadeCursors();
    }

    void FadeCursors()
    {
        Color d = defaultCursor.color;
        Color h = hoverCursor.color;

        d.a = Mathf.MoveTowards(d.a, 1f - targetHoverAlpha, fadeSpeed * Time.deltaTime);
        h.a = Mathf.MoveTowards(h.a, targetHoverAlpha, fadeSpeed * Time.deltaTime);

        defaultCursor.color = d;
        hoverCursor.color = h;
    }

    bool IsHoveringInteractable()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            var selectable = result.gameObject.GetComponent<Selectable>();
            if (selectable != null && selectable.interactable)
                return true;
        }

        return false;
    }
}