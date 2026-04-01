using UnityEngine;

public class EnergyBarTooltip : MonoBehaviour, ITooltip
{
    public string GetTooltipTitle()
    {
        return "#ENERGY# Energy #ENERGY#";
    }

    public string GetTooltipDescription()
    {
        return $"Every command will cost energy when used.\nYou will gain {PlayerCombat.Instance.Recharge}#ENERGY# at the start of your next turn.";
    }
}
