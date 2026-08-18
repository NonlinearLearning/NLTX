using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.Dome.Simulation.Players;

public sealed class PlayerPersistentState
{
  public const int ItemSlotCount = 990;
  private const int MaximumBuffCount = 44;

  public PlayerPersistentState(
    string uuid,
    PlayerPersistentProfile profile,
    int life,
    int maximumLife,
    int mana,
    int maximumMana,
    IReadOnlyList<PlayerPersistentBuff> buffs,
    byte selectedLoadout,
    ushort accessoryVisibility,
    IReadOnlyList<PlayerPersistentItem> items)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(uuid);
    ArgumentNullException.ThrowIfNull(profile.Name);
    ArgumentNullException.ThrowIfNull(buffs);
    ArgumentNullException.ThrowIfNull(items);
    if (!Guid.TryParseExact(uuid, "D", out _) || life < 0 || maximumLife < 1 || mana < 0 ||
        maximumMana < 0 || life > maximumLife || selectedLoadout > 2 ||
        buffs.Count > MaximumBuffCount || items.Count != ItemSlotCount)
    {
      throw new ArgumentOutOfRangeException(nameof(uuid));
    }

    for (int slotId = 0; slotId < items.Count; slotId++)
    {
      PlayerPersistentItem item = items[slotId];
      if (item.SlotId != slotId || item.Stack < 0 || item.ItemType < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(items));
      }
    }

    Uuid = uuid;
    Profile = profile;
    Life = life;
    MaximumLife = maximumLife;
    Mana = mana;
    MaximumMana = maximumMana;
    Buffs = Array.AsReadOnly(buffs.ToArray());
    SelectedLoadout = selectedLoadout;
    AccessoryVisibility = accessoryVisibility;
    Items = Array.AsReadOnly(items.ToArray());
  }

  public ushort AccessoryVisibility { get; }

  public IReadOnlyList<PlayerPersistentBuff> Buffs { get; }

  public IReadOnlyList<PlayerPersistentItem> Items { get; }

  public int Life { get; }

  public int Mana { get; }

  public int MaximumLife { get; }

  public int MaximumMana { get; }

  public PlayerPersistentProfile Profile { get; }

  public byte SelectedLoadout { get; }

  public string Uuid { get; }
}
