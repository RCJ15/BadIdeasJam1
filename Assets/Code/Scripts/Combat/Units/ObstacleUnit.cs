using DG.Tweening;
using UnityEngine;

public class ObstacleUnit : Unit
{
    public override void Die()
    {
        Destroy(gameObject);
    }

    protected override void BeginExecute()
    {

    }

    protected override void EndExecute()
    {

    }

    protected override void ExecuteCommand(int index, Command command)
    {

    }
}
