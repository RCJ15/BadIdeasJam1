using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DebugTools;

public class DraggableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IPointerDownHandler, IPointerUpHandler, IDraggable
    , IComponentDebugGUI
{
    public bool IsBeingDragged { get; private set; }

    public ComponentCollection<IDraggable> Draggables => draggables;

    [CacheComponent(AlwaysCache = true)]
    [SerializeField]
    private ComponentCollection<IDraggable> draggables;

    private bool _pointerOnTop;
    private bool _pointerDown;
    private Vector2? _pressPoint;

    private DraggableManager _draggableManager;

    private void Start()
    {
        _draggableManager = DraggableManager.Instance;
    }

    private void OnDisable()
    {
        if (DraggableManager.CurrentDraggable == this)
        {
            DraggableManager.CurrentDraggable = null;
        }

        _pointerDown = false;
        _pressPoint = null;
        IsBeingDragged = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _pointerOnTop = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pointerOnTop = false;
        _pointerDown = false;

        if (_pressPoint.HasValue)
        {
            BeginDragging();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        _pointerDown = true;

        _pressPoint = DraggableManager.MousePos;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (DraggableManager.IsDragging) return;

        if (_pressPoint.HasValue)
        {
            Vector2 delta = DraggableManager.MousePos - _pressPoint.Value;
            if (delta.sqrMagnitude >= _draggableManager.SqrMoveThreshold)
            {
                BeginDragging();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (!IsBeingDragged && _pointerOnTop)
        {
            OnClick();
        }

        _pointerDown = false;
        _pressPoint = null;
        IsBeingDragged = false;
    }

    private void BeginDragging()
    {
        _pressPoint = null;
        DraggableManager.CurrentDraggable = this;
        IsBeingDragged = true;
    }

    public void OnDebugGUI()
    {
        DebugGUI.Property("top", _pointerOnTop);
        DebugGUI.Property("down", _pointerDown);
    }

    public void OnDrag()
    {
        foreach (IDraggable draggable in draggables)
        {
            draggable.OnDrag();
        }
    }

    public void OnClick()
    {
        foreach (IDraggable draggable in draggables)
        {
            draggable.OnClick();
        }
    }

    public void OnDrop(DraggableReceiver receiver)
    {
        foreach (IDraggable draggable in draggables)
        {
            draggable.OnDrop(receiver);
        }
    }
}
