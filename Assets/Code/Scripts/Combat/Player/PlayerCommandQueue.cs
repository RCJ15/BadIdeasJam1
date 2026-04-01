using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCommandQueue : Singleton<PlayerCommandQueue>
{
    public static Action OnClear { get; set; }
    public static Action OnAdd { get; set; }
    public static Action<int> OnInsert { get; set; }
    public static Action<int> OnRemove { get; set; }
    public static Action<int, int> OnSwap { get; set; }

    public static Action OnTryAddCommandAtLimit { get; set; }

    public static int? Limit { get; set; }
    public static bool AtLimit => Limit.HasValue && Count >= Limit.Value;
    public static int Count { get; private set; }
    public static List<Command> List { get; private set; } = new();

    public static void Clear()
    {
        List.Clear();
        Count = 0;

        OnClear?.Invoke();
    }

    public static void Add(Command command)
    {
        if (AtLimit)
        {
            OnTryAddCommandAtLimit?.Invoke();
            return;
        }

        List.Add(command);
        Count++;

        OnAdd?.Invoke();
    }

    public static void Insert(int index, Command command)
    {
        if (AtLimit)
        {
            OnTryAddCommandAtLimit?.Invoke();
            return;
        }

        if (index < 0)
        {
            index = 0;
        }
        else if (index >= Count)
        {
            Add(command);
            return;
        }

        List.Insert(index, command);
        Count++;

        OnInsert?.Invoke(index);
    }

    public static void Remove(int index)
    {
        List.RemoveAt(index);
        Count--;

        OnRemove?.Invoke(index);
    }

    public static bool Remove(Command command)
    {
        int index = List.IndexOf(command);
        if (index < 0) return false;

        Remove(index);
        return true;
    }

    public static void SwapCommands(int a, int b)
    {
        if (a == b) return;

        Command command = List[b];
        List[b] = List[a];
        List[a] = command;

        OnSwap?.Invoke(a, b);
    }

    protected override void Awake()
    {
        base.Awake();

        List.Clear();
        Count = 0;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        List.Clear();
        Count = 0;
    }
}
