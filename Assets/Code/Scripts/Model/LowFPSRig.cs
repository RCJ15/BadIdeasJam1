using System;
using System.Collections.Generic;
using UnityEngine;

public class LowFPSRig : MonoBehaviour
{
    public const float LOW_FPS = 15;

    private static readonly HashSet<Type> _visualsTypes = new HashSet<Type>()
    {
        typeof(SpriteRenderer),
        typeof(MeshRenderer),
        typeof(LineRenderer),
        typeof(Line),
    };

    private static readonly HashSet<Type> _disableTypes = new HashSet<Type>()
    {
        typeof(FaceCamera),
        typeof(Mouth),
    };

    [SerializeField] private GameObject rig;
    private GameObject _newRig;
    //[SerializeField] private float fps = 60f;
    private float _rigTimer;

    private ObjectPair[] _objectPairs;

    private float _timeBtwRigUpdates;

    private List<(Transform from, Transform to)> _forceUpdateRotation = new();

    private void Start()
    {
        _timeBtwRigUpdates = 1f / LOW_FPS;

        _newRig = Instantiate(rig, transform);
        _newRig.transform.position = rig.transform.position;

        _newRig.name = "VisualRig";

        List<ObjectPair> pairs = new();
        List<Component> componentList = new();

        void AddChildren(Transform parent, Transform other)
        {
            foreach (Transform child in parent)
            {
                Transform to = other.GetChild(child.GetSiblingIndex());

                pairs.Add(new TransformPair(child, to));

                if (child.TryGetComponent(out Renderer childRenderer))
                {
                    pairs.Add(new RendererPair(childRenderer, to.GetComponent<Renderer>()));
                }

                if (child.TryGetComponent(out SpriteRenderer childSR))
                {
                    pairs.Add(new SpritePair(childSR, to.GetComponent<SpriteRenderer>()));
                }

                if (child.TryGetComponent(out Claw childClaw))
                {
                    pairs.Add(new ClawPair(childClaw, to.GetComponent<Claw>()));
                }

                foreach (Type type in _disableTypes)
                {
                    if (child.TryGetComponent(type, out Component component))
                    {
                        Destroy(to.GetComponent(type));

                        if (component is FaceCamera) _forceUpdateRotation.Add((child, to));
                    }
                }

                foreach (Type type in _visualsTypes)
                {
                    child.GetComponents(type, componentList);

                    foreach (Component component in componentList)
                    {
                        Behaviour behaviour = component as Behaviour;
                        if (behaviour != null) behaviour.enabled = false;

                        Renderer renderer = component as Renderer;
                        if (renderer != null) renderer.enabled = false;
                    }
                }

                AddChildren(child, to);
            }
        }

        pairs.Add(new TransformPair(rig.transform, _newRig.transform));
        AddChildren(rig.transform, _newRig.transform);

        _objectPairs = pairs.ToArray();
    }

    private void LateUpdate()
    {
        if (_rigTimer <= 0)
        {
            _rigTimer = _timeBtwRigUpdates;

            foreach (ObjectPair objectPair in _objectPairs)
            {
                objectPair.Update();
            }
        }
        else
        {
            _rigTimer -= Time.deltaTime;
            return;
        }

        foreach (var pair in _forceUpdateRotation)
        {
            pair.from.rotation = pair.to.rotation;
        }
    }

    public void ForceUpdate()
    {
        _rigTimer = 0;
    }

    #region Object Pairs
    private abstract class ObjectPair
    {
        public abstract void Update();
    }

    private abstract class ObjectPair<T> : ObjectPair
    {
        public T From { get; private set; }
        public T To { get; private set; }


        public ObjectPair(T from, T to)
        {
            From = from;
            To = to;
        }
    }

    private class TransformPair : ObjectPair<Transform>
    {
        public TransformPair(Transform from, Transform to) : base(from, to) { }

        public override void Update()
        {
            To.gameObject.SetActive(From.gameObject.activeSelf);
            To.localPosition = From.localPosition;
            To.localRotation = From.localRotation;
            To.localScale = From.localScale;
        }
    }

    private class SpritePair : ObjectPair<SpriteRenderer>
    {
        public SpritePair(SpriteRenderer from, SpriteRenderer to) : base(from, to) { }

        public override void Update()
        {
            To.sprite = From.sprite;
            To.color = From.color;
        }
    }

    private class RendererPair : ObjectPair<Renderer>
    {
        public RendererPair(Renderer from, Renderer to) : base(from, to) { }

        public override void Update()
        {
            To.material = From.material;
        }
    }


    private class ClawPair : ObjectPair<Claw>
    {
        public ClawPair(Claw from, Claw to) : base(from, to) { }

        public override void Update()
        {
            To.Rotate = false;
            To.OpenAmount = From.OpenAmount;
        }
    }
    #endregion

}
