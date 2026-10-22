using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Player;

/// <summary>Read-only Version4 item metadata source; the inventory owner remains authoritative.</summary>
public interface IPlayerEquipmentMetadataSource
{
  CommitVersion Revision { get; }

  bool TryGet(int itemType, out PlayerEquipmentItemFacts? facts);
}
