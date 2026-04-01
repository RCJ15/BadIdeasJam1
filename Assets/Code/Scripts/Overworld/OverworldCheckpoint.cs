using System.Collections.Generic;
using UnityEngine;

public class OverworldCheckpoint : MonoBehaviour
{
    public static readonly Dictionary<int, OverworldCheckpoint> Checkpoints = new();

    public int ID => id;

    [SerializeField] private int id;

    private void Awake()
    {
        Checkpoints.Add(id, this);
    }

    private void OnDestroy()
    {
        Checkpoints.Remove(id);
    }
}
