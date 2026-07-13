using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyCombat : Unit
{
    public static readonly List<EnemyCombat> AllEnemies = new();

    private static readonly List<Tile> _path = new();

    [SerializeField] protected CommandReference attackCommand;

    [Tooltip("How many tiles this enemy can move per turn")]
    [SerializeField] protected int speed = 1;

    protected PlayerCombat _player;
    protected GlobalEnemySettings _settings;

    public int Speed => speed;
    public bool ExecutingCommands { get; private set; }

    protected Direction _currentDirection;
    protected Tile _currentTile;

    protected override void OnEnable()
    {
        base.OnEnable();
        AllEnemies.Add(this);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        AllEnemies.Remove(this);
    }

    protected override void Start()
    {
        base.Start();

        _player = PlayerCombat.Instance;
        _settings = GlobalEnemySettings.Instance;
    }

    private void Update()
    {
#if UNITY_EDITOR
        // DEBUG
        if (UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
        {
            Debug.Log("DEBUG: Executing \"" + name + "\" turn now!");
            StartCoroutine(PerformTurn());
        }
#endif
    }

    public IEnumerator PerformTurn()
    {
        List<Command> commands = new();

        // Move to player
        _currentTile = tile;
        _currentDirection = FacingDirection;

        void Attack()
        {
            if (CanAttackPlayer(_currentTile))
            {
                FaceTowardTile(commands, _currentTile, _player.Tile);

                commands.Add(attackCommand);
            }
        }

        IEnumerator Move()
        {
            return AddCommandsToMoveToTile(commands, _currentTile, TargetTile(_currentTile));
        }

        if (AttackFirst())
        {
            Attack();
            if (speed > 0)
            {
                yield return Move();
            }
        }
        else
        {
            if (speed > 0)
            {
                yield return Move();
            }
            Attack();
        }

        AddCommands(commands);
        ExecuteCommands(commands);
    }

    protected virtual bool AttackFirst() => false;

    protected virtual Tile TargetTile(Tile currentTile)
    {
        return _player.Tile;
    }

    protected virtual bool CanAttackPlayer(Tile currentTile)
    {
        return currentTile.IsAdjacentTo(_player.Tile);
    }

    protected virtual void AddCommands(List<Command> list) { }

    protected IEnumerator AddCommandsToMoveToTile(List<Command> list, Tile start, Tile target)
    {
        if (start == target || speed <= 0)
        {
            yield break;
        }

        bool targetOccupied = target.Occupied;

        Task task = EnemyPathfinding.FindPath(start, target, _path, destroyCancellationToken);
        yield return new WaitUntil(() => task.IsCompleted);

        // Navigate path using commands
        int count = _path.Count;

        if (count <= 0)
        {
            yield break;
        }

        int moveAmount = Mathf.Min(count - 1, speed);
        bool canReachTarget = moveAmount >= count - 1;

        for (int i = 0; i < moveAmount; i++)
        {
            Tile tile = _path[i];
            Tile nextTile = _path[i + 1];

            FaceTowardTile(list, tile, nextTile);

            list.Add(_settings.Move);
        }

        if (!canReachTarget)
        {
            Tile currentTile = _path[moveAmount];
            Tile lastTile = _path[moveAmount + 1];
            FaceTowardTile(list, currentTile, lastTile);

            _currentTile = currentTile;
        }
        else if (targetOccupied)
        {
            Tile lastTile = _path[count - 1];
            FaceTowardTile(list, lastTile, target);

            _currentTile = lastTile;
        }
        else
        {
            _currentTile = _path[count - 1];
        }
    }

    protected void FaceTowardTile(List<Command> list, Tile from, Tile to)
    {
        Direction direction = TileUtility.FaceToward(from, to, _currentDirection);

        AddCommandForDirection(list, _currentDirection, direction);
        _currentDirection = direction;
    }

    protected void AddCommandForDirection(List<Command> list, Direction from, Direction to)
    {
        // No command needed
        if (from == to) return;

        switch (from)
        {
            case Direction.Up:
                switch (to)
                {
                    default:
                        return;

                    case Direction.Down:
                        list.Add(_settings.TurnAround);
                        return;

                    case Direction.Left:
                        list.Add(_settings.TurnLeft);
                        return;

                    case Direction.Right:
                        list.Add(_settings.TurnRight);
                        return;
                }

            case Direction.Down:
                switch (to)
                {
                    case Direction.Up:
                        list.Add(_settings.TurnAround);
                        return;

                    default:
                        return;

                    case Direction.Left:
                        list.Add(_settings.TurnRight);
                        return;

                    case Direction.Right:
                        list.Add(_settings.TurnLeft);
                        return;
                }

            case Direction.Left:
                switch (to)
                {
                    case Direction.Up:
                        list.Add(_settings.TurnRight);
                        return;

                    case Direction.Down:
                        list.Add(_settings.TurnLeft);
                        return;

                    default:
                        return;

                    case Direction.Right:
                        list.Add(_settings.TurnAround);
                        return;
                }

            case Direction.Right:
                switch (to)
                {
                    case Direction.Up:
                        list.Add(_settings.TurnLeft);
                        return;

                    case Direction.Down:
                        list.Add(_settings.TurnRight);
                        return;

                    case Direction.Left:
                        list.Add(_settings.TurnAround);
                        return;

                    default:
                        return;
                }
        }
    }

    protected override void BeginExecute()
    {
        ExecutingCommands = true;
    }

    protected override void EndExecute()
    {
        ExecutingCommands = false;
    }

    protected override void ExecuteCommand(int index, Command command)
    {
        SoundManager.PlaySound("delete_command");
    }

    public override void Die()
    {
        model.gameObject.AddComponent<EnemyDeath>();

        TooltipProvider tooltip = GetComponentInChildren<TooltipProvider>(true);

        if (tooltip != null)
        {
            tooltip.enabled = false;
        }

        this.enabled = false;
    }
}
