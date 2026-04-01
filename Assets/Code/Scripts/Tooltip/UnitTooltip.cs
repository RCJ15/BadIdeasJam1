using UnityEngine;

public class UnitTooltip : StaticTooltip
{
    [CacheComponent(CacheMethod.InParentInactive)]
    [SerializeField] private Unit unit;

    public override string GetTooltipTitle()
    {
        bool isEnemy = unit is EnemyCombat;

        string tooltip = base.GetTooltipTitle() + $" - <color=#FF494B>{unit.HP}#HEALTH#</color>";

        if (!(unit is ObstacleUnit))
        {
            tooltip += $" <color=white>{unit.Damage}#SWORD#</color>";
        }

        if (isEnemy)
        {
            tooltip += $" <color=#86C691>{(unit as EnemyCombat).Speed}#SPEED#</color>";
        }

        return tooltip;
    }

    public override string GetTooltipDescription()
    {
        string description = base.GetTooltipDescription();

        if (!(unit is ObstacleUnit))
        {
            description += $"\nCurrently facing {DirectionToString(unit.FacingDirection)}.";
        }

        return description;
    }

    private string DirectionToString(Direction direction)
    {
        switch (direction)
        {
            default:
            case Direction.Up:
                return "north";

            case Direction.Down:
                return "south";

            case Direction.Left:
                return "west";

            case Direction.Right:
                return "east";
        }
    }
}
