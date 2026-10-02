using System.Buffers.Binary;

namespace Terraria.NonAuthoritative.Persistence;

public static class FileMetadataCodec
{
  public const ulong MagicNumber = 27981915666277746UL;
  public const int Size = 20;

  private const ulong MagicMask = 0x00FFFFFFFFFFFFFFUL;

  public static byte[] Write(FileMetadataValue metadata)
  {
    if (!IsKnownType(metadata.Type))
    {
      throw new ArgumentOutOfRangeException(nameof(metadata), "A known save file type is required.");
    }

    byte[] bytes = new byte[Size];
    ulong typeAndMagic = MagicNumber | ((ulong)metadata.Type << 56);
    BinaryPrimitives.WriteUInt64LittleEndian(bytes, typeAndMagic);
    BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(8), metadata.Revision);
    BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(12), metadata.IsFavorite ? 1UL : 0UL);
    return bytes;
  }

  public static FileMetadataValue Read(ReadOnlySpan<byte> bytes, SaveFileType expectedType)
  {
    if (bytes.Length != Size)
    {
      throw new FormatException($"Expected {Size} metadata bytes but found {bytes.Length}.");
    }

    if (!IsKnownType(expectedType))
    {
      throw new ArgumentOutOfRangeException(nameof(expectedType), "A known expected file type is required.");
    }

    ulong typeAndMagic = BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    if ((typeAndMagic & MagicMask) != MagicNumber)
    {
      throw new FormatException("Expected Re-Logic file format.");
    }

    SaveFileType actualType = (SaveFileType)(typeAndMagic >> 56);
    if (!IsKnownType(actualType))
    {
      throw new FormatException("Found invalid file type.");
    }

    if (actualType != expectedType)
    {
      throw new FormatException($"Expected type '{expectedType}' but found '{actualType}'.");
    }

    uint revision = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(8, 4));
    bool isFavorite = (BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(12, 8)) & 1UL) != 0;
    return new FileMetadataValue(actualType, revision, isFavorite);
  }

  public static bool IsKnownType(SaveFileType type)
  {
    return type is SaveFileType.Map or SaveFileType.World or SaveFileType.Player;
  }
}
