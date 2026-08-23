using System;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects;

public sealed class ChestComponent
{
  public const int SlotCount = ChestInventoryComponent.SlotCount;
  public const int MaximumNameLength = ChestDefinitionComponent.DefaultMaximumNameLength;

  private string _name;

  public ChestComponent(
    int chestId,
    int tileX,
    int tileY,
    long revision = 1,
    string name = "")
  {
    if (chestId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(chestId));
    }

    Identity = new ChestIdentityComponent(
      chestId,
      tileX,
      tileY,
      new WorldSectionCoordinates(tileX / WorldGrid.SectionWidth, tileY / WorldGrid.SectionHeight));
    Inventory = new ChestInventoryComponent();
    Access = new ChestAccessComponent();
    Definition = ChestDefinitionComponent.World;
    RevisionState = new ChestRevisionComponent(revision);
    ArgumentNullException.ThrowIfNull(name);
    if (name.Length > Definition.MaximumNameLength)
    {
      throw new ArgumentOutOfRangeException(nameof(name));
    }

    _name = name;
  }

  public ChestAccessComponent Access { get; }
  public int ChestId => Identity.ChestId;
  public ChestDefinitionComponent Definition { get; }
  public ChestIdentityComponent Identity { get; }
  public ChestInventoryComponent Inventory { get; }
  public string Name => _name;
  public PlayerHandle? Opener => Access.Opener;
  public ChestRevisionComponent RevisionState { get; }
  public long Revision => RevisionState.Value;
  public WorldSectionCoordinates Section => Identity.Section;
  public int TileX => Identity.TileX;
  public int TileY => Identity.TileY;

  public ItemStack GetSlot(int slot)
  {
    return Inventory.GetSlot(slot);
  }

  public void SetSlot(int slot, ItemStack stack)
  {
    Inventory.SetSlot(slot, stack);
  }

  public bool TryOpen(PlayerHandle player)
  {
    return Access.TryOpen(player);
  }

  public bool IsLocked { get; private set; }
  public ChestLockDefinition LockDefinition { get; private set; }

  public void SetLocked(bool isLocked, ChestLockDefinition? lockDefinition = null)
  {
    if (isLocked)
    {
      ChestLockDefinition resolvedDefinition = lockDefinition ?? ChestLockDefinition.GoldKey;
      resolvedDefinition.Validate();
      LockDefinition = resolvedDefinition;
    }

    IsLocked = isLocked;
  }

  public bool TryRename(string name)
  {
    ArgumentNullException.ThrowIfNull(name);
    if (name.Length > Definition.MaximumNameLength)
    {
      return false;
    }

    if (string.Equals(_name, name, StringComparison.Ordinal))
    {
      return true;
    }

    if (!RevisionState.TryIncrement())
    {
      return false;
    }

    _name = name;
    return true;
  }

  public void Close(PlayerHandle player)
  {
    Access.Close(player);
  }

  public void IncrementRevision()
  {
    RevisionState.Increment();
  }

  public bool TryIncrementRevision()
  {
    return RevisionState.TryIncrement();
  }
}
