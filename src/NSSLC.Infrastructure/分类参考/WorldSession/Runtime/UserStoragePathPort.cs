namespace Terraria.WorldSession.Runtime;

public sealed class UserStoragePathPort
{
  public string? SavePath { get; private set; }

  public void Configure(string savePath)
  {
    SavePath = string.IsNullOrWhiteSpace(savePath)
      ? throw new ArgumentException("A save path is required.", nameof(savePath))
      : savePath;
  }
}
