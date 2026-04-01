public class MeleeAttackCommand : Command
{
    public override void Execute(Unit user)
    {
        // Melee attack in the direction the user is facing
        Direction direction = user.FacingDirection;

        // Get the tile in that direction
        Tile tile = Board.GetTile(user.GridPos + direction.ToVector2Int());

        // No tile or unit
        if (tile == null || tile.Unit == null)
        {
            user.UnitToDamage = null;
            user.Attack(Duration);
            return;
        }

        user.UnitToDamage = tile.Unit;
        user.Attack(Duration);
    }
}
