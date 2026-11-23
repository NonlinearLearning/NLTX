using Terraria.Lvcs;

namespace NSSLC.Infrastructure.Lvcs;

/// <summary>
/// Reads every file under one directory as an <see cref="ILvcsUnit" />.
/// </summary>
/// <remarks>
/// This is the only place in the loading stack that touches the file system. Files are ordered by
/// full path with ordinal comparison so that diagnostics are reproducible; that order carries no
/// semantic meaning and a codec must not rely on it.
/// </remarks>
public sealed class FileLvcsSource : ILvcsSource
{
  private readonly string _directory;
  private readonly string _searchPattern;
  private readonly bool _recursive;

  public FileLvcsSource(
      LvcsFamilyId family,
      string directory,
      string searchPattern = "*",
      bool recursive = true)
  {
    if (string.IsNullOrWhiteSpace(directory))
    {
      throw new ArgumentException("A content directory is required.", nameof(directory));
    }

    if (string.IsNullOrWhiteSpace(searchPattern))
    {
      throw new ArgumentException("A search pattern is required.", nameof(searchPattern));
    }

    Family = family;
    _directory = directory;
    _searchPattern = searchPattern;
    _recursive = recursive;
  }

  public LvcsFamilyId Family { get; }

  public IEnumerable<ILvcsUnit> ReadUnits()
  {
    if (!Directory.Exists(_directory))
    {
      throw new DirectoryNotFoundException(
          $"Content directory '{_directory}' does not exist.");
    }

    SearchOption option = _recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
    string[] files = Directory
        .EnumerateFiles(_directory, _searchPattern, option)
        .OrderBy(static path => path, StringComparer.Ordinal)
        .ToArray();

    if (files.Length == 0)
    {
      throw new InvalidDataException(
          $"Content directory '{_directory}' does not contain any '{_searchPattern}' files.");
    }

    foreach (string file in files)
    {
      string location = Path.GetRelativePath(_directory, file).Replace('\\', '/');
      yield return new LvcsUnit(location, File.ReadAllBytes(file));
    }
  }
}
