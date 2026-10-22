namespace MinimalImplementation.Server.Inventory.State;

/// <summary>One pickup effect, independent of the world-item stack count.</summary>
public readonly record struct WorldItemPickupEffect(int Heal, int Mana, ushort NebulaBuff)
{
  public static WorldItemPickupEffect ForItem(int type)
  {
    return type switch {
      58 or 1734 or 1867 => new(20, 0, 0),
      184 or 1735 or 1868 => new(0, 100, 0),
      4143 => new(0, 50, 0),
      3453 => new(0, 0, 179),
      3454 => new(0, 0, 173),
      3455 => new(0, 0, 176),
      _ => default
    };
  }
}
