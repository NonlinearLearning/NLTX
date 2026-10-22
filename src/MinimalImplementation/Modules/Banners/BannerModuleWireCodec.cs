using MinimalImplementation.Server.Modules.Banners;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Modules.Banners;

/// <summary>Version4 Banner codec for actions 0 through 4.</summary>
public sealed class BannerModuleWireCodec : IPacket82ModuleWireCodec
{
  public const int MaximumBannerTypes = 293;

  public ushort ModuleId => (ushort)Packet82ModuleId.Banners;

  public void Write(
    PacketWireWriter writer,
    object? payload,
    int tagEffectNpcSlotCount,
    Func<short, bool> tagEffectUsesProcTimes)
  {
    ArgumentNullException.ThrowIfNull(writer);
    ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
    if (tagEffectNpcSlotCount is < 0 or > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(tagEffectNpcSlotCount));
    }

    switch (payload)
    {
      case Packet82BannerData data:
        WriteExistingAction(writer, data);
        break;
      case Packet82BannerClaimRequestData request:
        ValidateBannerId(request.BannerId);
        writer.WriteByte(3);
        writer.WriteInt16(request.BannerId);
        writer.WriteUInt16(request.Amount);
        break;
      case Packet82BannerClaimResponseData response:
        ValidateBannerId(response.BannerId);
        writer.WriteByte(4);
        writer.WriteInt16(response.BannerId);
        writer.WriteUInt16(response.Amount);
        writer.WriteBoolean(response.Granted);
        break;
      default:
        throw new PacketWireFormatException("The Packet 82 banner payload type is unsupported.");
    }
  }

  public object Read(
    PacketWireReader reader,
    int tagEffectNpcSlotCount,
    Func<short, bool> tagEffectUsesProcTimes)
  {
    ArgumentNullException.ThrowIfNull(reader);
    ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
    if (tagEffectNpcSlotCount is < 0 or > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(tagEffectNpcSlotCount));
    }

    byte action = reader.ReadByte();
    return action switch
    {
      0 => ReadFullState(reader),
      1 => ReadKillCount(reader),
      2 => ReadClaimableCount(reader),
      3 => new Packet82BannerClaimRequestData(ReadBannerId(reader), reader.ReadUInt16()),
      4 => new Packet82BannerClaimResponseData(
        ReadBannerId(reader), reader.ReadUInt16(), ReadBoolean(reader)),
      _ => throw new PacketWireFormatException($"Unknown Packet 82 banner action {action}.")
    };
  }

  private static bool ReadBoolean(PacketWireReader reader)
  {
    byte value = reader.ReadByte();
    if (value > 1)
    {
      throw new PacketWireFormatException("Banner claim response booleans require 0 or 1.");
    }

    return value != 0;
  }

  public static IReadOnlyDictionary<ushort, IPacket82ModuleWireCodec> ReplaceInRegistry(
    IEnumerable<IPacket82ModuleWireCodec> codecs)
  {
    ArgumentNullException.ThrowIfNull(codecs);
    Dictionary<ushort, IPacket82ModuleWireCodec> map = new();
    bool replaced = false;
    foreach (IPacket82ModuleWireCodec codec in codecs)
    {
      ArgumentNullException.ThrowIfNull(codec);
      if (codec.ModuleId == (ushort)Packet82ModuleId.Banners)
      {
        if (replaced)
        {
          throw new ArgumentException("The Packet 82 banner module was registered more than once.", nameof(codecs));
        }

        map.Add(codec.ModuleId, new BannerModuleWireCodec());
        replaced = true;
      }
      else if (!map.TryAdd(codec.ModuleId, codec))
      {
        throw new ArgumentException($"Packet 82 module {codec.ModuleId} was registered more than once.", nameof(codecs));
      }
    }

    if (!replaced)
    {
      throw new ArgumentException("The Packet 82 banner module registration is missing.", nameof(codecs));
    }

    return Packet82KnownModuleCodecsV4.Create(map.Values);
  }

  private static void WriteExistingAction(PacketWireWriter writer, Packet82BannerData payload)
  {
    switch (payload.Action)
    {
      case 0:
        if (payload.KillCounts?.Count != MaximumBannerTypes
          || payload.ClaimableCounts?.Count != MaximumBannerTypes)
        {
          throw new PacketWireFormatException("Version4 full banner state requires exactly 293 entries in both tables.");
        }

        writer.WriteByte(0);
        writer.WriteInt16(MaximumBannerTypes);
        foreach (int count in payload.KillCounts)
        {
          if (count < 0)
          {
            throw new PacketWireFormatException("Banner kill counts cannot be negative.");
          }

          writer.WriteInt32(count);
        }

        writer.WriteInt16(MaximumBannerTypes);
        foreach (ushort count in payload.ClaimableCounts)
        {
          writer.WriteUInt16(count);
        }

        break;
      case 1:
        short killBannerId = RequireBannerId(payload.BannerId);
        if (payload.KillCount is not int killCount || killCount < 0)
        {
          throw new PacketWireFormatException("Banner kill updates require a non-negative kill count.");
        }

        writer.WriteByte(1);
        writer.WriteInt16(killBannerId);
        writer.WriteInt32(killCount);
        break;
      case 2:
        short claimBannerId = RequireBannerId(payload.BannerId);
        if (payload.ClaimableCount is not ushort claimableCount)
        {
          throw new PacketWireFormatException("Banner claim updates require a claimable count.");
        }

        writer.WriteByte(2);
        writer.WriteInt16(claimBannerId);
        writer.WriteUInt16(claimableCount);
        break;
      default:
        throw new PacketWireFormatException($"Unknown Packet 82 banner action {payload.Action}.");
    }
  }

  private static Packet82BannerData ReadFullState(PacketWireReader reader)
  {
    short killCount = reader.ReadInt16();
    if (killCount != MaximumBannerTypes)
    {
      throw new PacketWireFormatException("Version4 banner kill table must contain exactly 293 entries.");
    }

    int[] kills = new int[killCount];
    for (int index = 0; index < kills.Length; index++)
    {
      kills[index] = reader.ReadInt32();
      if (kills[index] < 0)
      {
        throw new PacketWireFormatException("Banner kill counts cannot be negative.");
      }
    }

    short claimCount = reader.ReadInt16();
    if (claimCount != MaximumBannerTypes)
    {
      throw new PacketWireFormatException("Version4 banner claim table must contain exactly 293 entries.");
    }

    ushort[] claims = new ushort[claimCount];
    for (int index = 0; index < claims.Length; index++)
    {
      claims[index] = reader.ReadUInt16();
    }

    return new Packet82BannerData(0, kills, claims, null, null, null);
  }

  private static Packet82BannerData ReadKillCount(PacketWireReader reader)
  {
    short bannerId = ReadBannerId(reader);
    int killCount = reader.ReadInt32();
    if (killCount < 0)
    {
      throw new PacketWireFormatException("Banner kill counts cannot be negative.");
    }

    return new Packet82BannerData(1, null, null, bannerId, killCount, null);
  }

  private static Packet82BannerData ReadClaimableCount(PacketWireReader reader)
  {
    short bannerId = ReadBannerId(reader);
    ushort claimableCount = reader.ReadUInt16();
    return new Packet82BannerData(2, null, null, bannerId, null, claimableCount);
  }

  private static short ReadBannerId(PacketWireReader reader)
  {
    short bannerId = reader.ReadInt16();
    ValidateBannerId(bannerId);
    return bannerId;
  }

  private static short RequireBannerId(short? bannerId)
  {
    if (bannerId is not short value)
    {
      throw new PacketWireFormatException("A banner payload must include a banner ID.");
    }

    ValidateBannerId(value);
    return value;
  }

  private static void ValidateBannerId(short bannerId)
  {
    if (bannerId < 0 || bannerId >= MaximumBannerTypes)
    {
      throw new PacketWireFormatException($"Banner ID {bannerId} is outside the Version4 table.");
    }
  }
}
