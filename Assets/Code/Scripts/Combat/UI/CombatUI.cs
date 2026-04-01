using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text _hpText;
    private string _hpTextFormat;
    [Space]
    [SerializeField] private Slider energySlider;
    [SerializeField] private Slider energySliderActual;
    [SerializeField] private Image energySliderFlashing;
    [SerializeField] private TMP_Text _energyText;
    private string _energyTextFormat;

    private PlayerCombat _player;
    private CommandQueueUI _commandQueue;

    private void Start()
    {
        Color color = Color.Lerp(Color.white, energySliderFlashing.color, 0.5f);
        color.a = energySliderFlashing.color.a;
        energySliderFlashing.DOColor(color, 2).SetLoops(-1, LoopType.Yoyo);

        _player = PlayerCombat.Instance;
        _commandQueue = CommandQueueUI.Instance;

        _hpTextFormat = _hpText.text;
        _energyTextFormat = _energyText.text;

        UpdateHP(true);
        UpdateEnergy(true);

        _player.OnChangeHP += OnChangeHP;
        _player.OnEnergyChanged += OnChangeEnergy;
        _player.OnBeginExecute += UpdateEnergy;
        _player.OnEndExecute += UpdateEnergy;
        PlayerCombat.OnExecuteCommand += OnPlayerExecuteCommand;
        CommandQueueUI.OnUpdateEnergy += OnCommandQueueUpdateEnergy;
    }

    private void OnDestroy()
    {
        _player.OnChangeHP -= OnChangeHP;
        _player.OnEnergyChanged -= OnChangeEnergy;
        _player.OnBeginExecute -= UpdateEnergy;
        _player.OnEndExecute -= UpdateEnergy;
        PlayerCombat.OnExecuteCommand -= OnPlayerExecuteCommand;
        CommandQueueUI.OnUpdateEnergy -= OnCommandQueueUpdateEnergy;
    }

    private void OnChangeHP(int oldHp, int newHp)
    {
        UpdateHP();
    }

    private void OnChangeEnergy(int oldEnergy, int newEnergy)
    {
        UpdateEnergy();
    }

    private void OnCommandQueueUpdateEnergy(int energy)
    {
        UpdateEnergy();
    }

    private void OnPlayerExecuteCommand(int index, Command command)
    {
        UpdateEnergy();
    }

    private void UpdateHP(bool instant = false)
    {
        _hpText.text = string.Format(_hpTextFormat, _player.HP, _player.MaxHP);

        hpSlider.maxValue = _player.MaxHP;

        hpSlider.DOKill();

        if (instant)
        {
            hpSlider.value = _player.HP;
        }
        else
        {
            hpSlider.DOValue(_player.HP, 0.25f).SetEase(Ease.OutExpo);
        }
    }

    private void UpdateEnergy() => UpdateEnergy(false);
    private void UpdateEnergy(bool instant)
    {
        int? commandQueueEnergy = _commandQueue.Energy;
        int energy = commandQueueEnergy.HasValue ? commandQueueEnergy.Value : _player.Energy;

        _energyText.text = string.Format(_energyTextFormat, energy, _player.MaxEnergy);

        energySliderActual.maxValue = _player.MaxEnergy;
        energySlider.maxValue = _player.MaxEnergy;

        energySlider.DOKill();
        energySliderActual.DOKill();

        if (instant)
        {
            energySlider.value = energy;
            energySliderActual.value = _player.Energy;
        }
        else
        {
            energySlider.DOValue(energy, 0.25f).SetEase(Ease.OutExpo);
            energySliderActual.DOValue(_player.Energy, 0.25f).SetEase(Ease.OutExpo);
        }
    }
}
