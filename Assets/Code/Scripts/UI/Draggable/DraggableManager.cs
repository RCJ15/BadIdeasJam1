using System;
using UnityEngine;
using DebugTools;
using Input;
using UnityEngine.UI;
using UnityEngine.Pool;
using System.Collections.Generic;

[SingletonMode(true)]
public class DraggableManager : Singleton<DraggableManager>, IComponentDebugGUI
{
    public static List<DraggableReceiver> ActiveReceivers { get; private set; } = new();

    public static bool IsDragging => CurrentDraggable != null;
    public static DraggableObject CurrentDraggable
    {
        get => _currentDraggable;
        set
        {
            if (_currentDraggable == value) return;

            _currentDraggable = value;

            if (_currentDraggable != null)
            {
                _currentDraggable.OnDrag();
                OnDrag?.Invoke(_currentDraggable);
            }
        }
    }
    private static DraggableObject _currentDraggable;

    public static Action<DraggableObject> OnDrag { get; set; }

    public static Vector2 MousePos => Instance._mousePos;
    public static Vector2 MousePosCanvas => Instance._mousePosCanvas;
    private Vector2 _mousePos;
    private Vector2 _mousePosCanvas;
    public float MoveThreshold => moveThreshold;
    public float SqrMoveThreshold { get; private set; }

    public RectTransform VisualsRect => visual;

    [CacheComponent]
    [SerializeField] private Canvas canvas;

    [Space]
    [SerializeField] private RectTransform mouse;
    [SerializeField] private float moveThreshold = 0.1f;

    [Space]
    [SerializeField] private RectTransform visual;
    [SerializeField] private float rotationMax;
    [SerializeField] private float rotationMultiplier = 1;
    [SerializeField] private float rotationDelta;
    [SerializeField] private float rotationReturnDelta;
    private float _currentRotation;
    private float _animatedRotation;

    protected override void Awake()
    {
        base.Awake();

        SqrMoveThreshold = MoveThreshold * MoveThreshold;
    }

    private void Update()
    {
        Vector2 oldPos = _mousePos;
        _mousePos = canvas.ScreenToCanvasPoint(GameInput.UI.Point.Value);

        mouse.localPosition = _mousePos;
        _mousePosCanvas = mouse.position;

        if (IsDragging && GameInput.UI.Click.Up)
        {
            DraggableReceiver receiver = ActiveReceivers.Count > 0 ? ActiveReceivers[0] : null;

            if (receiver != null)
            {
                receiver.OnDrop(CurrentDraggable);
            }

            CurrentDraggable.OnDrop(receiver);

            CurrentDraggable = null;
        }

        // Movement on visuals
        visual.localPosition = _mousePos;

        float rotation = Mathf.Clamp((oldPos.x - _mousePos.x) * rotationMultiplier, -rotationMax, rotationMax);

        if (rotation != 0)
        {
            _currentRotation = rotation;
        }
        else
        {
            _currentRotation = Mathf.MoveTowards(_currentRotation, 0, Time.deltaTime * rotationReturnDelta);
        }

        _animatedRotation = Mathf.Lerp(_animatedRotation, _currentRotation, Time.deltaTime * rotationDelta);
        visual.localRotation = Quaternion.Euler(0, 0, _animatedRotation);
    }

    public void OnDebugGUI()
    {
        DebugGUI.Property("MousePos", GameInput.UI.Point.Value);
        DebugGUI.Property("IsDragging", IsDragging);
    }
}
