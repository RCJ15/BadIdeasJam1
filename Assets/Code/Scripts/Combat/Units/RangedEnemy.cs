using UnityEngine;

public class RangedEnemy : EnemyCombat
{
    protected override Tile TargetTile(Tile currentTile)
    {
        // Go away from the player
        Vector2Int direction = currentTile.GridPos - _player.GridPos;

        direction.x = Mathf.Clamp(direction.x, -1, 1);
        direction.y = Mathf.Clamp(direction.y, -1, 1);

        return board.GetTile(board.ClampPos(currentTile.GridPos + direction * speed));
    }

    protected override bool CanAttackPlayer(Tile currentTile)
    {
        return !base.CanAttackPlayer(currentTile);
    }

    protected override bool AttackFirst()
    {
        return true;
    }
}
