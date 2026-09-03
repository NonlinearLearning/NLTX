using System;

namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct SignRestorePolicy(
  int MaximumTextLength = SignComponent.MaximumTextLength)
{
  public bool TryValidate(string text, out string? error)
  {
    ArgumentNullException.ThrowIfNull(text);
    if (text.Length > MaximumTextLength)
    {
      error = "The sign text exceeds the configured length.";
      return false;
    }

    error = null;
    return true;
  }
}
