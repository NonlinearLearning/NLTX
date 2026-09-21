using System;

namespace Terraria.WorldSession.Environment.AmbientSky;

public sealed class TreeTopVariationStateComponent
{
  public const int AreaCount = 13;

  private readonly int[] _variations;

  public TreeTopVariationStateComponent(IReadOnlyList<int>? variations = null)
  {
    _variations = new int[AreaCount];
    if (variations is not null)
    {
      if (variations.Count != AreaCount)
      {
        throw new ArgumentException(
          $"Tree-top variations must contain exactly {AreaCount} entries.",
          nameof(variations));
      }

      for (int index = 0; index < _variations.Length; index++)
      {
        _variations[index] = variations[index];
      }
    }

    Validate();
  }

  public IReadOnlyList<int> Variations => Array.AsReadOnly(_variations);

  public void Validate()
  {
    for (int index = 0; index < _variations.Length; index++)
    {
      ArgumentOutOfRangeException.ThrowIfNegative(_variations[index]);
    }
  }
}
