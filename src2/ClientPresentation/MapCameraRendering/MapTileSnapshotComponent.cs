namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class MapTileSnapshotComponent : IEquatable<MapTileSnapshotComponent>
{
  private const byte ChangedMask = 0x80;
  private const byte UpdateQueuedMask = 0x40;
  private const byte ColorMask = 0x1F;

  private byte _extraData;

  public MapTileSnapshotComponent(ushort type, byte light, byte color)
  {
    Type = type;
    Light = light;
    Color = color;
  }

  internal MapTileSnapshotComponent(ushort type, byte light, byte extraData, bool packed)
  {
    Type = type;
    Light = light;
    _extraData = extraData;
  }

  public ushort Type { get; private set; }

  public byte Light { get; private set; }

  public byte Color
  {
    get => (byte)(_extraData & ColorMask);
    private set => _extraData = (byte)((_extraData & ~ColorMask) | (value & ColorMask));
  }

  public bool IsChanged
  {
    get => (_extraData & ChangedMask) != 0;
    set => _extraData = value
      ? (byte)(_extraData | ChangedMask)
      : (byte)(_extraData & ~ChangedMask);
  }

  public bool UpdateQueued
  {
    get => (_extraData & UpdateQueuedMask) != 0;
    set => _extraData = value
      ? (byte)(_extraData | UpdateQueuedMask)
      : (byte)(_extraData & ~UpdateQueuedMask);
  }

  public byte PackedExtraData => _extraData;

  public MapTileSnapshotValue ToValue()
  {
    return new MapTileSnapshotValue(Type, Light, Color, IsChanged, UpdateQueued);
  }

  public void Clear()
  {
    Type = 0;
    Light = 0;
    _extraData = 0;
  }

  public bool Equals(MapTileSnapshotComponent? other)
  {
    return other is not null &&
      Type == other.Type &&
      Light == other.Light &&
      _extraData == other._extraData;
  }

  public override bool Equals(object? obj)
  {
    return obj is MapTileSnapshotComponent other && Equals(other);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(Type, Light, _extraData);
  }
}
