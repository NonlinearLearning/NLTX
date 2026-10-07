using System.IO;

namespace Terraria.Player;

/// <summary>
/// Decodes the packet-147 bytes observed in Version4.
/// It does not mutate loadout state or publish a network response.
/// </summary>
public static class PlayerLoadoutPacket147Adapter
{
  public static bool TryReadHeader(
    BinaryReader reader,
    Guid commandId,
    int authorityPlayerIndex,
    int mainPlayerIndex,
    bool usingOrReusingItem,
    bool cced,
    bool dead,
    out PlayerLoadoutPacket147Header header)
  {
    ArgumentNullException.ThrowIfNull(reader);
    header = default;

    try
    {
      header = new PlayerLoadoutPacket147Header(
        commandId,
        reader.ReadByte(),
        authorityPlayerIndex,
        mainPlayerIndex,
        reader.ReadByte(),
        usingOrReusingItem,
        cced,
        dead);
      return true;
    }
    catch (EndOfStreamException)
    {
      return false;
    }
    catch (ObjectDisposedException)
    {
      return false;
    }
  }

  public static bool TryReadVisibilityMask(
    BinaryReader reader,
    out ushort visibilityMask)
  {
    ArgumentNullException.ThrowIfNull(reader);
    visibilityMask = default;

    try
    {
      visibilityMask = reader.ReadUInt16();
      return true;
    }
    catch (EndOfStreamException)
    {
      return false;
    }
    catch (ObjectDisposedException)
    {
      return false;
    }
  }

  public static PlayerLoadoutPacket147DecodeResult Decode(
    BinaryReader reader,
    Guid commandId,
    int authorityPlayerIndex,
    int mainPlayerIndex,
    bool usingOrReusingItem,
    bool cced,
    bool dead)
  {
    ArgumentNullException.ThrowIfNull(reader);

    try
    {
      int packetPlayerIndex = reader.ReadByte();
      int targetLoadoutIndex = reader.ReadByte();
      ushort visibilityMask = reader.ReadUInt16();
      return new PlayerLoadoutPacket147DecodeResult(
        PlayerLoadoutPacket147DecodeStatus.Decoded,
        new PlayerLoadoutNetworkRequest(
          commandId,
          packetPlayerIndex,
          authorityPlayerIndex,
          mainPlayerIndex,
          targetLoadoutIndex,
          usingOrReusingItem,
          cced,
          dead,
          visibilityMask));
    }
    catch (EndOfStreamException)
    {
      return PlayerLoadoutPacket147DecodeResult.Truncated();
    }
    catch (ObjectDisposedException)
    {
      return PlayerLoadoutPacket147DecodeResult.Truncated();
    }
  }
}
