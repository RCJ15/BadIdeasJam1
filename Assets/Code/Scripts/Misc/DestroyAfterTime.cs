using System.Collections;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField] private float lifetime = 1;
    [SerializeField] private bool unscaled;

    protected virtual IEnumerator Start()
    {
        if (unscaled)
        {
            yield return CoroutineUtility.GetWaitRealtime(lifetime);
        }
        else
        {
            yield return CoroutineUtility.GetWait(lifetime);
        }

        Destroy();
    }

    protected virtual void Destroy()
    {
        Destroy(gameObject);
    }
}
