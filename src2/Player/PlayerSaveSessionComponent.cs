namespace Terraria.NonAuthoritative.Player;

public sealed class PlayerSaveSessionComponent
{
  public PlayerSaveSessionComponent(string name, string path, bool isCloudSave)
  {
    Name = string.IsNullOrWhiteSpace(name)
      ? throw new ArgumentException("A player name is required.", nameof(name))
      : name;
    Path = string.IsNullOrWhiteSpace(path)
      ? throw new ArgumentException("A player save path is required.", nameof(path))
      : path;
    IsCloudSave = isCloudSave;
    PlayTime = TimeSpan.Zero;
    IsTimerActive = false;
  }

  public string Name { get; }

  public string Path { get; }

  public bool IsCloudSave { get; }

  public TimeSpan PlayTime { get; private set; }

  public bool IsTimerActive { get; private set; }

  public void SetPlayTime(TimeSpan playTime)
  {
    if (playTime < TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(playTime), "Play time cannot be negative.");
    }

    PlayTime = playTime;
  }

  internal void AddPlayTime(TimeSpan elapsed)
  {
    if (elapsed < TimeSpan.Zero)
    {
      throw new ArgumentOutOfRangeException(nameof(elapsed), "Elapsed time cannot be negative.");
    }

    PlayTime = PlayTime.Add(elapsed);
  }

  internal void SetTimerActive(bool isActive)
  {
    IsTimerActive = isActive;
  }

  public PlayerSaveSessionSnapshot CreateSnapshot()
  {
    return new PlayerSaveSessionSnapshot(Name, Path, IsCloudSave, PlayTime);
  }
}
