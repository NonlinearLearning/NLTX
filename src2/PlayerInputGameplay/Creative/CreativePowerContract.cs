namespace NLTX.PlayerInputGameplay.Creative;

public sealed class CreativePowerContract
{
  public CreativePowerContract(
    ushort powerId,
    string serverConfigName,
    CreativePowerPermissionLevel currentPermissionLevel,
    CreativePowerPermissionLevel defaultPermissionLevel)
  {
    if (string.IsNullOrWhiteSpace(serverConfigName))
    {
      throw new ArgumentException("A server configuration name is required.", nameof(serverConfigName));
    }

    PowerId = powerId;
    ServerConfigName = serverConfigName;
    CurrentPermissionLevel = currentPermissionLevel;
    DefaultPermissionLevel = defaultPermissionLevel;
  }

  public ushort PowerId { get; }

  public string ServerConfigName { get; }

  public CreativePowerPermissionLevel CurrentPermissionLevel { get; }

  public CreativePowerPermissionLevel DefaultPermissionLevel { get; }
}
