[SingletonMode(true)]
public class GlobalEnemySettings : Singleton<GlobalEnemySettings>
{
    public CommandReference Move;
    public CommandReference TurnLeft;
    public CommandReference TurnRight;
    public CommandReference TurnAround;
}
