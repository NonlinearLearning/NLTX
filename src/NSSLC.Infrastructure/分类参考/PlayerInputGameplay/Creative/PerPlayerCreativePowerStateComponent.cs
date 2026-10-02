using System.Collections.ObjectModel;

namespace NLTX.PlayerInputGameplay.Creative;

public sealed class PerPlayerCreativePowerStateComponent
{
  private readonly bool[] _enabledByPlayer;
  private readonly float[] _sliderByPlayer;
  private readonly ReadOnlyCollection<bool> _enabledByPlayerView;
  private readonly ReadOnlyCollection<float> _sliderByPlayerView;

  public PerPlayerCreativePowerStateComponent(int playerCapacity = 256)
  {
    if (playerCapacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(playerCapacity));
    }

    _enabledByPlayer = new bool[playerCapacity];
    _sliderByPlayer = new float[playerCapacity];
    _enabledByPlayerView = Array.AsReadOnly(_enabledByPlayer);
    _sliderByPlayerView = Array.AsReadOnly(_sliderByPlayer);
  }

  public int PlayerCapacity => _enabledByPlayer.Length;

  public float SliderCurrentValueCache { get; private set; }

  public float SliderDefaultValue { get; private set; }

  public float CurrentTargetValue { get; private set; }

  public long NextTimeWeCanPushTick { get; private set; }

  public IReadOnlyList<bool> EnabledByPlayer => _enabledByPlayerView;

  public IReadOnlyList<float> SliderByPlayer => _sliderByPlayerView;

  public void ConfigureSlider(float defaultValue)
  {
    ValidateFinite(defaultValue, nameof(defaultValue));
    SliderDefaultValue = defaultValue;
    SliderCurrentValueCache = defaultValue;
    CurrentTargetValue = defaultValue;
    Array.Fill(_sliderByPlayer, defaultValue);
  }

  public void ResetPlayer(int playerSlot, bool defaultToggleState, float defaultSliderValue)
  {
    ValidateSlot(playerSlot);
    ValidateFinite(defaultSliderValue, nameof(defaultSliderValue));
    _enabledByPlayer[playerSlot] = defaultToggleState;
    _sliderByPlayer[playerSlot] = defaultSliderValue;
  }

  public bool IsEnabled(int playerSlot)
  {
    ValidateSlot(playerSlot);
    return _enabledByPlayer[playerSlot];
  }

  public void SetEnabled(int playerSlot, bool enabled)
  {
    ValidateSlot(playerSlot);
    _enabledByPlayer[playerSlot] = enabled;
  }

  public float GetSlider(int playerSlot)
  {
    ValidateSlot(playerSlot);
    return _sliderByPlayer[playerSlot];
  }

  public void SetSlider(int playerSlot, float value)
  {
    ValidateSlot(playerSlot);
    ValidateFinite(value, nameof(value));
    _sliderByPlayer[playerSlot] = value;
    SliderCurrentValueCache = value;
    CurrentTargetValue = value;
  }

  public void SetTarget(float value, long nextPushTick)
  {
    ValidateFinite(value, nameof(value));
    CurrentTargetValue = value;
    NextTimeWeCanPushTick = nextPushTick;
  }

  public float StrengthMultiplierToGiveNpcs(float value)
  {
    ValidateFinite(value, nameof(value));
    return value;
  }

  private void ValidateSlot(int playerSlot)
  {
    if (playerSlot < 0 || playerSlot >= _enabledByPlayer.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(playerSlot));
    }
  }

  private static void ValidateFinite(float value, string parameterName)
  {
    if (float.IsNaN(value) || float.IsInfinity(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
