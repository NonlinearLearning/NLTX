namespace Terraria.Content;

public sealed record ProjectileDefinition(
  ProjectileIdentityDefinition Identity,
  ProjectileGeometryDefinition Geometry,
  ProjectileBehaviorDefinition Behavior,
  ProjectileCombatDefinition Combat,
  ProjectilePenetrationDefinition Penetration,
  ProjectileCapabilitiesDefinition Capabilities,
  ProjectilePresentationDefinition Presentation)
{
  public ProjectileNetworkDefinition Network { get; init; } = new();

  public ProjectileDefinition(
    int typeId,
    string? persistentId,
    int width,
    int height,
    int aiStyle,
    int defaultTimeLeft,
    int penetrate,
    bool friendly,
    bool hostile)
    : this(
      new ProjectileIdentityDefinition(typeId, persistentId),
      new ProjectileGeometryDefinition(width, height, 1f, true, false),
      new ProjectileBehaviorDefinition(aiStyle, 0, defaultTimeLeft),
      new ProjectileCombatDefinition(0, 0f, friendly, hostile),
      new ProjectilePenetrationDefinition(penetrate, penetrate, false),
      new ProjectileCapabilitiesDefinition(false, false, false, false),
      new ProjectilePresentationDefinition(1, 0f, false))
  {
  }

  public int TypeId => Identity.TypeId;

  public string? PersistentId => Identity.PersistentId;

  public int Width => Geometry.Width;

  public int Height => Geometry.Height;
}
