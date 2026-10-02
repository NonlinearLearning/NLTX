namespace Terraria.Content;

public sealed record NpcDefinition(
  NpcIdentityDefinition Identity,
  NpcCoreDefinition Core,
  NpcMovementDefinition Movement,
  NpcSpawnDefinition Spawn,
  NpcTownDefinition Town,
  NpcCapabilitiesDefinition Capabilities)
{
  public NpcPresentationDefinition Presentation { get; init; } = new();

  public NpcDefinition(
    int typeId,
    int netId,
    string persistentId,
    int defaultLifeMax,
    int defaultDamage,
    int defaultDefense,
    int width,
    int height,
    int aiStyle,
    bool isTownNpc,
    bool friendly,
    bool hostile)
    : this(
      new NpcIdentityDefinition(typeId, netId, persistentId),
      new NpcCoreDefinition(defaultLifeMax, defaultDamage, defaultDefense),
      new NpcMovementDefinition(width, height),
      new NpcSpawnDefinition(aiStyle, false),
      new NpcTownDefinition(isTownNpc),
      new NpcCapabilitiesDefinition(friendly, hostile))
  {
  }

  public int TypeId => Identity.TypeId;

  public int NetId => Identity.NetId;

  public string PersistentId => Identity.PersistentId;
}
