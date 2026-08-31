using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonLegacyRoomSettings
{
  public DungeonLegacyRoomSettings(
    DungeonStepBasedRoomSettings stepBasedSettings,
    bool isEntranceRoom)
  {
    if (stepBasedSettings.OverrideStrength < 0 || stepBasedSettings.OverrideSteps < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(stepBasedSettings));
    }

    StepBasedSettings = stepBasedSettings;
    IsEntranceRoom = isEntranceRoom;
  }

  public DungeonStepBasedRoomSettings StepBasedSettings { get; }

  public bool IsEntranceRoom { get; }

  public int GetBoundingRadius()
  {
    return StepBasedSettings.GetBoundingRadius();
  }
}
