using UnityEngine;

public class StaticTooltip : MonoBehaviour, ITooltip
{
    [SerializeField] protected string title = "Title";
    [TextArea(5, 10)]
    [SerializeField] protected string description = "Description";

    public virtual string GetTooltipTitle()
    {
        return title;
    }

    public virtual string GetTooltipDescription()
    {
        return description;
    }
}
