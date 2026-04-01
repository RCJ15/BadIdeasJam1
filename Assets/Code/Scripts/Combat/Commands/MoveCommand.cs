using DG.Tweening;
using UnityEngine;

public class MoveCommand : Command
{
    public bool IsFirstInLine { get; set; } = false;

    //[SerializeField] private float firstInLineDuration = 0.5f;
    [SerializeField] private int distance = 1;
    [SerializeField] private AnimationCurve firstInLineCurve;

    public override void Execute(Unit user)
    {
        user.MoveDuration = Duration;
        Tween tween = user.MoveToTile(user.GridPos + (user.FacingDirection.ToVector2Int() * distance));

        if (tween != null)
        {
            if (IsFirstInLine)
            {
                tween.SetEase(firstInLineCurve);
            }
            else
            {
                tween.SetEase(Ease.Linear);
            }
        }
    }
}
