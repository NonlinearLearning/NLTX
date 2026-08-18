using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

public sealed class WldSectionPointerTable
{
  private const ulong FileMetadataMagicMask = 0x00FFFFFFFFFFFFFFUL;
  private const ulong FileMetadataMagicValue = 0x006369676F6C6572UL;
  private const byte WorldFileType = 2;
  private const int MaximumImportanceCount = 8_400;

  private readonly int[] _offsets;

  private WldSectionPointerTable(int[] offsets, bool[] tileImportance)
  {
    _offsets = offsets;
    Offsets = new ReadOnlyCollection<int>(_offsets);
    TileImportance = new LegacyReadOnlyList<bool>(tileImportance);
  }

  public IReadOnlyList<int> Offsets { get; }

  public int SectionCount => _offsets.Length;

  public IReadOnlyList<bool> TileImportance { get; }

  public static WldSectionPointerTable Read(int version, WldBinaryReader reader)
  {
    if (version >= 135)
    {
      ReadFileMetadata(reader);
    }

    short count = reader.ReadInt16();
    int expectedCount = GetExpectedSectionCount(version);
    if (count != expectedCount)
    {
      throw new InvalidDataException(
        $"The WLD section count {count} does not match version {version}.");
    }

    int[] offsets = new int[count];
    for (int index = 0; index < offsets.Length; index++)
    {
      offsets[index] = reader.ReadInt32();
    }

    ushort importanceCount = reader.ReadUInt16();
    if (importanceCount > MaximumImportanceCount)
    {
      throw new InvalidDataException("The WLD tile-importance count exceeds supported limits.");
    }

    int importanceByteCount = (importanceCount + 7) / 8;
    byte[] packedImportance = reader.ReadBytes(importanceByteCount);
    bool[] tileImportance = new bool[importanceCount];
    for (int index = 0; index < tileImportance.Length; index++)
    {
      tileImportance[index] = (packedImportance[index / 8] & (1 << (index % 8))) != 0;
    }

    ValidateOffsets(offsets, reader.Position, reader.SectionEnd);
    return new WldSectionPointerTable(offsets, tileImportance);
  }

  public WldBinaryReader CreateSectionReader(WldBinaryReader reader, int sectionIndex)
  {
    if (sectionIndex < 0 || sectionIndex >= _offsets.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(sectionIndex));
    }

    long sectionEnd = sectionIndex == _offsets.Length - 1
      ? reader.SectionEnd
      : _offsets[sectionIndex + 1];
    return reader.CreateBoundedReader(_offsets[sectionIndex], sectionEnd);
  }

  public static int GetExpectedSectionCount(int version)
  {
    int count = 6;
    if (version >= 116)
    {
      count++;
    }

    if (version >= 170)
    {
      count++;
    }

    if (version >= 189)
    {
      count++;
    }

    if (version >= 210)
    {
      count++;
    }

    if (version >= 220)
    {
      count++;
    }

    return count;
  }

  private static void ReadFileMetadata(WldBinaryReader reader)
  {
    ulong encodedTypeAndMagic = reader.ReadUInt64();
    byte fileType = (byte)(encodedTypeAndMagic >> 56);
    if ((encodedTypeAndMagic & FileMetadataMagicMask) != FileMetadataMagicValue ||
        fileType != WorldFileType)
    {
      throw new InvalidDataException("The WLD file metadata is not a world metadata record.");
    }

    _ = reader.ReadUInt32();
    _ = reader.ReadUInt64();
  }

  private static void ValidateOffsets(int[] offsets, long firstSectionOffset, long fileEnd)
  {
    if (offsets[0] != firstSectionOffset)
    {
      throw new InvalidDataException("The first WLD section pointer does not follow the table.");
    }

    int previousOffset = -1;
    for (int index = 0; index < offsets.Length; index++)
    {
      int offset = offsets[index];
      if (offset < firstSectionOffset || offset >= fileEnd)
      {
        throw new InvalidDataException("A WLD section pointer is outside the input file.");
      }

      if (index > 0 && offset <= previousOffset)
      {
        throw new InvalidDataException("WLD section pointers must be strictly increasing.");
      }

      previousOffset = offset;
    }
  }
}
