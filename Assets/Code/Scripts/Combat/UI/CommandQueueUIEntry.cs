using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class CommandQueueUIEntry : MonoBehaviour, IPointerClickHandler, IDraggable
{
    public int Index => Parent.IndexOf(this);
    public Command Command => PlayerCommandQueue.List[Index];
    public CommandQueueUI Parent { get; set; }

    public float YPos => _yPos;
    private float _yPos;

    public bool IsNew { get; set; } = false;

    public float YPosCanvas => _rect.position.y;

    [CacheComponent]
    [SerializeField] private CommandVisuals visuals;
    [CacheComponent]
    [SerializeField] private CanvasGroup group;
    [CacheComponent]
    [SerializeField] private UIDissolve dissolve;
    [CacheComponent]
    [SerializeField] private DraggableCommand draggableCommand;

    [Space]
    [SerializeField] private RectTransform visualsRect;

    private Vector2 _textOffset => visuals.Text.TextBoundCenterCanvasSpace(_canvas);
    private Vector2 _visualsPosition;
    private float _visualsPositionT;

    private RectTransform _rect;

    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>(true);

        _rect = transform as RectTransform;
        _yPos = _rect.anchoredPosition.y;

        dissolve.DissolveAmount = 1;
    }

    public void Disable()
    {
        group.alpha = 0;
        group.blocksRaycasts = false;
    }

    public void Enable()
    {
        group.alpha = 1;
    }

    public void Setup()
    {
        visualsRect.SetParent(Parent.transform);
    }

    public void Create(bool wasDragged)
    {
        group.blocksRaycasts = true;

        IsNew = true;

        Command command = Command;
        draggableCommand.Command = command;
        visuals.Command = command;
        visuals.Color = Color.white;

        visuals.DOKill();

        if (wasDragged)
        {
            CommandDragVisual.SetCommand(null, true);
            SetVisualPosition(CommandDragVisual.Instance.transform.position);
            dissolve.DissolveAmount = 0;
        }
        else
        {
            this.DOKill();
            _visualsPositionT = 0;
            dissolve.TweenDissolveAmount(0,  0.25f);
        }
    }

    public void Remove(bool instant = true)
    {
        group.blocksRaycasts = false;

        IsNew = false;

        dissolve.DOKill();

        if (instant)
        {
            dissolve.DissolveAmount = 1;
            Release();
        }
        else
        {
            dissolve.TweenDissolveAmount(1, 0.25f).onComplete = Release;
        }

        SoundManager.PlaySound("delete_command");
    }
    private void Release()
    {
        Parent.Pool.Release(this);
    }

    public void Execute(float duration)
    {
        visuals.TweenColor(new Color(1, 1, 1, 0.3f), duration);
    }

    public void DeleteCommand(bool instant)
    {
        CommandQueueUI.InstantlyDestroyNextCommand = instant;
        PlayerCommandQueue.Remove(Index);
    }

    public void SetYPos(float y, bool instant)
    {
        if (_yPos == y) return;

        _yPos = y;

        _rect.DOKill();

        if (instant)
        {
            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, y);
        }
        else
        {
            _rect.DOAnchorPosY(y, 0.35f).SetEase(Ease.OutExpo);
        }
    }

    private void LateUpdate()
    {
        visualsRect.transform.position = Vector3.Lerp(transform.position, _visualsPosition, _visualsPositionT);
    }

    public void OnDrag()
    {
        DeleteCommand(true);
        CommandDragVisual.Position = (Vector2)visuals.Text.transform.position + _textOffset;
    }

    public void OnClick()
    {

    }

    public void OnDrop(DraggableReceiver receiver)
    {

    }

    public void SetVisualPosition(Vector2 pos)
    {
        _visualsPosition = pos - _textOffset;
        _visualsPositionT = 1;

        this.DOKill();
        DOTween.To(() => _visualsPositionT, (v) => _visualsPositionT = v, 0, 0.5f).SetEase(Ease.OutExpo).SetTarget(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click destroys this
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            DeleteCommand(false);
        }
    }
}
