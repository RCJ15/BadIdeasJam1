using DebugTools;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.EventSystems;
using DG.Tweening;
using Input;
using UnityEngine.UI;
using System;


public class CommandQueueUI : Singleton<CommandQueueUI>, IPointerEnterHandler, IPointerExitHandler, IDraggableReceiver, IComponentDebugGUI
{
    public static Action<int> OnUpdateEnergy { get; set; }

    private bool _playerExecuting => PlayerCombat.ExecutingCommands;
    private PlayerCombat _player;

    [CacheComponent]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private CommandQueueConsole console;

    [Space]
    [SerializeField] private Transform entryParent;
    [SerializeField] private CommandQueueUIEntry entryTemplate;
    [SerializeField] private float entrySize;
    [SerializeField] private float entrySpacing;

    [Space]
    [SerializeField] private TMP_Text lineNumberTemplate;
    [SerializeField] private CustomButton deleteButtonTemplate;
    private CustomButton[] _deleteButtons;
    private int _oldDeleteButtonIndex = -1;
    [SerializeField] private RectTransform seperatorTemplate;
    [SerializeField] private RectTransform pointer;
    [SerializeField] private Image pointerImage;
    private float _pointerDefaultAlpha;
    [SerializeField] private float pointerExecuteAlpha;
    [SerializeField] private RectTransform executePointer;
    [SerializeField] private int maxCommandsAllowed = 20;

    [Space]
    [SerializeField] private Image overlay;
    [SerializeField] private CanvasGroup loadingThing;
    private float _overlayAlpha;
    [SerializeField] private CommandExecuteButton executeButton;

    private Tween _pointerSizeTween;
    private Tween _pointerPositionTween;
    private int? _pointerIndex = null;

    public ObjectPool<CommandQueueUIEntry> Pool => _pool;

    private ObjectPool<CommandQueueUIEntry> _pool;
    private List<CommandQueueUIEntry> _entries = new();
    private Dictionary<int, int> _emptySpace = new();
    public int? EmptySpaceIndex
    {
        get => _emptySpaceIndex;
        set
        {
            if (_emptySpaceIndex == value) return;

            if (_emptySpaceIndex.HasValue)
            {
                RemoveEmptySpace(_emptySpaceIndex.Value);
            }

            _emptySpaceIndex = value;

            if (_emptySpaceIndex.HasValue)
            {
                AddEmptySpace(_emptySpaceIndex.Value);
            }
        }
    }
    private int? _emptySpaceIndex = null;

    private bool _commandHover = false;
    private Vector2? _oldMousePos = null;

    private bool _dirty = false;
    private bool _nextWasDragged;
    private bool _mouseOnTop;

    private RectTransform _rect;
    private int _playerExecutingIndex;

    private int _mouseIndex;

    public int? Energy { get; set; }
    public static bool InstantlyDestroyNextCommand { get; set; }
    public static bool InstantlyDestroyNextDraggable { get; set; }

    private int? _startEnergy;

    protected override void Awake()
    {
        base.Awake();

        _overlayAlpha = overlay.color.a;

        _rect = transform as RectTransform;

        pointer.sizeDelta = Vector2.zero;
        executePointer.gameObject.SetActive(false);
        executePointer.sizeDelta = Vector2.zero;

        _pointerDefaultAlpha = pointerImage.color.a;

        PlayerCommandQueue.Limit = maxCommandsAllowed;

        group.blocksRaycasts = false;
        InstantlyDestroyNextDraggable = false;
    }

    private void Start()
    {
        _player = PlayerCombat.Instance;
        _player.OnBeginExecute += OnPlayerBeginExecute;
        _player.OnEndExecute += OnPlayerEndExecute;
        PlayerCombat.OnExecuteCommand += OnPlayerExecuteCommand;
        PlayerCombat.OnBeginPlayersTurn += OnBeginPlayersTurn;
        PlayerCombat.OnEndPlayersTurn += OnEndPlayersTurn;
        PlayerCombat.OnDie += OnDie;

        _pool = new(
            CreateEntry,
            OnGetEntry,
            OnReleaseEntry
            );

        _pool.Clear();

        // Populate pool
        for (int i = 0; i < maxCommandsAllowed; i++)
        {
            _pool.Release(CreateEntry());
        }

        _deleteButtons = new CustomButton[maxCommandsAllowed];

        string lineNumberFormat = lineNumberTemplate.text;
        for (int i = 0; i < maxCommandsAllowed; i++)
        {
            // Line Number
            TMP_Text lineNumber = Instantiate(lineNumberTemplate, lineNumberTemplate.transform.parent);
            lineNumber.gameObject.SetActive(true);

            lineNumber.text = string.Format(lineNumberFormat, i + 1);

            float y = IndexToYPos(i);
            lineNumber.rectTransform.anchoredPosition = new(0, y);

            // Delete button
            CustomButton deleteButton = Instantiate(deleteButtonTemplate, deleteButtonTemplate.transform.parent);
            deleteButton.gameObject.SetActive(true);

            RectTransform deleteButtonRect = deleteButton.transform as RectTransform;
            deleteButtonRect.anchoredPosition = new(deleteButtonRect.anchoredPosition.x, y);

            deleteButton.Interactable = false;
            int delteIndex = i;
            deleteButton.OnClick.AddListener(() => DeleteEntry(delteIndex));

            deleteButton.transform.localScale = new Vector3(1, 0, 1);

            _deleteButtons[i] = deleteButton;

            // Seperator
            if (i == maxCommandsAllowed - 1)
            {
                continue;
            }

            y -= (entrySize + entrySpacing) / 2f;

            RectTransform seperator = Instantiate(seperatorTemplate, seperatorTemplate.parent);
            seperator.gameObject.SetActive(true);
            seperator.anchoredPosition = new(0, y);
        }

        lineNumberTemplate.gameObject.SetActive(false);
        deleteButtonTemplate.gameObject.SetActive(false);
        seperatorTemplate.gameObject.SetActive(false);

        UpdateUI();

        Subscribe();
    }

    private void OnDestroy()
    {
        _player.OnBeginExecute -= OnPlayerBeginExecute;
        _player.OnEndExecute -= OnPlayerEndExecute;
        PlayerCombat.OnExecuteCommand -= OnPlayerExecuteCommand;
        PlayerCombat.OnBeginPlayersTurn -= OnBeginPlayersTurn;
        PlayerCombat.OnEndPlayersTurn -= OnEndPlayersTurn;
        PlayerCombat.OnDie -= OnDie;

        Unsubscribe();
    }

    #region Subscribing and Unsubscribing
    private void Subscribe()
    {
        PlayerCommandQueue.OnClear += OnClear;
        PlayerCommandQueue.OnAdd += OnAdd;
        PlayerCommandQueue.OnInsert += OnInsert;
        PlayerCommandQueue.OnRemove += OnRemove;
        PlayerCommandQueue.OnSwap += OnSwap;
        PlayerCommandQueue.OnSwap += OnSwap;
        DraggableManager.OnDrag += OnDrag;
    }

    private void Unsubscribe()
    {
        PlayerCommandQueue.OnClear -= OnClear;
        PlayerCommandQueue.OnAdd -= OnAdd;
        PlayerCommandQueue.OnInsert -= OnInsert;
        PlayerCommandQueue.OnRemove -= OnRemove;
        PlayerCommandQueue.OnSwap -= OnSwap;
        DraggableManager.OnDrag -= OnDrag;
    }
    #endregion

    private void OnBeginPlayersTurn()
    {
        overlay.DOFade(0, 0.25f);
        loadingThing.DOFade(0, 0.25f);
        executeButton.Enable();
        group.blocksRaycasts = true;

        _startEnergy = _player.Energy;
        Energy = _startEnergy;

        OnUpdateEnergy?.Invoke(Energy.Value);
    }

    private void OnEndPlayersTurn()
    {
        OnDie();

        Energy = null;
        _startEnergy = null;
    }

    private void OnDie()
    {
        overlay.DOFade(_overlayAlpha, 0.25f);
        executeButton.Disable();
        group.blocksRaycasts = false;
    }

    private void OnPlayerBeginExecute()
    {
        _emptySpace.Clear();
        UpdateUI();

        Unsubscribe();

        PlayerCommandQueue.Clear();

        _playerExecutingIndex = 0;

        executePointer.DOKill();
        executePointer.gameObject.SetActive(true);
        executePointer.DOSizeDelta(new(0, entrySize), 0.25f).SetEase(Ease.OutExpo);
        executePointer.anchoredPosition = new(0, 0);

        pointerImage.DOFade(pointerExecuteAlpha, 0.25f);

        group.blocksRaycasts = false;
    }

    private void OnPlayerEndExecute()
    {
        loadingThing.DOFade(1, 0.25f);

        PlayerCommandQueue.Clear();

        int count = _entries.Count;

        if (count > 0)
        {
            _entries[_entries.Count - 1].Remove(false);
        }

        _entries.Clear();

        Subscribe();

        executePointer.DOSizeDelta(new(0, 0), 0.25f).SetEase(Ease.InExpo).onComplete = () =>
        {
            executePointer.gameObject.SetActive(false);
        };

        pointerImage.DOFade(_pointerDefaultAlpha, 0.25f);

        // end the players turn
        PlayerCombat.IsPlayersTurn = false;
    }

    private void OnPlayerExecuteCommand(int index, Command command)
    {
        _playerExecutingIndex = index;

        _entries[index].Execute(command.Duration);

        if (index > 0)
        {
            _entries[index - 1].Remove(false);
        }

        executePointer.DOAnchorPosY(IndexToYPos(index), 0.25f).SetEase(Ease.OutExpo);
    }

    private CommandQueueUIEntry CreateEntry()
    {
        CommandQueueUIEntry entry = Instantiate(entryTemplate, entryParent);
        RectTransform rect = entry.transform as RectTransform;

        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, entrySize);

        entry.Parent = this;
        entry.Disable();

        return entry;
    }

    private void OnGetEntry(CommandQueueUIEntry entry) => entry.Enable();
    private void OnReleaseEntry(CommandQueueUIEntry entry) => entry.Disable();

    private void OnClear()
    {
        foreach (CommandQueueUIEntry entry in _entries)
        {
            entry.Remove();
            _pool.Release(entry);
        }

        _entries.Clear();

        UpdateEnergy();
    }

    private void OnAdd()
    {
        CommandQueueUIEntry entry = _pool.Get();
        _entries.Add(entry);
        entry.Create(_nextWasDragged);
        _nextWasDragged = false;

        SetDirty();

        UpdateEnergy();
    }

    private void OnInsert(int index)
    {
        CommandQueueUIEntry entry = _pool.Get();
        _entries.Insert(index, entry);
        entry.Create(_nextWasDragged);
        _nextWasDragged = false;

        SetDirty();

        UpdateEnergy();
    }

    private void OnRemove(int index)
    {
        CommandQueueUIEntry entry = _entries[index];
        entry.Remove(InstantlyDestroyNextCommand);
        InstantlyDestroyNextCommand = true;
        _entries.RemoveAt(index);

        SetDirty();

        UpdateEnergy();
    }

    private void OnSwap(int a, int b)
    {
        CommandQueueUIEntry entryA = _entries[a];
        CommandQueueUIEntry entryB = _entries[b];

        _entries[a] = entryB;
        _entries[b] = entryA;

        SetDirty();
    }

    public void OnDrop(DraggableObject draggableObj)
    {
        InstantlyDestroyNextDraggable = false;

        EmptySpaceIndex = null;
        _commandHover = false;

        if (draggableObj == null) return;
        if (!Energy.HasValue) return;

        foreach (IDraggable draggable in draggableObj.Draggables)
        {
            if (draggable is DraggableCommand)
            {
                DraggableCommand draggableCommand = draggable as DraggableCommand;

                Command command = draggableCommand.Command;

                if (command.Energy > Energy.Value)
                {
                    CommandQueueConsole.Instance.TriggerNoEnergy();
                    continue;
                }

                InstantlyDestroyNextDraggable = true;

                _nextWasDragged = true;
                int index = _mouseIndex;
                PlayerCommandQueue.Insert(index, command);
                break;
            }
        }
    }

    private void OnDrag(DraggableObject obj)
    {
        if (_mouseOnTop)
        {
            OnPointerEnter(null);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _mouseOnTop = true;
        DraggableObject draggableObj = DraggableManager.CurrentDraggable;
        if (draggableObj == null) return;

        foreach (IDraggable draggable in draggableObj.Draggables)
        {
            if (draggable is DraggableCommand)
            {
                _commandHover = true;
                _oldMousePos = null;
                UpdateEmptySpace();
                return;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _mouseOnTop = false;
        EmptySpaceIndex = null;
        _commandHover = false;
    }

    private void Update()
    {
        Vector2 mousePos = GameInput.UI.Point;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, mousePos, null, out Vector2 localPoint))
        {
            float y = localPoint.y - (_rect.rect.height / 2f);

            _mouseIndex = YPosToIndex(y + entrySize / 2f);
        }
        else
        {
            _mouseIndex = -1;
        }

        bool mousePosChanged = !_oldMousePos.HasValue || _oldMousePos.Value != mousePos;
        _oldMousePos = mousePos;

        if (_commandHover && mousePosChanged)
        {
            UpdateEmptySpace();
        }

        // Delete button
        int deleteButtonIndex;

        if (!_mouseOnTop || _mouseIndex >= PlayerCommandQueue.Count || DraggableManager.IsDragging)
        {
            deleteButtonIndex = -1;
        }
        else
        {
            deleteButtonIndex = _mouseIndex;
        }

        if (_oldDeleteButtonIndex != deleteButtonIndex)
        {
            HideDeleteButton(_oldDeleteButtonIndex);

            _oldDeleteButtonIndex = deleteButtonIndex;

            ShowDeleteButton(deleteButtonIndex);
        }

        // POINTER & EXECUTION pointers
        void DisablePointer()
        {
            if (_pointerIndex.HasValue)
            {
                if (_pointerSizeTween != null) _pointerSizeTween.Kill();
                _pointerSizeTween = pointer.DOSizeDelta(Vector2.zero, 0.1f).SetEase(Ease.OutSine);

                _pointerIndex = null;
            }
        }

        if (_mouseOnTop || _playerExecuting)
        {
            int index;

            if (_playerExecuting)
            {
                index = _playerExecutingIndex;
            }
            else
            {
                index = _mouseIndex;
            }

            if (index >= -1)
            {
                if (!_pointerIndex.HasValue)
                {
                    if (_pointerSizeTween != null) _pointerSizeTween.Kill();
                    _pointerSizeTween = pointer.DOSizeDelta(new(0, entrySize), 0.1f).SetEase(Ease.OutSine);
                }

                if (!_pointerIndex.HasValue || _pointerIndex.Value != index)
                {
                    _pointerIndex = index;

                    float y = IndexToYPos(index);

                    if (_pointerPositionTween != null) _pointerPositionTween.Kill();

                    if (pointer.sizeDelta.y <= 0.1f)
                    {
                        pointer.anchoredPosition = new(0, y);
                        _pointerPositionTween = null;
                    }
                    else
                    {
                        _pointerPositionTween = pointer.DOAnchorPosY(y, 0.5f).SetEase(Ease.OutExpo);
                        SoundManager.PlaySound("command_queue_hover");
                    }
                }
            }
            else
            {
                DisablePointer();
            }
        }
        else
        {
            DisablePointer();
        }
    }

    private void LateUpdate()
    {
        if (_playerExecuting) return;

        if (_dirty)
        {
            UpdateUI();
            _dirty = false;
        }
    }

    public void SetDirty() => _dirty = true;

    private void UpdateUI()
    {
        int count = PlayerCommandQueue.Count;
        if (count <= 0) return;

        float yPos = entrySize / -2f;

        int spacing;
        void EmptySpace(int amount) => yPos -= (entrySize + entrySpacing) * (float)amount;

        if (_emptySpace.TryGetValue(-1, out spacing)) EmptySpace(spacing);

        for (int i = 0; i < count; i++)
        {
            CommandQueueUIEntry entry = _entries[i];
            entry.SetYPos(yPos, entry.IsNew);
            entry.IsNew = false;

            yPos -= entrySize + entrySpacing;

            if (_emptySpace.TryGetValue(i, out spacing)) EmptySpace(spacing);
        }
    }

    #region Index and YPos conversion
    public int YPosToIndex(float yPos)
    {
        int index = 0;

        for (int i = 0; i < maxCommandsAllowed; i++)
        {
            float prevY = IndexToYPos(i - 1);
            float thisY = IndexToYPos(i);

            if (yPos <= prevY && yPos > thisY)
            {
                return i;
            }
        }

        if (yPos <= IndexToYPos(maxCommandsAllowed - 1))
        {
            return maxCommandsAllowed - 1;
        }

        return index;
    }

    public float IndexToYPos(int index)
    {
        if (index < 0)
        {
            return 0;
        }

        float yPos = entrySize / -2f;
        yPos -= (entrySize + entrySpacing) * (float)index;

        return yPos;
    }

    /*
    public int YPosToEntryIndex(float y)
    {
        int count = PlayerCommandQueue.Count;
        if (count == 0) return -1;

        if (count == 1)
        {
            CommandQueueUIEntry entry = _entries[0];

            return y >= entry.YPosCanvas ? -1 : 0;
        }

        if (y >= _entries[0].YPosCanvas)
        {
            return -1;
        }

        int index = -1;

        for (int i = 0; i < count; i++)
        {
            float thisY = _entries[i].YPosCanvas;

            if (y <= thisY)
            {
                index = i;
            }
        }

        return index;
    }
    */
    #endregion

    private void UpdateEnergy()
    {
        if (!_startEnergy.HasValue) return;

        int energy = _startEnergy.Value;

        for (int i = 0; i < PlayerCommandQueue.Count; i++)
        {
            energy -= PlayerCommandQueue.List[i].Energy;
        }

        energy = Mathf.Clamp(energy, 0, _player.MaxEnergy);
        Energy = energy;

        OnUpdateEnergy?.Invoke(energy);
    }

    #region Empty Space
    private void UpdateEmptySpace()
    {
        EmptySpaceIndex = _mouseIndex - 1;
    }

    public void AddEmptySpace(int index)
    {
        if (_emptySpace.ContainsKey(index))
        {
            _emptySpace[index]++;
        }
        else
        {
            _emptySpace[index] = 1;
        }

        SetDirty();
    }

    public bool RemoveEmptySpace(int index)
    {
        if (_emptySpace.ContainsKey(index))
        {
            _emptySpace[index]--;

            if (_emptySpace[index] <= 0)
            {
                ClearEmptySpace(index);
            }

            SetDirty();

            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ClearEmptySpace(int index)
    {
        bool success = _emptySpace.Remove(index);

        if (success)
        {
            SetDirty();
        }

        return success;
    }
    #endregion

    public int IndexOf(CommandQueueUIEntry entry)
    {
        return _entries.IndexOf(entry);
    }

    #region Delete Buttons
    public void DeleteEntry(int index, bool instant = false)
    {
        if (index < 0 || index >= PlayerCommandQueue.Count) return;

        Debug.Log("DELETING: " + index);

        CommandQueueUIEntry entry = _entries[index];

        entry.DeleteCommand(instant);
    }

    private void ShowDeleteButton(int index)
    {
        if (index < 0) return;

        CustomButton button = _deleteButtons[index];
        button.Interactable = true;

        button.transform.DOKill();
        button.transform.DOScaleY(1, 0.25f).SetEase(Ease.OutExpo);
    }

    private void HideDeleteButton(int index)
    {
        if (index < 0) return;

        CustomButton button = _deleteButtons[index];
        button.Interactable = false;

        button.transform.DOKill();
        button.transform.DOScaleY(0, 0.25f).SetEase(Ease.InExpo);
    }
    #endregion

    #region Debug
    public void OnDebugGUI()
    {
        DebugGUI.Property("Energy", Energy.HasValue ? Energy.Value.ToString() : "null");
        DebugGUI.Property("Mouse Y", DraggableManager.MousePosCanvas.y);
        DebugGUI.Property("Mouse I", _mouseIndex);
        DebugGUI.Space();

        int count = PlayerCommandQueue.Count;
        for (int i = 0; i < count; i++)
        {
            DebugGUI.Property(i.ToString(), _entries[i].YPosCanvas);
        }
    }
    #endregion
}
