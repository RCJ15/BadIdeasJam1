using DG.Tweening;
using Input;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : Unit
{
    public static PlayerCombat Instance { get; private set; }
    public static bool Dead { get; private set; }
    public static Action OnDie { get; set; }

    public static bool IsPlayersTurn
    {
        get => _isPlayersTurn;
        set
        {
            if (_isPlayersTurn == value) return;

            _isPlayersTurn = value;

            if (_isPlayersTurn)
            {
                Instance.TurnBegin();
                OnBeginPlayersTurn?.Invoke();
            }
            else
            {
                Instance.TurnEnd();
                OnEndPlayersTurn?.Invoke();
            }
        }
    }

    private static bool _isPlayersTurn { get; set; } = false;
    public static Action OnBeginPlayersTurn { get; set; }
    public static Action OnEndPlayersTurn { get; set; }

    public static bool ExecutingCommands { get; private set; }
    public static Action<int, Command> OnExecuteCommand { get; set; }

    public ChangeDelegate<int> OnEnergyChanged { get; set; }

    public int Energy
    {
        get => _energy;
        set
        {
            if (_energy == value) return;

            int oldEnergy = _energy;
            _energy = Mathf.Clamp(value, 0, MaxEnergy);

            OnEnergyChanged?.Invoke(oldEnergy, _energy);
        }
    }
    public int MaxEnergy => capacity;

    public int Recharge => recharge;

    [Space]
    [SerializeField] private int capacity;
    [SerializeField] private int recharge;

    private int _energy;
    private bool _isFirstTurn;


    protected override void Awake()
    {
        base.Awake();

        Instance = this;
        ExecutingCommands = false;

        _energy = Mathf.CeilToInt((float)MaxEnergy / 2f);
        _isFirstTurn = true;

        Dead = false;
        ExecutingCommands = false;
        _isPlayersTurn = false;
    }

    public override void Die()
    {
        Debug.Log("PLAYER DIED GAME OVER!!");

        MusicPlayer.StopAll();
        Dead = true;

        model.transform.DOKill();

        BigText.Appear("Game over...", 1);
        SceneTransition.Goto(SceneTransition.CurrentSceneIndex, 2f);

        OnDie?.Invoke();

        this.enabled = false;
    }

    protected override void BeginExecute()
    {
        ExecutingCommands = true;
    }

    protected override void EndExecute()
    {
        ExecutingCommands = false;
    }

    protected override void ExecuteCommand(int index, Command command)
    {
        OnExecuteCommand?.Invoke(index, command);
        Energy -= command.Energy;
    }

    private void TurnBegin()
    {
        if (_isFirstTurn)
        {
            _isFirstTurn = false;
            return;
        }

        // Recover energy
        Energy += recharge;

        SoundManager.PlaySound("energy");
    }

    private void TurnEnd()
    {

    }
}
