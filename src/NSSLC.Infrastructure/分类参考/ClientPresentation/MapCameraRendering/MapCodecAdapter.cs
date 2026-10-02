using System.Buffers.Binary;

namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class MapCodecAdapter
{
  private const int EncodedTileLength = 4;

  private readonly MapEncodingCatalogComponent _catalog;

  public MapCodecAdapter(MapEncodingCatalogComponent catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    if (!MapEncodingCatalogQuery.IsValid(catalog))
    {
      throw new ArgumentException("The map encoding catalog is invalid.", nameof(catalog));
    }

    _catalog = catalog;
  }

  public byte[] Encode(MapTileSnapshotComponent tile)
  {
    ArgumentNullException.ThrowIfNull(tile);
    byte[] buffer = new byte[EncodedTileLength];
    BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(0, 2), tile.Type);
    buffer[2] = tile.Light;
    buffer[3] = tile.PackedExtraData;
    return buffer;
  }

  public MapTileSnapshotComponent Decode(ReadOnlySpan<byte> encoded)
  {
    if (encoded.Length != EncodedTileLength)
    {
      throw new ArgumentException(
        $"A map tile requires {EncodedTileLength} bytes.",
        nameof(encoded));
    }

    ushort type = BinaryPrimitives.ReadUInt16LittleEndian(encoded[..2]);
    return new MapTileSnapshotComponent(type, encoded[2], encoded[3], packed: true);
  }

  public MapEncodingCatalogComponent Catalog => _catalog;
}
