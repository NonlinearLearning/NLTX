namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class MemoCommandFileAdapter
{
  public MemoCommandFileAdapter(string path)
  {
    Path = path ?? throw new ArgumentNullException(nameof(path));
  }

  public string Path { get; }

  public string? Read()
  {
    return File.Exists(Path) ? File.ReadAllText(Path) : null;
  }

  public void Write(string content)
  {
    File.WriteAllText(Path, content ?? string.Empty);
  }
}
