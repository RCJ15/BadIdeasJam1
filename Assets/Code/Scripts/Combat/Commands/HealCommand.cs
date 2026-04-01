using UnityEngine;

public class HealCommand : Command
{
    [SerializeField] private int amount;

    public override void Execute(Unit user)
    {
        user.Heal(amount);
    }
}
