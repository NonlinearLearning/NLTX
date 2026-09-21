namespace Terraria.WorldInteraction.Components;

public sealed class ManEaterProtectionIndexComponent
{
  private readonly HashSet<int> _protectedSpots = new();

  internal void Clear()
  {
    _protectedSpots.Clear();
  }

  internal void Protect(int x, int y)
  {
    _protectedSpots.Add(Pack(x, y));
  }

  internal bool Contains(int x, int y)
  {
    return _protectedSpots.Contains(Pack(x, y));
  }

  private static int Pack(int x, int y)
  {
    return ((x & 0xFFFF) << 16) | (y & 0xFFFF);
  }
}
