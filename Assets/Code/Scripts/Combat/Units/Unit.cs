using Input;
using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public abstract class Unit : MonoBehaviour
{
    public static readonly List<Unit> AllUnits = new List<Unit>();

    public Vector2Int GridPos => gridPos;
    public Tile Tile => tile;

    protected Vector2Int gridPos;
    protected Tile tile;
    protected Board board;

    public Action OnBeginExecute { get; set; }
    public Action OnEndExecute { get; set; }
    public ChangeDelegate<int> OnChangeHP { get; set; }

    public bool Invincible => invincible;
    public int HP
    {
        get => _hp;
        set         {
            if (_hp == value) return;

            int oldHp = _hp;
            _hp = Mathf.Clamp(value, 0, MaxHP);

            OnChangeHP?.Invoke(oldHp, _hp);
        }
    }
    public int MaxHP => _maxHp;
    public int Damage => damage;
    public int Knockback => knockback;

    [SerializeField] private bool invincible;
    [SerializeField] private int health;
    [SerializeField] private int damage;
    [SerializeField] private int knockback;
    private int _maxHp;
    private int _hp;

    [Space]
    [SerializeField] private Direction facingDirection;
    [SerializeField] protected Model model;
    private AnimEvents _animEvents;

    private Vector3 _previousTilePos;
    private float _tileDelta;

    private Tween _tileDeltaTween;
    private UnitMaterialEffect _materialEffect;

    public Direction FacingDirection
    {
        get => facingDirection;
        set
        {
            if (facingDirection == value) return;

            UpdateFacingDirection(facingDirection, value);
            facingDirection = value;
        }
    }

    public float MoveDuration { get; set; } = 0.5f;
    public float TurnAroundDuration { get; set; } = 0.5f;
    public Unit UnitToDamage { get; set; }

    protected GlobalUnitSettings unitSettings;

    private Tween _scaleTween;

    protected virtual void OnEnable()
    {
        AllUnits.Add(this);
    }

    protected virtual void OnDisable()
    {
        AllUnits.Remove(this);
    }

    protected virtual void Awake()
    {
        _maxHp = health;
        _hp = _maxHp;

        _animEvents = GetComponentInChildren<AnimEvents>(true);

        if (_animEvents != null)
        {
            _animEvents.OnAnimEvent += OnAnimEvent;
        }

        _materialEffect = gameObject.AddComponent<UnitMaterialEffect>();
    }

    protected virtual void OnDestroy()
    {
        if (_animEvents != null)
        {
            _animEvents.OnAnimEvent -= OnAnimEvent;
        }
    }

    protected virtual void Start()
    {
        unitSettings = GlobalUnitSettings.Instance;
        board = Board.Instance;

        // Snap to grid
        MoveToTile(board.ClampPos(board.WorldToGrid(transform.position)), true);

        UpdateFacingDirection(facingDirection, facingDirection, true);
    }

    public void Heal(int amount)
    {
        HP += amount;
        _materialEffect.HealEffect();
        Squish();

        SoundManager.PlaySound("heal");
    }

    public virtual void TakeDamage(int damage, int knockback, Unit from)
    {
        if (damage <= 0) return;
        if (Invincible) return;

        HP -= damage;

        _materialEffect.HurtEffect();
        Squish();

        if (HP <= 0)
        {
            model.SetTrigger("Death");
            Die();
        }
        else
        {
            model.SetTrigger("Hurt");
        }

        if (knockback > 0)
        {
            Vector2Int difference = gridPos - from.gridPos;
            MoveDuration = 0.4f;
            MoveToTile(gridPos + difference).SetEase(Ease.OutSine);
        }

        SoundManager.PlaySound("damage");

        Debug.Log("OUCH I LOST " + damage + " HP!");
    }

    private void Squish()
    {
        if (model == null)
        {
            return;
        }

        if (_scaleTween != null) _scaleTween.Kill();
        model.transform.localScale = Vector3.one * 0.5f;
        _scaleTween = model.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetTarget(null);
    }

    public abstract void Die();

    private void UpdateFacingDirection(Direction oldDir, Direction newDir, bool instant = false)
    {
        if (model == null) return;

        float angle = newDir switch
        {
            Direction.Up => 0f,
            Direction.Right => 90f,
            Direction.Down => 180f,
            Direction.Left => 270f,
            _ => 0f,
        };

        model.transform.DOKill();

        if (instant)
        {
            model.transform.rotation = Quaternion.Euler(0, angle, 0);
            return;
        }

        SoundManager.PlaySound("whirr");

        string animation;
        bool turnAround = false;

        if (oldDir.RotateClockwise() == newDir) animation = "TurnRight";
        else if (oldDir.RotateCounterClockwise() == newDir) animation = "TurnLeft";
        else
        {
            animation = "TurnAround";
            turnAround = true;
        }

        model.transform.DORotate(new(0, angle, 0), TurnAroundDuration).SetEase(turnAround ? Ease.OutBack : Ease.OutSine);

        float speed = 1 / TurnAroundDuration;
        model.SetFloat("TurnSpeed", speed);

        model.SetTrigger(animation);
    }

    public void ExecuteCommands(List<Command> commands)
    {
        StartCoroutine(ExecuteCommandsCoroutine(commands));
    }

    private IEnumerator ExecuteCommandsCoroutine(List<Command> commands)
    {
        BeginExecute();
        OnBeginExecute?.Invoke();

        int count = commands.Count;

        for (int i = 0; i < count; i++)
        {
            Command command = commands[i];

            MoveCommand moveCommand = command as MoveCommand;
            bool isMoveCommand = moveCommand != null;

            if (isMoveCommand)
            {
                moveCommand.IsFirstInLine = false;
            }

            if (model.HasAnim && model.GetBool("Walking") != isMoveCommand)
            {
                model.SetFloat("WalkSpeed", isMoveCommand ? 1f / command.Duration : 1f);
                model.SetBool("Walking", isMoveCommand);

                if (isMoveCommand)
                {
                    moveCommand.IsFirstInLine = true;
                }
            }

            ExecuteCommand(i, command);
            command.Execute(this);

            yield return CoroutineUtility.GetWait(command.Duration);
        }

        model.SetBool("Walking", false);

        EndExecute();
        OnEndExecute?.Invoke();
    }

    protected abstract void BeginExecute();
    protected abstract void EndExecute();

    protected abstract void ExecuteCommand(int index, Command command);

    protected virtual void LateUpdate()
    {
        if (tile == null) return;

        transform.position = Vector3.Lerp(_previousTilePos, tile.Pos, _tileDelta);
    }

    public Tween MoveToTile(Vector2Int pos, bool instant = false)
    {
        pos = board.ClampPos(pos);
        if (pos == gridPos) return null;

        Tile newTile = board.GetTile(pos);

        if (newTile == null) return null;
        if (newTile.Occupied) return null;

        gridPos = pos;

        if (tile != newTile)
        {
            if (tile != null) tile.Unit = null;

            tile = newTile;

            tile.Unit = this;
        }

        if (_tileDeltaTween != null) _tileDeltaTween.Kill();

        if (instant)
        {
            _tileDelta = 1f;
            return null;
        }

        _previousTilePos = transform.position;
        _tileDelta = 0f;
        _tileDeltaTween = DOTween.To(() => _tileDelta, x => _tileDelta = x, 1f, MoveDuration);

        return _tileDeltaTween;
    }

    public void Attack(float attackDuration)
    {
        if (model == null || !model.HasAnim)
        {
            AttackUnit();
        }
        else
        {
            model.SetFloat("AttackSpeed", 1f / attackDuration);
            model.SetTrigger("Attack");
        }
    }

    protected virtual void OnAnimEvent(string evt)
    {
        if (evt == "DealDamage")
        {
            AttackUnit();
        }
    }

    public void AttackUnit()
    {
        if (UnitToDamage == null) return;
        UnitToDamage.TakeDamage(Damage, Knockback, this);
    }

    public Tile GetTile(Direction direction)
    {
        return GetTile(direction.ToVector2Int());
    }

    public Tile GetTile(Vector2Int offset)
    {
        return board.GetTile(GridPos + offset);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawLine(transform.position, transform.position + facingDirection.ToVector3() * 0.5f);
    }
#endif
}
