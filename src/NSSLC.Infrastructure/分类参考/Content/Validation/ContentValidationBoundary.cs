namespace Terraria.NonAuthoritative.ContentDefinitions;

public readonly record struct TextureSize
{
  public TextureSize(int width, int height)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    Width = width;
    Height = height;
  }

  public int Width { get; }

  public int Height { get; }
}

public readonly record struct TextureMetaData
{
  public TextureMetaData(int width, int height)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    Width = width;
    Height = height;
  }

  public int Width { get; }

  public int Height { get; }
}

public readonly record struct ContentValidationResult(
  bool IsAccepted,
  string? FailureReason);

public sealed class ContentValidationBoundary
{
  private readonly HashSet<string> _allowedPaths = new(StringComparer.Ordinal);
  private readonly Dictionary<string, TextureMetaData> _metadata =
    new(StringComparer.Ordinal);

  public bool IsLoaded { get; internal set; }

  internal void AllowPath(string contentPath)
  {
    _allowedPaths.Add(ValidatePath(contentPath));
    IsLoaded = true;
  }

  internal void SetMetadata(string contentPath, TextureMetaData metadata)
  {
    _metadata[ValidatePath(contentPath)] = metadata;
    IsLoaded = true;
  }

  internal bool IsAllowed(string contentPath)
  {
    return _allowedPaths.Count == 0 || _allowedPaths.Contains(contentPath);
  }

  internal bool TryGetMetadata(string contentPath, out TextureMetaData metadata)
  {
    return _metadata.TryGetValue(contentPath, out metadata);
  }

  internal void Clear()
  {
    _allowedPaths.Clear();
    _metadata.Clear();
    IsLoaded = false;
  }

  private static string ValidatePath(string contentPath)
  {
    if (string.IsNullOrWhiteSpace(contentPath))
    {
      throw new ArgumentException("Content paths must be non-empty.", nameof(contentPath));
    }

    return contentPath.Replace('/', '\\');
  }
}

public static class ContentValidationLoadSystem
{
  public static void Allow(ContentValidationBoundary boundary, string contentPath)
  {
    ArgumentNullException.ThrowIfNull(boundary);
    boundary.AllowPath(contentPath);
  }

  public static void SetMetadata(
    ContentValidationBoundary boundary,
    string contentPath,
    TextureMetaData metadata)
  {
    ArgumentNullException.ThrowIfNull(boundary);
    boundary.SetMetadata(contentPath, metadata);
  }

  public static void Clear(ContentValidationBoundary boundary)
  {
    ArgumentNullException.ThrowIfNull(boundary);
    boundary.Clear();
  }
}

public static class ContentValidationQuery
{
  public static ContentValidationResult Validate(
    ContentValidationBoundary boundary,
    string contentPath,
    TextureSize actualSize)
  {
    ArgumentNullException.ThrowIfNull(boundary);
    string normalizedPath = contentPath.Replace('/', '\\');
    if (!boundary.IsAllowed(normalizedPath))
    {
      return new ContentValidationResult(false, "path-not-allowed");
    }

    if (boundary.TryGetMetadata(normalizedPath, out TextureMetaData expected) &&
      (expected.Width != actualSize.Width || expected.Height != actualSize.Height))
    {
      return new ContentValidationResult(false, "size-mismatch");
    }

    return new ContentValidationResult(true, null);
  }
}
