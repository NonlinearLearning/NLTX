using System;
using System.Collections.Generic;
using System.IO;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

public static class WldTileEntityReader
{
  public static IReadOnlyList<LegacyTileEntity> Read(
    int version,
    WldBinaryReader reader,
    LegacyWorldMetadata metadata,
    WldReadLimits limits)
  {
    int count = reader.ReadInt32();
    if (count < 0 || count > limits.MaxEntityCount)
    {
      throw new InvalidDataException("The WLD tile-entity count exceeds configured limits.");
    }

    HashSet<(int X, int Y)> coordinates = [];
    List<LegacyTileEntity> entities = new(count);
    for (int index = 0; index < count; index++)
    {
      byte type = reader.ReadByte();
      int id = reader.ReadInt32();
      int x = reader.ReadInt16();
      int y = reader.ReadInt16();
      if (x < 0 || x >= metadata.Width || y < 0 || y >= metadata.Height)
      {
        throw new InvalidDataException("A WLD tile-entity coordinate is outside the world.");
      }

      if (!coordinates.Add((x, y)))
      {
        throw new InvalidDataException("A WLD tile-entity coordinate is duplicated.");
      }

      bool isOpaque = !IsKnownType(type);
      byte[] payload = isOpaque
        ? ReadOpaquePayload(reader)
        : ReadPayload(type, version, reader);
      entities.Add(new LegacyTileEntity(id, type, x, y, payload, isOpaque));
      if (isOpaque)
      {
        break;
      }
    }

    reader.RequireSectionEnd();
    return entities;
  }

  private static byte[] ReadPayload(byte type, int version, WldBinaryReader reader)
  {
    return type switch
    {
      0 => ReadInt16Payload(reader),
      1 => ReadItemPayload(reader),
      2 => ReadLogicSensorPayload(reader),
      3 => ReadDisplayDollPayload(version, reader),
      5 => ReadHatRackPayload(reader),
      4 => ReadItemPayload(reader),
      6 => ReadItemPayload(reader),
      7 => [],
      8 => ReadItemPayload(reader),
      9 => ReadInt16Payload(reader),
      10 => ReadInt16Payload(reader),
      _ => throw new InvalidDataException("The WLD tile-entity type is not recognized.")
    };
  }

  private static bool IsKnownType(byte type)
  {
    return type <= 10;
  }

  private static byte[] ReadOpaquePayload(WldBinaryReader reader)
  {
    int remainingBytes = checked((int)(reader.SectionEnd - reader.Position));
    return reader.ReadBytes(remainingBytes);
  }

  private static byte[] ReadInt16Payload(WldBinaryReader reader)
  {
    short value = reader.ReadInt16();
    return [(byte)value, (byte)(value >> 8)];
  }

  private static byte[] ReadItemPayload(WldBinaryReader reader)
  {
    short itemType = reader.ReadInt16();
    byte prefix = reader.ReadByte();
    short stack = reader.ReadInt16();
    return [(byte)itemType, (byte)(itemType >> 8), prefix, (byte)stack, (byte)(stack >> 8)];
  }

  private static byte[] ReadLogicSensorPayload(WldBinaryReader reader)
  {
    byte logicCheckType = reader.ReadByte();
    bool isOn = reader.ReadBoolean();
    return [logicCheckType, isOn ? (byte)1 : (byte)0];
  }

  private static byte[] ReadDisplayDollPayload(int version, WldBinaryReader reader)
  {
    List<byte> payload = [];
    byte equipFlags = ReadByte(reader, payload);
    byte dyeFlags = ReadByte(reader, payload);
    if (version >= 307)
    {
      _ = ReadByte(reader, payload);
    }

    byte extendedFlags = version >= 308 ? ReadByte(reader, payload) : (byte)0;
    bool legacyMountFlag = version == 311 && (extendedFlags & 2) != 0;
    int equipMask = equipFlags | ((extendedFlags & 2) != 0 ? 256 : 0);
    int dyeMask = dyeFlags | ((extendedFlags & 4) != 0 ? 256 : 0);
    for (int index = 0; index < 9; index++)
    {
      if ((equipMask & (1 << index)) != 0)
      {
        ReadItemPayload(reader, payload);
      }
    }

    for (int index = 0; index < 9; index++)
    {
      if ((dyeMask & (1 << index)) != 0)
      {
        ReadItemPayload(reader, payload);
      }
    }

    if ((extendedFlags & 1) != 0)
    {
      ReadItemPayload(reader, payload);
    }

    if (legacyMountFlag)
    {
      ReadItemPayload(reader, payload);
    }

    return payload.ToArray();
  }

  private static byte[] ReadHatRackPayload(WldBinaryReader reader)
  {
    List<byte> payload = [];
    byte flags = ReadByte(reader, payload);
    for (int index = 0; index < 4; index++)
    {
      if ((flags & (1 << index)) != 0)
      {
        ReadItemPayload(reader, payload);
      }
    }

    return payload.ToArray();
  }

  private static byte ReadByte(WldBinaryReader reader, List<byte> payload)
  {
    byte value = reader.ReadByte();
    payload.Add(value);
    return value;
  }

  private static void ReadItemPayload(WldBinaryReader reader, List<byte> payload)
  {
    short itemType = reader.ReadInt16();
    byte prefix = reader.ReadByte();
    short stack = reader.ReadInt16();
    payload.Add((byte)itemType);
    payload.Add((byte)(itemType >> 8));
    payload.Add(prefix);
    payload.Add((byte)stack);
    payload.Add((byte)(stack >> 8));
  }
}
