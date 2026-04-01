public interface ITooltip
{
    public string GetTooltipTitle();
    public string GetTooltipDescription();

    public bool ShowTooltip() => true;
}
