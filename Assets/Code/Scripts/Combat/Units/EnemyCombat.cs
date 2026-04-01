using System.Collections.Generic;
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
            PerformTurn();
        }
#endif
    }

    public void PerformTurn()
    {
        List<Command> commands = new();

        // Move to player
        Tile currentTile = tile;
        Tile playerTile = _player.Tile;
        Direction currentDirection = FacingDirection;
        Direction newDirection;

        void Attack()
        {
            if (CanAttackPlayer(currentTile))
            {
                FaceTowardTile(commands, currentDirection, currentTile, playerTile, out newDirection);
                currentDirection = newDirection;

                commands.Add(attackCommand);
            }
        }

        void Move()
        {
            AddCommandsToMoveToTile(commands, tile, TargetTile(currentTile), currentDirection, out currentTile, out newDirection);
            currentDirection = newDirection;
        }

        if (AttackFirst())
        {
            Attack();
            Move();
        }
        else
        {
            Move();
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

    protected void AddCommandsToMoveToTile(List<Command> list, Tile start, Tile target, Direction currentDirection, out Tile destination, out Direction direction)
    {
        if (start == target)
        {
            direction = currentDirection;
            destination = start;
            return;
        }

        bool targetOccupied = target.Occupied;
        EnemyPathfinding.FindPath(start, target, _path);

        // Navigate path using commands
        int count = _path.Count;

        if (count <= 0)
        {
            direction = currentDirection;
            destination = start;
            return;
        }

        currentDirection = FacingDirection;

        int moveAmount = Mathf.Min(count - 1, speed);
        bool canReachTarget = moveAmount >= count - 1;

        for (int i = 0; i < moveAmount; i++)
        {
            Tile tile = _path[i];
            Tile nextTile = _path[i + 1];

            FaceTowardTile(list, currentDirection, tile, nextTile, out Direction newDirection);
            currentDirection = newDirection;

            list.Add(_settings.Move);
        }

        if (!canReachTarget)
        {
            Tile currentTile = _path[moveAmount];
            Tile lastTile = _path[moveAmount + 1];
            FaceTowardTile(list, currentDirection, currentTile, lastTile, out Direction newDirection);
            currentDirection = newDirection;

            destination = currentTile;
        }
        else if (targetOccupied)
        {
            Tile lastTile = _path[count - 1];
            FaceTowardTile(list, currentDirection, lastTile, target, out Direction newDirection);
            currentDirection = newDirection;

            destination = lastTile;
        }
        else
        {
            destination = _path[count - 1];
        }

        direction = currentDirection;
    }

    protected void FaceTowardTile(List<Command> list, Direction currentDirection, Tile from, Tile to, out Direction direction)
    {
        Vector2Int fromPos = from.GridPos;
        Vector2Int toPos = to.GridPos;

        Vector2Int difference = toPos - fromPos;
        direction = Vector2IntToDirection(difference);

        AddCommandForDirection(list, currentDirection, direction);
    }

    protected Direction Vector2IntToDirection(Vector2Int vector2Int)
    {
        if (vector2Int.magnitude > 1.5f)
        {
            vector2Int = Vector2Int.FloorToInt((Vector2)vector2Int / vector2Int.magnitude);
        }

        // Down
        if (vector2Int == Vector2Int.down)
        {
            return Direction.Down;
        }
        // Left
        else if (vector2Int == Vector2Int.left)
        {
            return Direction.Left;
        }
        // Right
        else if (vector2Int == Vector2Int.right)
        {
            return Direction.Right;
        }
        // Up (default)
        else
        {
            return Direction.Up;
        }
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
