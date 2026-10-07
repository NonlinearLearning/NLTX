using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldObjectDestructionContext
{
  public bool IsActive { get; private set; }

  public bool TryEnter()
  {
    if (IsActive)
    {
      return false;
    }

    IsActive = true;
    return true;
  }

  public void Exit()
  {
    if (!IsActive)
    {
      throw new InvalidOperationException(
        "A destruction operation cannot exit without an active operation.");
    }

    IsActive = false;
  }

  public void Reset()
  {
    IsActive = false;
  }
}
