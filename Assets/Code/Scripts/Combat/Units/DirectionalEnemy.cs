using UnityEngine;

public class DirectionalEnemy : EnemyCombat
{
    [SerializeField] private Direction targetDirection;

    private Tile DirectionalTileTarget()
    {
        Direction playerDir = _player.FacingDirection;
        var playerRelativeDirection = targetDirection switch
        {
            Direction.Down => playerDir.Opposite(),
            Direction.Left => playerDir.RotateCounterClockwise(),
            Direction.Right => playerDir.RotateClockwise(),
            _ => playerDir,
        };
        return _player.GetTile(playerRelativeDirection);
    }

    protected override Tile TargetTile(Tile currentTile)
    {
        Tile playerTile = _player.Tile;
        Tile tileTarget = DirectionalTileTarget();

        if (tileTarget != null && (!tileTarget.Occupied || tileTarget.GridPos == currentTile.GridPos))
        {
            return tileTarget;
        }

        return base.TargetTile(currentTile);
    }

    protected override bool CanAttackPlayer(Tile currentTile)
    {
        Tile tileTarget = DirectionalTileTarget();
        return tileTarget != null && currentTile.GridPos == tileTarget.GridPos;
    }
}
