using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommandButton : MonoBehaviour, ITooltip
{
    public CommandReference Command => command;

    [CacheComponent]
    [SerializeField] private CustomButton button;
    [CacheComponent]
    [SerializeField] private CommandVisuals visuals;
    [CacheComponent]
    [SerializeField] private FitToText fitToText;
    [CacheComponent]
    [SerializeField] private DraggableCommand draggableCommand;

    [Space]
    [SerializeField] private Image border;
    [SerializeField] private Graphic energyIcon;
    private Color _startEnergyIconColor;
    [SerializeField] private TMP_Text energyText;
    private Color _startEnergyTextColor;
    [SerializeField] private CommandReference command;

    [Space]
    [SerializeField] private float extraWidth;
    [SerializeField] private float minWidth;

    [Header("Off")]
    [SerializeField] private Color offColor = Color.gray;
    [SerializeField] private float tweenDuration = 0.5f;

    private PlayerCombat _player;
    private CommandQueueUI _commandQueue;
    private CommandQueueConsole _console;

    private bool _enoughEnergy => _commandQueue.Energy.HasValue ? (command.Energy <= _commandQueue.Energy.Value) : (command.Energy <= _player.Energy);
    private bool _canBeUsed => PlayerCombat.IsPlayersTurn && !PlayerCombat.ExecutingCommands && !PlayerCombat.Dead;

    private IEnumerator Start()
    {
        _player = PlayerCombat.Instance;
        _commandQueue = CommandQueueUI.Instance;
        _console = CommandQueueConsole.Instance;

        _startEnergyIconColor = energyIcon.color;
        _startEnergyTextColor = energyText.color;

        CommandQueueUI.OnUpdateEnergy += OnUpdateEnergy;
        PlayerCombat.OnBeginPlayersTurn += UpdateColor;
        PlayerCombat.OnEndPlayersTurn += UpdateColor;
        PlayerCombat.OnDie += UpdateColor;
        _player.OnBeginExecute += UpdateColor;
        _player.OnEndExecute += UpdateColor;

        border.color = command.Command.Color;

        energyText.text = command.Energy.ToString();
        visuals.Command = command;

        button.OnClick.AddListener(OnClick);
        draggableCommand.Command = command;

        RectTransform rect = transform as RectTransform;
        rect.sizeDelta = new(Mathf.Max(minWidth, visuals.Text.textBounds.size.x + extraWidth), rect.sizeDelta.y);

        UpdateColor(true);

        // I don't know
        for (int i = 0; i < 10; i++)
        {
            fitToText.Fit();
            yield return null;
        }
    }

    private void OnDestroy()
    {
        CommandQueueUI.OnUpdateEnergy -= OnUpdateEnergy;
        PlayerCombat.OnBeginPlayersTurn -= UpdateColor;
        PlayerCombat.OnEndPlayersTurn -= UpdateColor;
        PlayerCombat.OnDie -= UpdateColor;

        if (_player != null)
        {
            _player.OnBeginExecute -= UpdateColor;
            _player.OnEndExecute -= UpdateColor;
        }
    }

    private void OnUpdateEnergy(int energy)
    {
        UpdateEnergy(false);
    }

    private void UpdateColor() => UpdateColor(false);
    private void UpdateColor(bool instant)
    {
        draggableCommand.enabled = _canBeUsed && _enoughEnergy;

        border.DOKill();
        visuals.Text.DOKill();
        visuals.Icon.DOKill();

        UpdateEnergy(instant);

        if (!_canBeUsed)
        {
            if (instant)
            {
                border.color = offColor;
                visuals.Text.color = offColor;
                visuals.Icon.color = offColor;
                return;
            }

            border.DOColor(offColor, tweenDuration);
            visuals.Text.DOColor(offColor, tweenDuration);
            visuals.Icon.DOColor(offColor, tweenDuration);
        }
        else
        {
            Color commandColor = command.Command.Color;

            if (instant)
            {
                border.color = commandColor;
                visuals.Text.color = commandColor;
                visuals.Icon.color = commandColor;
                return;
            }

            border.DOColor(commandColor, tweenDuration);
            visuals.Text.DOColor(commandColor, tweenDuration);
            visuals.Icon.DOColor(commandColor, tweenDuration);
        }
    }

    private void UpdateEnergy(bool instant)
    {
        energyText.DOKill();
        energyIcon.DOKill();

        if (!_canBeUsed)
        {
            if (instant)
            {
                energyText.color = offColor;
                energyIcon.color = offColor;
                return;
            }

            energyText.DOColor(offColor, tweenDuration);
            energyIcon.DOColor(offColor, tweenDuration);
        }
        else if (!_enoughEnergy)
        {
            if (instant)
            {
                energyText.color = Color.red;
                energyIcon.color = Color.red;
                return;
            }

            energyText.DOColor(Color.red, tweenDuration);
            energyIcon.DOColor(Color.red, tweenDuration);
        }
        else
        {
            if (instant)
            {
                energyText.color = _startEnergyTextColor;
                energyIcon.color = _startEnergyIconColor;
                return;
            }

            energyText.DOColor(_startEnergyTextColor, tweenDuration);
            energyIcon.DOColor(_startEnergyIconColor, tweenDuration);
        }
    }

    public void OnClick()
    {
        if (!PlayerCombat.IsPlayersTurn)
        {
            _console.TriggerNotPlayersTurn();
            return;
        }

        if (PlayerCombat.ExecutingCommands)
        {
            _console.TriggerCurrentlyExecutingCommands();
            return;
        }

        if (!_enoughEnergy)
        {
            _console.TriggerNoEnergy();
            return;
        }

        PlayerCommandQueue.Add(command);
    }

    public string GetTooltipTitle()
    {
        return null;
    }

    public string GetTooltipDescription()
    {
        string description = command.Description;
        description = description.Replace("#DAMAGE#", _player.Damage.ToString());
        description = description.Replace("#KNOCKBACK#", _player.Knockback.ToString());
        return description;
    }
}
