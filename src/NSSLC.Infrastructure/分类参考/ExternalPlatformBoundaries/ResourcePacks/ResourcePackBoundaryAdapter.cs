using System.IO.Compression;
using System.Text.Json;

namespace Terraria.ExternalPlatformBoundaries.ResourcePacks;

public sealed class ResourcePackBoundaryAdapter : IDisposable
{
  public const string IconFileName = "icon.png";

  public const string PackFileName = "pack.json";

  private readonly IServiceProvider? _services;
  private readonly string _fullPath;
  private readonly ResourcePackBranding _branding;
  private ZipArchive? _zipFile;
  private object? _icon;
  private ResourcePackMetadataSnapshot? _metadata;
  private bool _disposed;

  public bool HasCachedIcon => _icon is not null;

  public ResourcePackBoundaryAdapter(
    string path,
    ResourcePackBranding branding,
    IServiceProvider? services = null)
  {
    ArgumentNullException.ThrowIfNull(path);
    _fullPath = Path.GetFullPath(path);
    _branding = branding;
    _services = services;
  }

  public ResourcePackLoadResult Load()
  {
    if (_disposed)
    {
      return ResourcePackLoadResult.Failure(
        ResourcePackLoadStatus.AlreadyDisposed,
        "The resource-pack adapter has been disposed.");
    }

    if (_metadata is not null)
    {
      return ResourcePackLoadResult.Loaded(_metadata);
    }

    bool isCompressed;
    if (File.Exists(_fullPath))
    {
      isCompressed = true;
    }
    else if (Directory.Exists(_fullPath))
    {
      isCompressed = false;
    }
    else
    {
      return ResourcePackLoadResult.Failure(
        ResourcePackLoadStatus.MissingPath,
        "The resource-pack path does not exist.");
    }

    try
    {
      using Stream? manifestStream = OpenManifestStream(isCompressed);
      if (manifestStream is null)
      {
        return ResourcePackLoadResult.Failure(
          ResourcePackLoadStatus.MissingManifest,
          "The resource pack does not contain pack.json.");
      }

      using JsonDocument document = JsonDocument.Parse(manifestStream);
      if (!TryReadManifest(document.RootElement, out string name, out string author, out int version))
      {
        return ResourcePackLoadResult.Failure(
          ResourcePackLoadStatus.InvalidManifest,
          "The resource-pack manifest is missing required fields.");
      }

      bool hasIcon = HasEntry(isCompressed, IconFileName);
      _icon = hasIcon ? ReadEntryBytes(isCompressed, IconFileName) : null;
      _metadata = new ResourcePackMetadataSnapshot(
        _fullPath,
        Path.GetFileName(_fullPath),
        isCompressed,
        _branding,
        name,
        author,
        version,
        hasIcon);
      GC.KeepAlive(_services);
      return ResourcePackLoadResult.Loaded(_metadata);
    }
    catch (JsonException exception)
    {
      DisposeZip();
      return ResourcePackLoadResult.Failure(
        ResourcePackLoadStatus.InvalidManifest,
        exception.Message);
    }
    catch (IOException exception)
    {
      DisposeZip();
      return ResourcePackLoadResult.Failure(ResourcePackLoadStatus.Failed, exception.Message);
    }
    catch (UnauthorizedAccessException exception)
    {
      DisposeZip();
      return ResourcePackLoadResult.Failure(ResourcePackLoadStatus.Failed, exception.Message);
    }
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    _icon = null;
    DisposeZip();
    GC.SuppressFinalize(this);
  }

  private Stream? OpenManifestStream(bool isCompressed)
  {
    if (isCompressed)
    {
      _zipFile ??= ZipFile.OpenRead(_fullPath);
      ZipArchiveEntry? entry = _zipFile.GetEntry(PackFileName);
      return entry?.Open();
    }

    string manifestPath = Path.Combine(_fullPath, PackFileName);
    return File.Exists(manifestPath) ? File.OpenRead(manifestPath) : null;
  }

  private bool HasEntry(bool isCompressed, string name)
  {
    if (isCompressed)
    {
      return _zipFile?.GetEntry(name) is not null;
    }

    return File.Exists(Path.Combine(_fullPath, name));
  }

  private byte[] ReadEntryBytes(bool isCompressed, string name)
  {
    using Stream? stream = isCompressed
      ? _zipFile?.GetEntry(name)?.Open()
      : File.OpenRead(Path.Combine(_fullPath, name));
    if (stream is null)
    {
      return Array.Empty<byte>();
    }

    using var buffer = new MemoryStream();
    stream.CopyTo(buffer);
    return buffer.ToArray();
  }

  private static bool TryReadManifest(
    JsonElement root,
    out string name,
    out string author,
    out int version)
  {
    name = string.Empty;
    author = string.Empty;
    version = 0;
    if (root.ValueKind != JsonValueKind.Object ||
      !TryGetString(root, "name", out name) ||
      !TryGetString(root, "author", out author) ||
      !TryGetInt32(root, "version", out version))
    {
      return false;
    }

    return !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(author);
  }

  private static bool TryGetString(JsonElement root, string name, out string value)
  {
    foreach (JsonProperty property in root.EnumerateObject())
    {
      if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase) &&
        property.Value.ValueKind == JsonValueKind.String)
      {
        value = property.Value.GetString() ?? string.Empty;
        return true;
      }
    }

    value = string.Empty;
    return false;
  }

  private static bool TryGetInt32(JsonElement root, string name, out int value)
  {
    foreach (JsonProperty property in root.EnumerateObject())
    {
      if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase) &&
        property.Value.TryGetInt32(out value))
      {
        return true;
      }
    }

    value = 0;
    return false;
  }

  private void DisposeZip()
  {
    _zipFile?.Dispose();
    _zipFile = null;
  }
}
