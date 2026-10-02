namespace Terraria.Player;

public readonly record struct PlayerContainerRef(Guid EntityId)
{
  public static PlayerContainerRef None => new(Guid.Empty);

  public bool IsEmpty => EntityId == Guid.Empty;
}
