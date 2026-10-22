using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Produces the NPC-side packet-65 teleport projection.</summary>
public static class NpcTeleportPacketProducer
{
  public static TeleportEntityPacket Create(
      NpcView npc,
      Vector2 position,
      byte style,
      bool useCurrentEntityPosition,
      int? teleportValue)
  {
    NpcNetworkPacketProducerGuards.RequireCommitted(npc, 65);
    NpcNetworkPacketProducerGuards.RequireFinite(position, nameof(position));
    if (teleportValue == 0)
    {
      throw new ArgumentException(
          "Packet 65 emits the optional teleport value only when it is nonzero.",
          nameof(teleportValue));
    }

    return new TeleportEntityPacket
    {
      EntityKind = 1,
      EntityIndex = npc.Slot,
      Position = new PacketVector2(position.X, position.Y),
      Style = style,
      UseCurrentEntityPosition = useCurrentEntityPosition,
      TeleportValue = teleportValue
    };
  }
}

/// <summary>Produces the NPC-side packet-100 portal teleport projection.</summary>
public static class NpcPortalTeleportPacketProducer
{
  public static TeleportNPCThroughPortalPacket Create(
      NpcView npc,
      short portalColorIndex,
      Vector2 position,
      Vector2 velocity)
  {
    NpcNetworkPacketProducerGuards.RequireCommitted(npc, 100);
    NpcNetworkPacketProducerGuards.RequireFinite(position, nameof(position));
    NpcNetworkPacketProducerGuards.RequireFinite(velocity, nameof(velocity));
    return new TeleportNPCThroughPortalPacket
    {
      NpcIndex = checked((ushort)npc.Slot),
      PortalColorIndex = portalColorIndex,
      PositionX = position.X,
      PositionY = position.Y,
      VelocityX = velocity.X,
      VelocityY = velocity.Y
    };
  }
}

/// <summary>Produces packet 130 from a trusted fish-out spawn fact.</summary>
public static class NpcFishOutPacketProducer
{
  public static FishOutNPCPacket Create(int tileX, int tileY, short npcType)
  {
    if (tileX is < 0 or > ushort.MaxValue || tileY is < 0 or > ushort.MaxValue || npcType <= 0)
    {
      throw new ArgumentOutOfRangeException(
          nameof(tileX), "Fish-out coordinates and NPC type must fit packet 130.");
    }

    return new FishOutNPCPacket
    {
      X = (ushort)tileX,
      Y = (ushort)tileY,
      NpcType = npcType
    };
  }
}

/// <summary>Produces the fixed six-value packet-136 cavern monster table.</summary>
public static class NpcCavernMonsterTypePacketProducer
{
  public static SyncCavernMonsterTypePacket Create(IReadOnlyList<ushort> types)
  {
    ArgumentNullException.ThrowIfNull(types);
    if (types.Count != 6)
    {
      throw new ArgumentException(
          "Packet 136 requires exactly six cavern monster types.", nameof(types));
    }

    return new SyncCavernMonsterTypePacket
    {
      Type00 = types[0],
      Type01 = types[1],
      Type02 = types[2],
      Type10 = types[3],
      Type11 = types[4],
      Type12 = types[5]
    };
  }
}

/// <summary>Packet-153 source fact handed to P02's player damage owner.</summary>
public sealed record NpcDebuffDamageIntent(
    CombatEntityIdentity Npc,
    short NpcSlot,
    ushort TargetPlayer,
    short BuffType,
    long NpcRevision,
    CommitVersion CommitVersion)
{
  public NPCDebuffDamagePacket CreatePacket()
  {
    if (NpcSlot is < 0 or > byte.MaxValue || BuffType <= 0 ||
        TargetPlayer > byte.MaxValue)
    {
      throw new InvalidOperationException(
          "Packet 153 requires a valid debuff type and player slot.");
    }

    return new NPCDebuffDamagePacket
    {
      NpcIndex = checked((byte)NpcSlot),
      BuffType = BuffType
    };
  }
}

public static class NpcDebuffDamagePacketProducer
{
  public static NpcDebuffDamageIntent Create(
      NpcView npc,
      byte targetPlayer,
      short buffType)
  {
    NpcNetworkPacketProducerGuards.RequireCommitted(npc, 153);
    if (buffType <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(buffType));
    }

    return new NpcDebuffDamageIntent(
        npc.Entity,
        npc.Slot,
        targetPlayer,
        buffType,
        npc.Revision,
        npc.CommitVersion);
  }
}

internal static class NpcNetworkPacketProducerGuards
{
  internal static void RequireCommitted(NpcView npc, byte packet)
  {
    ArgumentNullException.ThrowIfNull(npc);
    if (!npc.Entity.IsValid || npc.Revision <= 0 || !npc.CommitVersion.IsValid)
    {
      throw new InvalidOperationException($"Packet {packet} requires a committed NPC generation.");
    }
  }

  internal static void RequireFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentException("Packet coordinates must be finite.", parameterName);
    }
  }
}

