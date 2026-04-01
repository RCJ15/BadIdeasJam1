using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    [CacheComponent]
    [SerializeField] private SpriteRenderer sprite;

    [Space]
    [SerializeField] private Color[] colors;
    [SerializeField] private float timeBtwColors;

    private int _length;

    private IEnumerator Start()
    {
        _length = colors.Length;

        while (true)
        {
            for (int i = 0; i < _length; i++)
            {
                sprite.DOColor(colors[i], timeBtwColors);

                yield return CoroutineUtility.GetWait(timeBtwColors);
            }
        }
    }
}
