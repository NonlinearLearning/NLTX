using System;

namespace Terraria.WorldSession.Environment.AmbientSky;

public sealed class BackgroundChangeFlashStateComponent
{
  public const int AreaCount = 13;

  private readonly int[] _variations;
  private readonly float[] _flashPowers;

  public BackgroundChangeFlashStateComponent(
    IReadOnlyList<int>? variations = null,
    IReadOnlyList<float>? flashPowers = null)
  {
    _variations = new int[AreaCount];
    _flashPowers = new float[AreaCount];
    if (variations is not null)
    {
      if (variations.Count != AreaCount)
      {
        throw new ArgumentException(
          $"Background flash state must contain exactly {AreaCount} entries.",
          nameof(variations));
      }

      for (int index = 0; index < _variations.Length; index++)
      {
        _variations[index] = variations[index];
      }
    }

    if (flashPowers is not null)
    {
      if (flashPowers.Count != AreaCount)
      {
        throw new ArgumentException(
          $"Background flash state must contain exactly {AreaCount} entries.",
          nameof(flashPowers));
      }

      for (int index = 0; index < _flashPowers.Length; index++)
      {
        _flashPowers[index] = flashPowers[index];
      }
    }

    Validate();
  }

  public IReadOnlyList<int> Variations => Array.AsReadOnly(_variations);

  public IReadOnlyList<float> FlashPowers => Array.AsReadOnly(_flashPowers);

  public void Validate()
  {
    for (int index = 0; index < _flashPowers.Length; index++)
    {
      float flashPower = _flashPowers[index];
      if (!float.IsFinite(flashPower) || flashPower < 0.0f || flashPower > 1.0f)
      {
        throw new ArgumentOutOfRangeException(nameof(FlashPowers));
      }
    }
  }

}
