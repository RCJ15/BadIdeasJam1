using UnityEngine;

public class HPBarTooltip : MonoBehaviour, ITooltip
{
    public string GetTooltipTitle()
    {
        return "#HEALTH# Health #HEALTH#";
    }

    public string GetTooltipDescription()
    {
        return $"If this hits 0, you lose.";
    }
}
