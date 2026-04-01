public interface ICommand
{
    public int Energy { get; }
    public void Execute(Unit user);
}
