using System.IO.Compression;
using System.Text;

namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class MapPersistenceAdapter : IDisposable
{
  private static readonly byte[] Magic = [(byte)'P', (byte)'1', (byte)'8', (byte)'M'];
  private const byte FormatVersion = 1;

  private readonly MapCodecAdapter _codec;
  private readonly SemaphoreSlim _ioLock = new(1, 1);
  private readonly int _maxRetries;
  private bool _disposed;

  public MapPersistenceAdapter(MapCodecAdapter codec, int maxRetries = 1)
  {
    _codec = codec ?? throw new ArgumentNullException(nameof(codec));
    if (maxRetries < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxRetries));
    }

    _maxRetries = maxRetries;
  }

  public MapPersistenceResult Save(string path, MapPersistenceSnapshot snapshot)
  {
    if (!TryPreparePath(path, out string fullPath, out MapPersistenceFailureKind pathFailure))
    {
      return new MapPersistenceResult(false, pathFailure, 0, null);
    }

    ArgumentNullException.ThrowIfNull(snapshot);
    if (!_ioLock.Wait(0))
    {
      return new MapPersistenceResult(false, MapPersistenceFailureKind.LockBusy, 0, null);
    }

    try
    {
      for (int attempt = 1; attempt <= _maxRetries + 1; attempt++)
      {
        string temporaryPath = $"{fullPath}.{Guid.NewGuid():N}.tmp";
        try
        {
          WriteDocument(temporaryPath, snapshot);
          ReplaceDocument(temporaryPath, fullPath);
          return new MapPersistenceResult(true, MapPersistenceFailureKind.None, attempt, null);
        }
        catch (DirectoryNotFoundException)
        {
          DeleteTemporary(temporaryPath);
          return new MapPersistenceResult(
            false,
            MapPersistenceFailureKind.DirectoryMissing,
            attempt,
            nameof(DirectoryNotFoundException));
        }
        catch (UnauthorizedAccessException exception) when (attempt <= _maxRetries)
        {
          DeleteTemporary(temporaryPath);
          _ = exception;
        }
        catch (IOException exception) when (attempt <= _maxRetries)
        {
          DeleteTemporary(temporaryPath);
          _ = exception;
        }
        catch (UnauthorizedAccessException exception)
        {
          DeleteTemporary(temporaryPath);
          return new MapPersistenceResult(
            false,
            MapPersistenceFailureKind.IoFailure,
            attempt,
            exception.GetType().Name);
        }
        catch (IOException exception)
        {
          DeleteTemporary(temporaryPath);
          return new MapPersistenceResult(
            false,
            MapPersistenceFailureKind.IoFailure,
            attempt,
            exception.GetType().Name);
        }
        catch (ArgumentException exception)
        {
          DeleteTemporary(temporaryPath);
          return new MapPersistenceResult(
            false,
            MapPersistenceFailureKind.CodecFailure,
            attempt,
            exception.GetType().Name);
        }
      }
    }
    finally
    {
      _ioLock.Release();
    }

    return new MapPersistenceResult(false, MapPersistenceFailureKind.IoFailure, _maxRetries + 1, null);
  }

  public MapLoadResult Load(string path)
  {
    if (!TryPreparePath(path, out string fullPath, out MapPersistenceFailureKind pathFailure))
    {
      return new MapLoadResult(false, null, pathFailure, 0, null);
    }

    if (!_ioLock.Wait(0))
    {
      return new MapLoadResult(false, null, MapPersistenceFailureKind.LockBusy, 0, null);
    }

    try
    {
      for (int attempt = 1; attempt <= _maxRetries + 1; attempt++)
      {
        try
        {
          MapPersistenceSnapshot snapshot = ReadDocument(fullPath);
          return new MapLoadResult(true, snapshot, MapPersistenceFailureKind.None, attempt, null);
        }
        catch (FileNotFoundException exception)
        {
          return new MapLoadResult(
            false,
            null,
            MapPersistenceFailureKind.IoFailure,
            attempt,
            exception.GetType().Name);
        }
        catch (DirectoryNotFoundException exception)
        {
          return new MapLoadResult(
            false,
            null,
            MapPersistenceFailureKind.DirectoryMissing,
            attempt,
            exception.GetType().Name);
        }
        catch (InvalidDataException exception)
        {
          return new MapLoadResult(
            false,
            null,
            MapPersistenceFailureKind.CorruptDocument,
            attempt,
            exception.GetType().Name);
        }
        catch (IOException exception) when (attempt <= _maxRetries)
        {
          _ = exception;
        }
        catch (IOException exception)
        {
          return new MapLoadResult(
            false,
            null,
            MapPersistenceFailureKind.IoFailure,
            attempt,
            exception.GetType().Name);
        }
        catch (NotSupportedException exception)
        {
          return new MapLoadResult(
            false,
            null,
            MapPersistenceFailureKind.UnsupportedVersion,
            attempt,
            exception.GetType().Name);
        }
        catch (ArgumentException exception)
        {
          return new MapLoadResult(
            false,
            null,
            MapPersistenceFailureKind.CodecFailure,
            attempt,
            exception.GetType().Name);
        }
      }
    }
    finally
    {
      _ioLock.Release();
    }

    return new MapLoadResult(
      false,
      null,
      MapPersistenceFailureKind.IoFailure,
      _maxRetries + 1,
      null);
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    _ioLock.Dispose();
  }

  private void WriteDocument(string temporaryPath, MapPersistenceSnapshot snapshot)
  {
    using FileStream stream = new(
      temporaryPath,
      FileMode.CreateNew,
      FileAccess.Write,
      FileShare.None);
    using ZLibStream compressed = new(stream, CompressionLevel.Fastest);
    using BinaryWriter writer = new(compressed, Encoding.UTF8, leaveOpen: false);
    writer.Write(Magic);
    writer.Write(FormatVersion);
    writer.Write(snapshot.MaxWidth);
    writer.Write(snapshot.MaxHeight);
    writer.Write(snapshot.BlackEdgeWidth);
    writer.Write(snapshot.SourceRevision);
    writer.Write(snapshot.Tiles.Length);
    foreach (MapTileSnapshotValue value in snapshot.Tiles.Span)
    {
      MapTileSnapshotComponent tile = new(value.Type, value.Light, value.Color)
      {
        IsChanged = value.IsChanged,
        UpdateQueued = value.UpdateQueued
      };
      writer.Write(_codec.Encode(tile));
    }
  }

  private MapPersistenceSnapshot ReadDocument(string path)
  {
    using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    using ZLibStream compressed = new(stream, CompressionMode.Decompress);
    using BinaryReader reader = new(compressed, Encoding.UTF8, leaveOpen: false);
    byte[] magic = reader.ReadBytes(Magic.Length);
    if (!magic.AsSpan().SequenceEqual(Magic))
    {
      throw new InvalidDataException("The map document magic is invalid.");
    }

    byte version = reader.ReadByte();
    if (version != FormatVersion)
    {
      throw new NotSupportedException("The map document version is unsupported.");
    }

    int width = reader.ReadInt32();
    int height = reader.ReadInt32();
    int blackEdgeWidth = reader.ReadInt32();
    uint sourceRevision = reader.ReadUInt32();
    int tileCount = reader.ReadInt32();
    int expectedCount = checked(width * height);
    if (width <= 0 || height <= 0 || blackEdgeWidth < 0 || tileCount != expectedCount)
    {
      throw new InvalidDataException("The map document dimensions are invalid.");
    }

    MapTileSnapshotValue[] tiles = new MapTileSnapshotValue[tileCount];
    for (int index = 0; index < tileCount; index++)
    {
      byte[] encoded = reader.ReadBytes(4);
      if (encoded.Length != 4)
      {
        throw new InvalidDataException("The map document ended before all tiles were read.");
      }

      tiles[index] = _codec.Decode(encoded).ToValue();
    }

    return new MapPersistenceSnapshot(
      width,
      height,
      blackEdgeWidth,
      sourceRevision,
      tiles);
  }

  private static void ReplaceDocument(string temporaryPath, string targetPath)
  {
    if (File.Exists(targetPath))
    {
      File.Replace(temporaryPath, targetPath, null);
      return;
    }

    File.Move(temporaryPath, targetPath);
  }

  private static void DeleteTemporary(string path)
  {
    if (File.Exists(path))
    {
      File.Delete(path);
    }
  }

  private static bool TryPreparePath(
    string path,
    out string fullPath,
    out MapPersistenceFailureKind failureKind)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      fullPath = string.Empty;
      failureKind = MapPersistenceFailureKind.InvalidInput;
      return false;
    }

    try
    {
      fullPath = Path.GetFullPath(path);
    }
    catch (ArgumentException)
    {
      fullPath = string.Empty;
      failureKind = MapPersistenceFailureKind.InvalidInput;
      return false;
    }

    string? directory = Path.GetDirectoryName(fullPath);
    if (directory is null || !Directory.Exists(directory))
    {
      failureKind = MapPersistenceFailureKind.DirectoryMissing;
      return false;
    }

    failureKind = MapPersistenceFailureKind.None;
    return true;
  }
}
