namespace Terraria.Content;

public sealed record TileDefinition(
  int TypeId,
  string? PersistentId,
  bool Solid,
  bool SolidTop,
  bool Lighted,
  bool FrameImportant,
  bool Container,
  bool Sign)
{
  public TileIdentityDefinition Identity => new(TypeId, PersistentId);

  public TileCollisionDefinition Collision => new(Solid, SolidTop, Platform: SolidTop);

  public TileLightingDefinition Lighting => new(Lighted);

  public TileFramingDefinition Framing => new(FrameImportant);

  public TileInteractionDefinition Interaction => new(Container, Sign);

  public TileEnvironmentDefinition Environment { get; init; } = new();
}
