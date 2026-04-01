using UnityEngine;

public static class CanvasExtensions
{
    public static Vector2 ScreenToCanvasPoint(this Canvas canvas, Vector2 screenPoint)
    {
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 referenceResolution = canvasRect.sizeDelta;

        Vector2 canvasPoint = new Vector2(
            (screenPoint.x / Screen.width) * referenceResolution.x,
            (screenPoint.y / Screen.height) * referenceResolution.y
        ) - referenceResolution / 2f;

        return canvasPoint;
    }
}
