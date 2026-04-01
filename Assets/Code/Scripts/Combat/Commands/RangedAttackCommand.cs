using UnityEngine;

public class RangedAttackCommand : Command
{
    [SerializeField] private Projectile projectile;

    public override void Execute(Unit user)
    {
        // TODO: Make this work on the player someday
        if (user is PlayerCombat)
        {
            return;
        }

        PlayerCombat target = PlayerCombat.Instance;

        Projectile projectile = Instantiate(this.projectile);
        projectile.User = user;
        projectile.Target = target;
        projectile.TravelTime = Duration;

        user.UnitToDamage = target;
        user.Attack(Duration);
    }
}
