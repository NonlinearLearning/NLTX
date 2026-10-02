namespace Terraria.NonAuthoritative.Player;

public readonly record struct PlayerSaveCommand
{
  public PlayerSaveCommand(
    string path,
    bool isCloudSave,
    PlayerSavePolicy policy)
  {
    Path = string.IsNullOrWhiteSpace(path)
      ? throw new ArgumentException("A player save path is required.", nameof(path))
      : path;
    IsCloudSave = isCloudSave;
    Policy = policy;
  }

  public string Path { get; }

  public bool IsCloudSave { get; }

  public PlayerSavePolicy Policy { get; }
}
