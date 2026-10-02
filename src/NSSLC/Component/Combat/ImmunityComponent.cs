using System;
using System.Collections.Generic;

namespace Terraria.Combat;

public struct ImmunityComponent
{
  public ImmunityComponent(int remainingTicks)
  {
    _windows = remainingTicks > 0
      ? new[]
      {
        new HitImmunityWindow(
          new HitImmunityKey(HitImmunityScope.General, null, null),
          remainingTicks)
      }
      : null;
    Revision = 0;
  }

  public ImmunityComponent(
    IReadOnlyList<HitImmunityWindow>? windows = null,
    int revision = 0)
  {
    _windows = windows is null
      ? null
      : new List<HitImmunityWindow>(windows).ToArray();
    Revision = revision;
  }

  private HitImmunityWindow[]? _windows;

  public int Revision;

  public ReadOnlyMemory<HitImmunityWindow> Windows =>
    new(_windows ?? Array.Empty<HitImmunityWindow>());

  public bool HasActiveWindow => _windows is { Length: > 0 };

  // Compatibility projection for the former single general cooldown field.
  // The general window remains stored in _windows; this property is not a second state source.
  public int RemainingTicks
  {
    get
    {
      if (_windows is null)
      {
        return 0;
      }

      foreach (HitImmunityWindow window in _windows)
      {
        if (window.Key.Scope == HitImmunityScope.General)
        {
          return window.RemainingTicks;
        }
      }

      return 0;
    }
    set
    {
      List<HitImmunityWindow> windows = _windows is null
        ? []
        : new List<HitImmunityWindow>(_windows);

      for (int index = windows.Count - 1; index >= 0; index--)
      {
        if (windows[index].Key.Scope == HitImmunityScope.General)
        {
          windows.RemoveAt(index);
        }
      }

      if (value > 0)
      {
        windows.Add(new HitImmunityWindow(
          new HitImmunityKey(HitImmunityScope.General, null, null),
          value));
      }

      _windows = windows.Count == 0 ? null : windows.ToArray();
      Revision++;
    }
  }
}
