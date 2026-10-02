using System;

namespace Terraria.Npc;

public sealed class NpcDefinitionReferenceComponent
{
  public NpcDefinitionReferenceComponent(NpcTypeId typeId, NpcNetId netId, int catalogRevision = 0)
  {
    if (!typeId.IsValid)
    {
      throw new ArgumentException(
        "NpcTypeId must be valid.",
        nameof(typeId));
    }

    TypeId = typeId;
    NetId = netId;
    InitialTypeId = typeId;
    CatalogRevision = catalogRevision;
  }

  public NpcTypeId TypeId { get; }

  public NpcNetId NetId { get; }

  public NpcTypeId InitialTypeId { get; }

  public int CatalogRevision { get; }

  public bool UsesNetIdVariant => NetId.IsVariant;

  public bool IsInitialized => TypeId.IsValid && InitialTypeId.IsValid;
}
