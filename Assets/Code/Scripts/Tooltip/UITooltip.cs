using UnityEngine;
using UnityEngine.EventSystems;

public class UITooltip : TooltipProvider, IPointerEnterHandler, IPointerExitHandler
{
    private RectTransform _canvasRect;
    private RectTransform _rect;

    protected override void Awake()
    {
        base.Awake();

        _canvasRect = GetComponentInParent<RectTransform>(true);
        _rect = transform as RectTransform;
    }

    public override Rect ScreenRect()
    {
        return RectTransformToScreenSpace(_rect);
    }

    public static Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.5f), size);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Active = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Active = false;
    }
}
