using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[SingletonMode(true)]
public class SceneTransition : Singleton<SceneTransition>
{
    public static int CurrentSceneIndex => SceneManager.GetActiveScene().buildIndex;
    public static bool IsTransitioning { get; private set; } = false;

    [CacheComponent]
    [SerializeField] private UIDissolve dissolve;
    [Space]
    [SerializeField] private float transitionTime;
    [SerializeField] private Ease ease = Ease.OutExpo;

    protected override void Awake()
    {
        base.Awake();

        dissolve.DissolveAmount = 0;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private IEnumerator Start()
    {
        yield return CoroutineUtility.GetWait(1f);

        Disappear(2f);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        Disappear();
    }

    private void Disappear(float? duration = null)
    {
        IsTransitioning = false;

        dissolve.DOKill();
        dissolve.TweenDissolveAmount(1, duration.HasValue ? duration.Value : transitionTime).SetEase(ease);
    }

    public static void Goto(int sceneIndex, float delay = 0)
    {
        Instance.GotoLocal(sceneIndex, delay);
    }

    private void GotoLocal(int sceneIndex, float delay = 0)
    {
        IsTransitioning = true;

        dissolve.DOKill();
        dissolve.TweenDissolveAmount(0, transitionTime).SetDelay(delay).SetEase(ease).onComplete = () =>
        {
            SceneManager.LoadScene(sceneIndex);
        };
    }
}
