using System;

namespace Terraria.Npc;

// status: proposed
public sealed class NpcDefinitionReferenceState
{
  public NpcDefinitionReferenceState(
    NpcTypeId typeId = default,
    NpcNetId netId = default,
    int catalogRevision = 0,
    NpcTypeId? initialTypeId = null)
  {
    if (catalogRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(catalogRevision));
    }

    TypeId = typeId;
    NetId = netId;
    InitialTypeId = initialTypeId ?? typeId;
    CatalogRevision = catalogRevision;
    Validate();
  }

  public NpcTypeId TypeId { get; }

  public NpcNetId NetId { get; }

  public NpcTypeId InitialTypeId { get; }

  public int CatalogRevision { get; }

  public bool UsesNetIdVariant => NetId.IsVariant;

  public bool IsInitialized =>
    TypeId.IsValid && InitialTypeId.IsValid;

  public void Validate()
  {
    if (TypeId.IsValid && !InitialTypeId.IsValid)
    {
      throw new InvalidOperationException(
        "An assigned type requires an assigned initial type.");
    }
  }
}
