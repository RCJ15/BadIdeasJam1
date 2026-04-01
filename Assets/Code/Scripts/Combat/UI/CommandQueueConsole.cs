using DG.Tweening;
using System;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class CommandQueueConsole : Singleton<CommandQueueConsole>
{
    private static readonly char DOT = '.';

    private PlayerCombat _player;

    [CacheComponent]
    [SerializeField] private TMP_Text text;

    [Space]
    [SerializeField] private float timeBtwDots;
    [SerializeField] private int dotsAmount;
    private float _dotsTimer;
    private int _dotsIndex;
    private StringBuilder _dotsStringBuilder = new();

    [Space]
    [SerializeField] private string normal;
    [SerializeField] private string execute;
    [SerializeField] private string maxCommands;
    [SerializeField] private string recharging;
    [SerializeField] private string enemyTurn;
    [SerializeField] private string playerDead;
    [SerializeField] private Color disabledColor = Color.gray;

    [Header("Reaction Text")]
    [SerializeField] private float reactionTextFontSize;
    [SerializeField] private float reactionTextDuration;
    private float _reactionTextTimer;
    [SerializeField] private Color reactionTextColor;
    [SerializeField] private string nothingToExecute;
    [SerializeField] private string tryAddCommandAtLimit;
    [SerializeField] private string currentlyExecutingCommands;
    [SerializeField] private string noEnergy;
    [SerializeField] private string notPlayersTurn;

    private float _normalFontSize;
    private Color _normalColor;

    private string _reactionText;
    private bool _textDirty;

    private bool _wasAtLimit;

    private void Start()
    {
        _player = PlayerCombat.Instance;
        _player.OnBeginExecute += SetTextDirty;
        _player.OnEndExecute += SetTextDirty;
        PlayerCombat.OnBeginPlayersTurn += SetTextDirty;
        PlayerCombat.OnEndPlayersTurn += SetTextDirty;
        PlayerCombat.OnDie += SetTextDirty;
        PlayerCommandQueue.OnAdd += OnAddCommand;
        PlayerCommandQueue.OnInsert += OnInsertCommand;
        PlayerCommandQueue.OnRemove += OnRemoveCommand;
        PlayerCommandQueue.OnTryAddCommandAtLimit += OnTryAddCommandAtLimit;

        _normalFontSize = text.fontSize;

        _normalColor = text.color;
        reactionTextColor.a = _normalColor.a;
        disabledColor.a = _normalColor.a;

        _textDirty = true;
    }

    private void OnDestroy()
    {
        _player.OnBeginExecute -= SetTextDirty;
        _player.OnEndExecute -= SetTextDirty;
        PlayerCombat.OnBeginPlayersTurn -= SetTextDirty;
        PlayerCombat.OnEndPlayersTurn -= SetTextDirty;
        PlayerCombat.OnDie -= SetTextDirty;
        PlayerCommandQueue.OnAdd -= OnAddCommand;
        PlayerCommandQueue.OnInsert -= OnInsertCommand;
        PlayerCommandQueue.OnRemove -= OnRemoveCommand;
        PlayerCommandQueue.OnTryAddCommandAtLimit -= OnTryAddCommandAtLimit;
    }

    private void SetTextDirty()
    {
        _textDirty = true;
    }

    private void OnAddCommand()
    {
        OnUpdateCommands();
    }

    private void OnInsertCommand(int index)
    {
        OnUpdateCommands();
    }

    private void OnRemoveCommand(int index)
    {
        OnUpdateCommands();
    }

    private void OnUpdateCommands()
    {
        if (PlayerCommandQueue.AtLimit)
        {
            _textDirty = true;
            _wasAtLimit = true;
        }
        else
        {
            if (_wasAtLimit)
            {
                _textDirty = true;
            }

            _wasAtLimit = false;
        }
    }

    private void OnTryAddCommandAtLimit()
    {
        SetReactionText(tryAddCommandAtLimit);
    }

    private void Update()
    {
        if (_reactionTextTimer > 0)
        {
            _dotsTimer = timeBtwDots;
            _dotsIndex = 0;

            _reactionTextTimer -= Time.deltaTime;
            return;
        }

        if (!string.IsNullOrEmpty(_reactionText))
        {
            _reactionText = null;
            _textDirty = true;
        }

        if (_dotsTimer >= timeBtwDots)
        {
            _dotsTimer = 0;

            _dotsIndex++;

            if (_dotsIndex > dotsAmount)
            {
                _dotsIndex = 0;
            }

            _dotsStringBuilder.Clear();
            for (int i = 0; i < _dotsIndex; i++)
            {
                _dotsStringBuilder.Append(DOT);
            }
            _textDirty = true;
        }
        else
        {
            _dotsTimer += Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if (!_textDirty)
        {
            return;
        }

        _textDirty = false;

        if (_reactionTextTimer > 0)
        {
            text.text = _reactionText;
            text.color = reactionTextColor;
        }
        else
        {
            string text = "";
            Color color = _normalColor;

            if (PlayerCombat.Dead)
            {
                text = enemyTurn;
                color = disabledColor;
            }
            else if (!PlayerCombat.IsPlayersTurn)
            {
                text = enemyTurn;
                color = disabledColor;
            }
            else if (PlayerCombat.ExecutingCommands)
            {
                text = execute;
            }
            else if (PlayerCommandQueue.AtLimit)
            {
                text = maxCommands;
            }
            else
            {
                text = normal;
            }

            // so much text stuff
            this.text.color = color;
            this.text.text = text + _dotsStringBuilder.ToString();
        }
    }

    public void TriggerCurrentlyExecutingCommands() => SetReactionText(currentlyExecutingCommands);
    public void TriggerNotPlayersTurn() => SetReactionText(notPlayersTurn);
    public void TriggerNothingToExecute() => SetReactionText(nothingToExecute);
    public void TriggerNoEnergy() => SetReactionText(noEnergy);

    public void SetReactionText(string reactionText)
    {
        SoundManager.PlaySound("error");

        _reactionText = reactionText;
        _reactionTextTimer = reactionTextDuration;

        _textDirty = true;

        text.DOKill();
        text.fontSize = reactionTextFontSize;
        DOTween.To(() => text.fontSize, (v) => text.fontSize = v, _normalFontSize, 0.5f).SetEase(Ease.OutBack).SetTarget(text);
    }
}
