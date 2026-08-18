using System;
using System.Collections.Generic;
using System.IO;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

public static class WldTileRleReader
{
  public static IReadOnlyList<LegacyTile> Read(
    int version,
    WldBinaryReader reader,
    int width,
    int height,
    IReadOnlyList<bool> tileImportance)
  {
    if (version < 88)
    {
      throw new InvalidDataException("The WLD tile-section version is invalid.");
    }

    if (tileImportance is null)
    {
      throw new ArgumentNullException(nameof(tileImportance));
    }

    reader.Limits.ValidateWorldDimensions(width, height);
    int tileCount = checked(width * height);
    LegacyTile[] tiles = new LegacyTile[tileCount];
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        byte header = reader.ReadByte();
        byte header1 = (header & 1) != 0 ? reader.ReadByte() : (byte)0;
        byte header2 = (header1 & 1) != 0 ? reader.ReadByte() : (byte)0;
        byte header3 = (header2 & 1) != 0 ? reader.ReadByte() : (byte)0;
        bool isActive = (header & 2) != 0;
        ushort tileType = 0;
        short frameX = -1;
        short frameY = -1;
        byte tileColor = 0;
        if (isActive)
        {
          tileType = (header & 0x20) != 0
            ? (ushort)(reader.ReadByte() | (reader.ReadByte() << 8))
            : reader.ReadByte();
          if (tileType < tileImportance.Count && tileImportance[tileType])
          {
            frameX = reader.ReadInt16();
            frameY = reader.ReadInt16();
            if (tileType == 144)
            {
              frameY = 0;
            }
          }

          if ((header2 & 8) != 0)
          {
            tileColor = reader.ReadByte();
          }
        }

        ushort wallType = 0;
        byte wallColor = 0;
        if ((header & 4) != 0)
        {
          wallType = reader.ReadByte();
          if ((header2 & 0x10) != 0) wallColor = reader.ReadByte();
        }

        byte liquidAmount = 0;
        byte liquidKind = 0;
        byte liquidType = (byte)((header & 0x18) >> 3);
        if (liquidType != 0)
        {
          liquidAmount = reader.ReadByte();
          liquidKind = (byte)((header2 & 0x80) != 0
            ? (byte)3
            : liquidType switch
            {
              1 => 0,
              2 => 1,
              3 => 2,
              _ => throw new InvalidDataException("The WLD tile liquid type is invalid.")
            });
        }

        bool hasWire = (header1 & 2) != 0;
        bool hasWire2 = (header1 & 4) != 0;
        bool hasWire3 = (header1 & 8) != 0;
        byte tileShape = (byte)((header1 & 0x70) >> 4);
        bool isHalfBrick = tileShape == 1;
        byte slope = tileShape > 1 ? (byte)(tileShape - 1) : (byte)0;
        bool isActuated = (header2 & 2) != 0;
        bool isInactive = (header2 & 4) != 0;
        bool hasWire4 = (header2 & 0x20) != 0;
        if ((header2 & 0x40) != 0) wallType |= (ushort)(reader.ReadByte() << 8);
        LegacyTile tile = new(
          isActive,
          tileType,
          wallType,
          liquidAmount,
          liquidKind,
          hasWire,
          hasWire2,
          hasWire3,
          hasWire4,
          isHalfBrick,
          slope,
          isActuated,
          isInactive,
          frameX,
          frameY,
          tileColor,
          wallColor,
          (header3 & 2) != 0,
          (header3 & 4) != 0,
          (header3 & 8) != 0,
          (header3 & 0x10) != 0);
        int runLength = (header & 0xC0) switch
        {
          0x00 => 0,
          0x40 => reader.ReadByte(),
          _ => reader.ReadInt16()
        };
        if (runLength < 0 || runLength > height - y - 1)
        {
          throw new InvalidDataException("The WLD tile RLE run crosses a column boundary.");
        }
        for (int repeat = 0; repeat <= runLength; repeat++)
        {
          int tileIndex = checked((x * height) + y + repeat);
          tiles[tileIndex] = tile;
        }

        y += runLength;
      }
    }

    reader.RequireSectionEnd();
    return new LegacyReadOnlyList<LegacyTile>(tiles);
  }
}
