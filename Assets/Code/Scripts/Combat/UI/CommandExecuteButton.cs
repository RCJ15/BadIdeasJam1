using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class CommandExecuteButton : MonoBehaviour, ITooltip
{
    [CacheComponent]
    [SerializeField] private CustomButton button;

    [Space]
    [SerializeField] private Graphic[] graphics;
    [SerializeField] private TMP_Text energyText;
    private int _graphicsLength;
    [SerializeField] private Color disableColor = Color.gray;
    private Color[] _startColors;

    private PlayerCombat _player;
    private CommandQueueConsole _console;

    private void Start()
    {
        _graphicsLength = graphics.Length;
        _startColors = new Color[_graphicsLength];

        for (int i = 0; i < _graphicsLength; i++)
        {
            _startColors[i] = graphics[i].color;
        }

        Disable(true);

        _player = PlayerCombat.Instance;
        _console = CommandQueueConsole.Instance;

        string format = energyText.text;
        energyText.text = string.Format(format, _player.Recharge.ToString());

        button.OnClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (PlayerCombat.Dead)
        {
            return;
        }

        if (PlayerCombat.ExecutingCommands)
        {
            _console.TriggerCurrentlyExecutingCommands();
            return;
        }

        if (!PlayerCombat.IsPlayersTurn)
        {
            _console.TriggerNotPlayersTurn();
            return;
        }

        if (PlayerCommandQueue.Count <= 0)
        {
            _console.TriggerNothingToExecute();
            return;
        }

        List<Command> commands = new();

        foreach (Command command in PlayerCommandQueue.List)
        {
            commands.Add(command);
        }

        _player.ExecuteCommands(commands);
    }

    public void Enable(bool instant = false)
    {
        Toggle(true, instant);
    }

    public void Disable(bool instant = false)
    {
        Toggle(false, instant);
    }

    private void Toggle(bool enable, bool instant = false)
    {
        for (int i = 0; i < _graphicsLength; i++)
        {
            Graphic graphic = graphics[i];

            graphic.DOKill();

            Color startColor = _startColors[i];
            Color targetColor = enable ? startColor : disableColor;
            targetColor.a = startColor.a;

            if (instant)
            {
                graphic.color = targetColor;
            }
            else
            {
                graphic.DOColor(targetColor, 0.25f);
            }
        }
    }

    public string GetTooltipTitle()
    {
        return "Execute Button";
    }

    public string GetTooltipDescription()
    {
        return $"Ends your turn and executes all of your commands. \nGain {_player.Recharge}#ENERGY# when your next turn starts.";
    }
}
