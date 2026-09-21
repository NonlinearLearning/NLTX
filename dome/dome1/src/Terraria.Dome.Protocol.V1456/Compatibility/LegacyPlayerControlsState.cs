using Terraria.Dome.Simulation;

namespace Terraria.Dome.Protocol.V1456.Compatibility;

public sealed record LegacyPlayerControlsState(
  byte PlayerSlot,
  byte ControlFlags,
  byte SecondaryFlags,
  byte TertiaryFlags,
  byte QuaternaryFlags,
  byte SelectedItem,
  SimulationVector Position,
  SimulationVector? Velocity,
  ushort? MountType,
  SimulationVector? ReturnOrigin,
  SimulationVector? ReturnHome,
  SimulationVector? CameraTarget);
