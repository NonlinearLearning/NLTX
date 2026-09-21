using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Protocol.V1456.Packets;

namespace Terraria.Dome.Protocol.V1456.Session;

public sealed class PlayerBootstrapState
{
  public PlayerBootstrapState(
    string uuid,
    PlayerProfilePacket profile,
    PlayerVitalsPacket? life,
    PlayerVitalsPacket? mana,
    IReadOnlyList<ushort> buffTypes,
    PlayerLoadoutPacket? loadout,
    IReadOnlyList<PlayerEquipmentPacket> equipment)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(uuid);
    ArgumentNullException.ThrowIfNull(buffTypes);
    ArgumentNullException.ThrowIfNull(equipment);

    Uuid = uuid;
    Profile = profile;
    Life = life;
    Mana = mana;
    BuffTypes = Array.AsReadOnly(buffTypes.ToArray());
    Loadout = loadout;
    Equipment = Array.AsReadOnly(equipment.ToArray());
  }

  public IReadOnlyList<ushort> BuffTypes { get; }

  public IReadOnlyList<PlayerEquipmentPacket> Equipment { get; }

  public PlayerVitalsPacket? Life { get; }

  public PlayerLoadoutPacket? Loadout { get; }

  public PlayerVitalsPacket? Mana { get; }

  public PlayerProfilePacket Profile { get; }

  public string Uuid { get; }
}
