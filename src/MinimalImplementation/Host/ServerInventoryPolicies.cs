using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Modules.Creative;

namespace MinimalImplementation.Server.Host;

/// <summary>Applies the current server policy that loaded-world chests are accessible.</summary>
public sealed class ServerChestAccessPolicy : IChestAccessPolicy
{
  public bool CanOpen(EntityUuid playerId, WorldScopeId worldScope, ChestSnapshot chest)
  {
    ArgumentNullException.ThrowIfNull(chest);
    return playerId.IsValid && worldScope.IsValid && chest.WorldScope == worldScope;
  }
}

/// <summary>Validates creative sacrifice reports against the reviewed item range.</summary>
public sealed class ServerCreativeSacrificeFacts : ICreativeSacrificeFacts
{
  public bool IsValidSacrificeReport(ushort itemId, ushort amount)
  {
    return itemId > 0
      && ItemContentFacts.IsKnownItemType(itemId)
      && amount > 0
      && amount <= ItemContentFacts.CommonMaxStack;
  }
}
