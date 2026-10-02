namespace NLTX.PlayerInputGameplay.Input;

public sealed class PlayerInputProfileConfigurationComponent
{
  private readonly Dictionary<InputMode, KeyBindingConfigurationComponent> _inputModes = new();

  public PlayerInputProfileConfigurationComponent()
  {
    foreach (var mode in Enum.GetValues<InputMode>())
    {
      _inputModes.Add(mode, new KeyBindingConfigurationComponent());
    }
  }

  public string Name { get; private set; } = string.Empty;

  public bool AllowEditing { get; private set; } = true;

  public int HotbarRadialHoldTimeRequired { get; private set; } = 16;

  public float TriggersDeadzone { get; private set; } = 0.3f;

  public float InterfaceDeadzoneX { get; private set; } = 0.2f;

  public float LeftThumbstickDeadzoneX { get; private set; } = 0.25f;

  public float LeftThumbstickDeadzoneY { get; private set; } = 0.4f;

  public float RightThumbstickDeadzoneX { get; private set; }

  public float RightThumbstickDeadzoneY { get; private set; }

  public bool LeftThumbstickInvertX { get; private set; }

  public bool LeftThumbstickInvertY { get; private set; }

  public bool RightThumbstickInvertX { get; private set; }

  public bool RightThumbstickInvertY { get; private set; }

  public int InventoryMoveCooldown { get; private set; } = 6;

  public IReadOnlyDictionary<InputMode, KeyBindingConfigurationComponent> InputModes => _inputModes;

  public void Configure(
    string name,
    bool allowEditing,
    int hotbarRadialHoldTimeRequired,
    float triggersDeadzone,
    float interfaceDeadzoneX,
    float leftThumbstickDeadzoneX,
    float leftThumbstickDeadzoneY,
    float rightThumbstickDeadzoneX,
    float rightThumbstickDeadzoneY,
    bool leftThumbstickInvertX,
    bool leftThumbstickInvertY,
    bool rightThumbstickInvertX,
    bool rightThumbstickInvertY,
    int inventoryMoveCooldown)
  {
    if (string.IsNullOrWhiteSpace(name) || hotbarRadialHoldTimeRequired < 0 || inventoryMoveCooldown < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(name));
    }

    var deadzones = new[]
    {
      triggersDeadzone,
      interfaceDeadzoneX,
      leftThumbstickDeadzoneX,
      leftThumbstickDeadzoneY,
      rightThumbstickDeadzoneX,
      rightThumbstickDeadzoneY
    };
    if (deadzones.Any(value => float.IsNaN(value) || float.IsInfinity(value) || value < 0f))
    {
      throw new ArgumentOutOfRangeException(nameof(triggersDeadzone));
    }

    Name = name;
    AllowEditing = allowEditing;
    HotbarRadialHoldTimeRequired = hotbarRadialHoldTimeRequired;
    TriggersDeadzone = triggersDeadzone;
    InterfaceDeadzoneX = interfaceDeadzoneX;
    LeftThumbstickDeadzoneX = leftThumbstickDeadzoneX;
    LeftThumbstickDeadzoneY = leftThumbstickDeadzoneY;
    RightThumbstickDeadzoneX = rightThumbstickDeadzoneX;
    RightThumbstickDeadzoneY = rightThumbstickDeadzoneY;
    LeftThumbstickInvertX = leftThumbstickInvertX;
    LeftThumbstickInvertY = leftThumbstickInvertY;
    RightThumbstickInvertX = rightThumbstickInvertX;
    RightThumbstickInvertY = rightThumbstickInvertY;
    InventoryMoveCooldown = inventoryMoveCooldown;
  }
}
