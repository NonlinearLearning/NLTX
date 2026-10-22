namespace MinimalImplementation.Modules.Creative;

public enum CreativePowerValueKind : byte
{
  SharedToggle,
  Button,
  PerPlayerToggleSync,
  PerPlayerToggle,
  SharedSlider,
  PerPlayerSlider,
  NoPayload
}

public sealed record CreativePowerWireMessage
{
  private readonly bool[]? _playerStates;

  public ushort PowerId { get; }
  public CreativePowerValueKind ValueKind { get; }
  public bool ToggleValue { get; }
  public byte PlayerId { get; }
  public float SliderValue { get; }
  public IReadOnlyList<bool>? PlayerStates => _playerStates is null
    ? null
    : Array.AsReadOnly((bool[])_playerStates.Clone());

  private CreativePowerWireMessage(
    ushort powerId,
    CreativePowerValueKind valueKind,
    bool toggleValue,
    byte playerId,
    float sliderValue,
    bool[]? playerStates)
  {
    PowerId = powerId;
    ValueKind = valueKind;
    ToggleValue = toggleValue;
    PlayerId = playerId;
    SliderValue = sliderValue;
    _playerStates = playerStates is null ? null : (bool[])playerStates.Clone();
  }

  public static CreativePowerWireMessage SharedToggle(ushort powerId, bool value) =>
    new(powerId, CreativePowerValueKind.SharedToggle, value, 0, 0, null);

  public static CreativePowerWireMessage Button(ushort powerId) =>
    new(powerId, CreativePowerValueKind.Button, false, 0, 0, null);

  public static CreativePowerWireMessage PerPlayerToggleSync(ushort powerId, bool[] states) =>
    new(powerId, CreativePowerValueKind.PerPlayerToggleSync, false, 0, 0, states);

  public static CreativePowerWireMessage PerPlayerToggle(
    ushort powerId,
    byte playerId,
    bool value) =>
    new(powerId, CreativePowerValueKind.PerPlayerToggle, value, playerId, 0, null);

  public static CreativePowerWireMessage SharedSlider(ushort powerId, float value) =>
    new(powerId, CreativePowerValueKind.SharedSlider, false, 0, value, null);

  public static CreativePowerWireMessage PerPlayerSlider(
    ushort powerId,
    byte playerId,
    float value) =>
    new(powerId, CreativePowerValueKind.PerPlayerSlider, false, playerId, value, null);

  public static CreativePowerWireMessage NoPayload(ushort powerId) =>
    new(powerId, CreativePowerValueKind.NoPayload, false, 0, 0, null);

  public bool[] CopyPlayerStates()
  {
    return _playerStates is null
      ? throw new InvalidOperationException("This creative power message has no player-state table.")
      : (bool[])_playerStates.Clone();
  }
}
