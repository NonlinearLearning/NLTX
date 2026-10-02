namespace Terraria.EntityLifecycleAttribution;

public sealed class WorldSignStore
{
  private readonly Dictionary<TileCoordinate, WorldSignState> _signs = new();

  public WorldSignState GetOrCreate(TileCoordinate coordinate)
  {
    if (!coordinate.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(coordinate));
    }

    if (!_signs.TryGetValue(coordinate, out WorldSignState? sign))
    {
      sign = new WorldSignState(coordinate);
      _signs.Add(coordinate, sign);
    }
    else
    {
      sign.IsActive = true;
    }

    return sign;
  }

  public bool TrySetText(TileCoordinate coordinate, string text)
  {
    ArgumentNullException.ThrowIfNull(text);
    return _signs.TryGetValue(coordinate, out WorldSignState? sign) &&
           sign.IsActive &&
           SetText(sign, text);
  }

  public bool Invalidate(TileCoordinate coordinate)
  {
    if (!_signs.TryGetValue(coordinate, out WorldSignState? sign) || !sign.IsActive)
    {
      return false;
    }

    sign.IsActive = false;
    return true;
  }

  public bool TryGetActive(TileCoordinate coordinate, out WorldSignState? sign)
  {
    if (_signs.TryGetValue(coordinate, out sign) && sign.IsActive)
    {
      return true;
    }

    sign = null;
    return false;
  }

  private static bool SetText(WorldSignState sign, string text)
  {
    sign.Text = text;
    return true;
  }
}
