namespace NLTX.PlayerInputGameplay.Creative;

public sealed class SharedCreativePowerDefinition
{
  public SharedCreativePowerDefinition(
    ushort powerId,
    string serverConfigName,
    CreativePowerPermissionLevel currentPermissionLevel,
    CreativePowerPermissionLevel defaultPermissionLevel,
    string powerNameKey,
    string descriptionKey,
    CreativePowerIconLocation iconLocation,
    bool syncToJoiningPlayers)
  {
    if (string.IsNullOrWhiteSpace(serverConfigName) || string.IsNullOrWhiteSpace(powerNameKey) || string.IsNullOrWhiteSpace(descriptionKey))
    {
      throw new ArgumentException("Power metadata is required.");
    }

    PowerId = powerId;
    ServerConfigName = serverConfigName;
    CurrentPermissionLevel = currentPermissionLevel;
    DefaultPermissionLevel = defaultPermissionLevel;
    PowerNameKey = powerNameKey;
    DescriptionKey = descriptionKey;
    IconLocation = iconLocation;
    SyncToJoiningPlayers = syncToJoiningPlayers;
  }

  public ushort PowerId { get; }

  public string ServerConfigName { get; }

  public CreativePowerPermissionLevel CurrentPermissionLevel { get; private set; }

  public CreativePowerPermissionLevel DefaultPermissionLevel { get; }

  public string PowerNameKey { get; }

  public string DescriptionKey { get; }

  public CreativePowerIconLocation IconLocation { get; }

  public bool SyncToJoiningPlayers { get; }

  public void SetCurrentPermission(CreativePowerPermissionLevel permissionLevel)
  {
    CurrentPermissionLevel = permissionLevel;
  }
}
