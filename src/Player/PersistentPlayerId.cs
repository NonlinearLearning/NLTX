namespace Terraria.Player;

public readonly record struct PersistentPlayerId(Guid Value)
{
  public static PersistentPlayerId None => new(Guid.Empty);

  public bool IsEmpty => Value == Guid.Empty;
}
