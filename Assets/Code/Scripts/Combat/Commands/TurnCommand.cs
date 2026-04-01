using UnityEngine;

public class TurnCommand : Command
{
    [SerializeField] private Turn direction;

    public override void Execute(Unit user)
    {
        user.TurnAroundDuration = Duration;
        user.FacingDirection = GetDirection(user.FacingDirection);
    }

    private Direction GetDirection(Direction userDirection)
    {
        switch (direction)
        {
            case Turn.Right:
                switch (userDirection)
                {
                    default:
                    case Direction.Up:
                        return Direction.Right;

                    case Direction.Down:
                        return Direction.Left;

                    case Direction.Left:
                        return Direction.Up;

                    case Direction.Right:
                        return Direction.Down;
                }

            case Turn.Left:
                switch (userDirection)
                {
                    default:
                    case Direction.Up:
                        return Direction.Left;

                    case Direction.Down:
                        return Direction.Right;

                    case Direction.Left:
                        return Direction.Down;

                    case Direction.Right:
                        return Direction.Up;
                }

            default:
            case Turn.Around:
                switch (userDirection)
                {
                    default:
                    case Direction.Up:
                        return Direction.Down;

                    case Direction.Down:
                        return Direction.Up;

                    case Direction.Left:
                        return Direction.Right;

                    case Direction.Right:
                        return Direction.Left;
                }
        }
    }

    enum Turn
    {
        Right,
        Left,
        Around,
    }
}
