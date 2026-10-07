using System;
using System.Collections.Generic;

namespace Terraria.WorldInteraction.Wiring;

public static class PumpTransferPolicy
{
  public const int MaxPumpCount = 20;

  public static PumpTransferCommandBatch CreateCommands(
    PumpTransferScratchComponent scratch,
    byte wireColor)
  {
    ArgumentNullException.ThrowIfNull(scratch);
    if (wireColor is < 1 or > 4)
    {
      return PumpTransferCommandBatch.Rejected("wire-color");
    }

    int inputCount = scratch.InputPumpCount;
    int outputCount = scratch.OutputPumpCount;
    if (inputCount is < 0 or > MaxPumpCount ||
        outputCount is < 0 or > MaxPumpCount)
    {
      return PumpTransferCommandBatch.Rejected("pump-count");
    }

    if (inputCount == 0)
    {
      return PumpTransferCommandBatch.Empty("missing-input-pump");
    }

    if (outputCount == 0)
    {
      return PumpTransferCommandBatch.Empty("missing-output-pump");
    }

    List<PumpTransferCommand> commands = new(inputCount * outputCount);
    for (int inputIndex = 0; inputIndex < inputCount; inputIndex++)
    {
      for (int outputIndex = 0; outputIndex < outputCount; outputIndex++)
      {
        if (!PumpTransferCommand.TryCreate(
          scratch.InputPumpPositions[inputIndex],
          scratch.OutputPumpPositions[outputIndex],
          inputIndex,
          outputIndex,
          wireColor,
          out PumpTransferCommand command))
        {
          return PumpTransferCommandBatch.Rejected(
            "invalid-pump-pair");
        }

        commands.Add(command);
      }
    }

    return PumpTransferCommandBatch.Accepted(commands.AsReadOnly());
  }
}
