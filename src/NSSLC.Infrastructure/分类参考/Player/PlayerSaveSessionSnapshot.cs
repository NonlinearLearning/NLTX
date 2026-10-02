namespace Terraria.NonAuthoritative.Player;

public readonly record struct PlayerSaveSessionSnapshot
{
  public PlayerSaveSessionSnapshot(
    string name,
    string path,
    bool isCloudSave,
    TimeSpan playTime)
  {
    Name = string.IsNullOrWhiteSpace(name)
      ? throw new ArgumentException("A player name is required.", nameof(name))
      : name;
    Path = string.IsNullOrWhiteSpace(path)
      ? throw new ArgumentException("A player save path is required.", nameof(path))
      : path;
    if (playTime < TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(playTime), "Play time cannot be negative.");
    }

    IsCloudSave = isCloudSave;
    PlayTime = playTime;
  }

  public string Name { get; }

  public string Path { get; }

  public bool IsCloudSave { get; }

  public TimeSpan PlayTime { get; }
}
