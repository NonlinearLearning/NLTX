namespace Terraria.Share.Player;

/// <summary>Why a use request was or was not applied.</summary>
public enum PlayerItemUseOutcomeKind : byte
{
  Applied = 0,
  NotReady = 1,
  UnknownItem = 2,
  MissingAmmo = 3,
  NoInventory = 4,
  EmptySlot = 5,
  NotChanneling = 6,

  /// <summary>The request named a slot the inventory does not currently hold.</summary>
  NotHeld = 7
}
