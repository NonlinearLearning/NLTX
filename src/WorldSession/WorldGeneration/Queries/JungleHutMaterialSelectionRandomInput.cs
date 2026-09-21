using System;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Explicit random input for the Version4 jungle-hut material selection.
/// </summary>
public readonly record struct JungleHutMaterialSelectionRandomInput(
  int HutSelectionRoll)
{
  public void Validate()
  {
    if (HutSelectionRoll < 0 || HutSelectionRoll >= 5)
    {
      throw new ArgumentOutOfRangeException(nameof(HutSelectionRoll));
    }
  }
}
