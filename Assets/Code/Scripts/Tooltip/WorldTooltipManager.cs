using Input;
using System.Collections.Generic;
using UnityEngine;

[SingletonMode(true)]
public class WorldTooltipManager : Singleton<WorldTooltipManager>
{
    public static readonly Dictionary<Collider, WorldTooltip> Tooltips = new();

    private Camera _camera;

    [SerializeField] private LayerMask layerMask;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (_camera == null)
        {
            return;
        }

        //Vector2 mouseScreenPoint = GameInput.UI.Point;
        //Vector3 mousePos = _camera.ScreenToWorldPoint(new(mouseScreenPoint.x, mouseScreenPoint.y, -_camera.transform.position.z));
        //Vector3 cameraPoint = _camera.transform.position;
        Ray mouseRay = _camera.ScreenPointToRay(GameInput.UI.Point.Value);

        WorldTooltip active = null;

        if (Physics.Raycast(mouseRay, out RaycastHit hit, 10000f, layerMask))
        {
            if (Tooltips.TryGetValue(hit.collider, out WorldTooltip tooltip))
            {
                tooltip.Active = true;
                active = tooltip;
            }
        }

        foreach (var pair in Tooltips)
        {
            if (pair.Value == active) continue;

            pair.Value.Active = false;
        }
    }
}
