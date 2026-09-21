namespace NLTX.PlayerInputGameplay.Creative;

public sealed class PerPlayerCreativePowerDefinition
{
  public PerPlayerCreativePowerDefinition(
    ushort powerId,
    string serverConfigName,
    CreativePowerPermissionLevel currentPermissionLevel,
    CreativePowerPermissionLevel defaultPermissionLevel,
    string powerNameKey,
    CreativePowerIconLocation iconLocation,
    bool defaultToggleState,
    float sliderDefaultValue)
  {
    if (string.IsNullOrWhiteSpace(serverConfigName) || string.IsNullOrWhiteSpace(powerNameKey))
    {
      throw new ArgumentException("Power names are required.");
    }

    PowerId = powerId;
    ServerConfigName = serverConfigName;
    CurrentPermissionLevel = currentPermissionLevel;
    DefaultPermissionLevel = defaultPermissionLevel;
    PowerNameKey = powerNameKey;
    IconLocation = iconLocation;
    DefaultToggleState = defaultToggleState;
    SliderDefaultValue = sliderDefaultValue;
  }

  public ushort PowerId { get; }

  public string ServerConfigName { get; }

  public CreativePowerPermissionLevel CurrentPermissionLevel { get; private set; }

  public CreativePowerPermissionLevel DefaultPermissionLevel { get; }

  public string PowerNameKey { get; }

  public CreativePowerIconLocation IconLocation { get; }

  public bool DefaultToggleState { get; }

  public float SliderDefaultValue { get; }

  public void SetCurrentPermission(CreativePowerPermissionLevel permissionLevel)
  {
    CurrentPermissionLevel = permissionLevel;
  }
}
