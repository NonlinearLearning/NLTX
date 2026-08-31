using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonStepBasedRoomSettings stepBased = new(5, 3);
DungeonLegacyRoomSettings settings = new(stepBased, isEntranceRoom: true);
if (!settings.IsEntranceRoom ||
    settings.StepBasedSettings != stepBased ||
    settings.GetBoundingRadius() != stepBased.GetBoundingRadius())
{
  throw new InvalidOperationException("Legacy dungeon room settings diverged.");
}

Console.WriteLine("PASS: Legacy dungeon room settings contract");
