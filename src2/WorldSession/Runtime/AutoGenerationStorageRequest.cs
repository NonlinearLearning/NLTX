namespace Terraria.WorldSession.Runtime;

public sealed class AutoGenerationStorageRequest
{
  public string? FileLocation { get; private set; }

  public void Set(string fileLocation)
  {
    FileLocation = string.IsNullOrWhiteSpace(fileLocation)
      ? throw new ArgumentException("A generation file location is required.", nameof(fileLocation))
      : fileLocation;
  }

  public void Clear()
  {
    FileLocation = null;
  }
}
