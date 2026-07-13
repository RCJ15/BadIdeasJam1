public class RangedEnemy : EnemyCombat
{
    protected override Tile TargetTile(Tile currentTile)
    {
        // Go away from the player
        Tile playerTile = _player.Tile;

        float highestDist = 0;
        Tile furthestTile = null;

        foreach (Tile tile in board.Tiles)
        {
            if (tile == currentTile) continue;

            float dist = TileUtility.Distance(playerTile, tile);

            if (furthestTile == null || dist > highestDist)
            {
                highestDist = dist;
                furthestTile = tile;
            }
        }

        return furthestTile;
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
