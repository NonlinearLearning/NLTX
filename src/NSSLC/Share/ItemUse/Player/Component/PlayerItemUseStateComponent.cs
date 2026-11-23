using Terraria.Item;

namespace Terraria.Player;

/// <summary>Use gate and channel state of one player, written only by the item use system.</summary>
/// <remarks>
/// Version4 spreads this across <c>Player.itemAnimation</c>, <c>Player.reuseDelay</c>,
/// <c>Player.channel</c> and <c>Player.selectedItem</c>. The server keeps the absolute ready tick
/// and the last used stack instead, so a replayed tick stream produces the same gate.
/// </remarks>
public readonly record struct PlayerItemUseStateComponent(
    short Slot,
    long ReadyTick,
    long UseStartTick,
    bool Channeling,
    ItemId ActiveItem,
    ItemId ActiveAmmo)
{
  public static PlayerItemUseStateComponent Idle => new(-1, 0, -1, false, ItemId.None, ItemId.None);

  public bool IsChanneling => Channeling;
}
