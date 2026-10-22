using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Host;

/// <summary>Projects one committed world snapshot into the Version4 packet-7 response.</summary>
public sealed class WorldDataPacketProducer
{
  private readonly Version4WireProfile _wireProfile;

  public WorldDataPacketProducer(Version4WireProfile wireProfile)
  {
    ArgumentNullException.ThrowIfNull(wireProfile);
    _wireProfile = wireProfile;
  }

  public OutboundPacketIntent CreateIntent(
    WorldSnapshot world,
    WorldDataPacketSupplementSnapshot supplement,
    IReadOnlyCollection<SessionEpoch> recipients)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(supplement);
    ArgumentNullException.ThrowIfNull(recipients);
    if (recipients.Count == 0)
    {
      throw new ArgumentException("Packet 7 requires at least one recipient.", nameof(recipients));
    }

    WorldDataPacket packet = CreatePacket(world, supplement);
    Version4EncodedPacket encoded = _wireProfile.Encode(WireDirection.ServerToClient, packet);
    return new OutboundPacketIntent(
      recipients,
      ignoreEpoch: null,
      encoded.MessageId,
      encoded.Payload,
      world.Version);
  }

  /// <summary>Creates packet 7 from one matching world and persistence-facts snapshot.</summary>
  public WorldDataPacket CreatePacket(
    WorldSnapshot world,
    WorldDataPacketSupplementSnapshot supplement)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(supplement);
    if (supplement.WorldEpoch != world.WorldEpoch || supplement.CommitVersion != world.Version)
    {
      throw new InvalidOperationException(
        "Packet 7 supplemental facts do not match the captured world epoch and version.");
    }

    return WorldDataPacketProjection.Create(world.Header, supplement);
  }
}

/// <summary>Deterministically combines P03 header facts and same-version packet-7 supplements.</summary>
public static class WorldDataPacketProjection
{
  public static WorldDataPacket Create(
    WorldHeaderSnapshot header,
    WorldDataPacketSupplementSnapshot supplement)
  {
    ArgumentNullException.ThrowIfNull(header);
    ArgumentNullException.ThrowIfNull(supplement);

    WorldHeaderFacts facts = header.Facts;
    byte[] flagGroups = supplement.WorldFlagGroups.ToArray();
    SetFlag(flagGroups, 1, 5, facts.Crimson);
    SetFlag(flagGroups, 6, 4, facts.DrunkWorld);
    SetFlag(flagGroups, 6, 7, facts.GetGoodWorld);
    SetFlag(flagGroups, 7, 0, facts.TenthAnniversaryWorld);
    SetFlag(flagGroups, 7, 1, facts.DontStarveWorld);
    SetFlag(flagGroups, 7, 3, facts.NotTheBeesWorld);
    SetFlag(flagGroups, 7, 4, facts.RemixWorld);
    SetFlag(flagGroups, 9, 0, facts.NoTrapsWorld);
    SetFlag(flagGroups, 9, 1, facts.ZenithWorld);
    SetFlag(flagGroups, 9, 6, facts.SkyblockWorld);

    byte timeFlags = 0;
    if (header.DayTime)
    {
      timeFlags |= 1;
    }

    if (supplement.BloodMoon)
    {
      timeFlags |= 2;
    }

    if (supplement.Eclipse)
    {
      timeFlags |= 4;
    }

    return new WorldDataPacket
    {
      Time = checked((int)header.Time),
      TimeFlags = timeFlags,
      MoonPhase = header.MoonPhase,
      MaxTilesX = checked((short)header.Geometry.Width),
      MaxTilesY = checked((short)header.Geometry.Height),
      SpawnTileX = checked((short)header.Spawn.X),
      SpawnTileY = checked((short)header.Spawn.Y),
      WorldSurface = checked((short)facts.WorldSurface),
      RockLayer = checked((short)facts.RockLayer),
      WorldId = header.WorldId,
      WorldName = header.Name,
      GameMode = checked((byte)facts.GameMode),
      WorldGuid = header.UniqueId.ToByteArray(),
      WorldGeneratorVersion = facts.WorldGeneratorVersion ?? throw new InvalidDataException(
        "Packet 7 requires the Version4 world-generator version."),
      MoonType = facts.MoonType,
      BackgroundTypes = supplement.BackgroundTypes.ToArray(),
      SpecialBackgroundStyles =
      [
        checked((byte)facts.IceBackStyle),
        checked((byte)facts.JungleBackStyle),
        checked((byte)facts.HellBackStyle)
      ],
      WindSpeedTarget = supplement.WindSpeedTarget,
      CloudCount = supplement.CloudCount,
      TreePositions = facts.TreeX.ToArray(),
      TreeStyles = ToBytes(facts.TreeStyle),
      CaveBackgroundPositions = facts.CaveBackX.ToArray(),
      CaveBackgroundStyles = ToBytes(facts.CaveBackStyle),
      TreeTopStyles = supplement.TreeTopStyles.ToArray(),
      MaximumRain = supplement.IsRaining ? supplement.MaximumRain : 0f,
      WorldFlagGroups = flagGroups,
      SundialCooldown = supplement.SundialCooldown,
      MoondialCooldown = supplement.MoondialCooldown,
      SavedOreTiers = supplement.SavedOreTiers.ToArray(),
      InvasionType = supplement.InvasionType,
      LobbyId = supplement.LobbyId,
      SandstormSeverity = supplement.SandstormSeverity,
      ExtraSpawnPoints = supplement.ExtraSpawnPoints.ToArray(),
      DungeonX = checked((short)facts.DungeonX),
      DungeonY = checked((short)facts.DungeonY)
    };
  }

  private static byte[] ToBytes(IReadOnlyList<int> values)
  {
    var result = new byte[values.Count];
    for (int index = 0; index < values.Count; index++)
    {
      result[index] = checked((byte)values[index]);
    }

    return result;
  }

  private static void SetFlag(byte[] groups, int group, int bit, bool enabled)
  {
    byte mask = (byte)(1 << bit);
    if (enabled)
    {
      groups[group] |= mask;
    }
    else
    {
      groups[group] &= (byte)~mask;
    }
  }
}
