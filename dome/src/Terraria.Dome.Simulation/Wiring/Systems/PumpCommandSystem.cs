using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.Wiring.Components;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public sealed class PumpCommandSystem
{
  public LiquidTransferCommand? CreateCommand(
    PumpComponent pump,
    MechanismActivationCommand activation,
    long sequence,
    LiquidType type)
  {
    if (activation.MechanismId != pump.MechanismId || sequence < 0 ||
        activation.Kind is not (MechanismActivationKind.Activate or MechanismActivationKind.Open))
    {
      return null;
    }

    return new LiquidTransferCommand(
      sequence,
      pump.InputX,
      pump.InputY,
      pump.OutputX,
      pump.OutputY,
      pump.Capacity,
      (byte)type);
  }
}
