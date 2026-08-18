using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Wiring.Components;

public sealed class WireNetworkComponent
{
  private readonly Dictionary<(int X, int Y), byte> _masks = new();

  public int Count => _masks.Count;

  public byte GetMask(int x, int y)
  {
    return _masks.TryGetValue((x, y), out byte mask) ? mask : (byte)0;
  }

  public bool HasWire(int x, int y, WireColor color)
  {
    if (!Enum.IsDefined(color))
    {
      return false;
    }

    return (GetMask(x, y) & (1 << (int)color)) != 0;
  }

  public void SetMask(int x, int y, byte mask)
  {
    if (mask == 0)
    {
      _masks.Remove((x, y));
      return;
    }

    _masks[(x, y)] = mask;
  }
}
