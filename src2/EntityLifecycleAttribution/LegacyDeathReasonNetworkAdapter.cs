using System.IO;

namespace Terraria.EntityLifecycleAttribution;

public static class LegacyDeathReasonNetworkAdapter
{
  public static byte[] Serialize(PlayerDeathAttributionSnapshot snapshot)
  {
    using MemoryStream stream = new();
    using (BinaryWriter writer = new(stream, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(snapshot.SourcePlayerIndex);
      writer.Write(snapshot.SourceNpcIndex);
      writer.Write(snapshot.SourceProjectileLocalIndex);
      writer.Write(snapshot.SourceOtherIndex);
      writer.Write(snapshot.SourceProjectileType);
      writer.Write(snapshot.SourceItemType);
      writer.Write(snapshot.SourceItemPrefix);
      writer.Write(snapshot.CustomReason is not null);
      if (snapshot.CustomReason is not null)
      {
        writer.Write(snapshot.CustomReason);
      }
    }

    return stream.ToArray();
  }

  public static bool TryDeserialize(
    ReadOnlySpan<byte> payload,
    out PlayerDeathAttributionSnapshot snapshot)
  {
    snapshot = default;

    try
    {
      using MemoryStream stream = new(payload.ToArray(), writable: false);
      using BinaryReader reader = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);
      int sourcePlayerIndex = reader.ReadInt32();
      int sourceNpcIndex = reader.ReadInt32();
      int sourceProjectileLocalIndex = reader.ReadInt32();
      int sourceOtherIndex = reader.ReadInt32();
      int sourceProjectileType = reader.ReadInt32();
      int sourceItemType = reader.ReadInt32();
      int sourceItemPrefix = reader.ReadInt32();
      string? customReason = reader.ReadBoolean() ? reader.ReadString() : null;

      if (stream.Position != stream.Length)
      {
        return false;
      }

      snapshot = new PlayerDeathAttributionSnapshot(
        sourcePlayerIndex,
        sourceNpcIndex,
        sourceProjectileLocalIndex,
        sourceOtherIndex,
        sourceProjectileType,
        sourceItemType,
        sourceItemPrefix,
        customReason);
      return true;
    }
    catch (ArgumentException)
    {
      return false;
    }
    catch (EndOfStreamException)
    {
      return false;
    }
    catch (IOException)
    {
      return false;
    }
  }
}
