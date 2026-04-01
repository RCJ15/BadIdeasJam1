using UnityEngine;

public class WorldTooltip : TooltipProvider
{
    [CacheComponent]
    [SerializeField] private Collider col;

    private Camera _camera;

    public override Rect ScreenRect()
    {
        Bounds bounds = col.bounds;

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        Vector3[] points = new Vector3[]
        {
            min,
            max,

            new(max.x, min.y, min.z),
            new(min.x, max.y, min.z),
            new(min.x, min.y, max.z),

            new(min.x, max.y, max.z),
            new(max.x, min.y, max.z),
            new(max.x, max.y, min.z),
        };

        bool createdScreenBounds = false;
        Bounds screenBounds = new();

        foreach (Vector3 point in points)
        {
            Vector2 screenPoint = _camera.WorldToScreenPoint(point);

            if (createdScreenBounds)
            {
                screenBounds.Encapsulate(screenPoint);
            }
            else
            {
                screenBounds = new Bounds(screenPoint, Vector3.zero);
                createdScreenBounds = true;
            }
        }
        
        return new(screenBounds.center, screenBounds.size);
    }

    protected override void Awake()
    {
        base.Awake();

        _camera = Camera.main;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        WorldTooltipManager.Tooltips.Add(col, this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        WorldTooltipManager.Tooltips.Remove(col);
    }
}
