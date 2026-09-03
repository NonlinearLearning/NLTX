using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.World;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Simulation.WorldGeneration.Systems;
using Terraria.Dome.Simulation.WorldModel;

const int width = 400;
const int height = 300;
WorldMetadata metadata = new("simple-structure", new WorldSeed(1456), width, height);
WorldGrid world = new(width, height);
TileProtectionComponent protection = new(200, 80, 0, 0);
SimpleStructurePattern pattern = SimpleStructurePattern.Parse(new[] { "0x", "10" });
IReadOnlyList<StructureDefinition> actions = new[]
{
  new StructureDefinition("tile-and-wall", 1, 1, 5, 2, allowReplaceExisting: false),
  new StructureDefinition("tile-only", 1, 1, 6, 0, allowReplaceExisting: false)
};
List<TileChangeCommand> commands = new();
WorldGenerationStateComponent state = new(50);
if (!new SimpleStructurePlacementSystem().TryAppendCommands(
      world.CreateSnapshot(metadata),
      pattern.Mirror(horizontalMirror: true, verticalMirror: false),
      actions,
      originX: 50,
      originY: 50,
      protection,
      ref state,
      commands))
{
  throw new InvalidOperationException("The valid pattern was rejected.");
}

if (commands.Count != 5 ||
    commands.Any(command => command.X is not (49 or 50)) ||
    commands.Any(command => command.Y is not (50 or 51)) ||
    state.Stage < WorldGenerationStage.Structure)
{
  throw new InvalidOperationException("Pattern command projection was not deterministic.");
}

if (commands.Any(command => command.Source != "tile-and-wall" &&
    command.Source != "tile-only"))
{
  throw new InvalidOperationException("Pattern commands did not preserve structure provenance.");
}

if (SimpleStructurePattern.Parse(new[] { "0" }).GetActionIndex(0, 0) != 0 ||
    SimpleStructurePattern.Parse(new[] { "x" }).GetActionIndex(0, 0) != -1)
{
  throw new InvalidOperationException("Pattern digit/hole parsing failed.");
}

if (new SimpleStructurePlacementSystem().TryAppendCommands(
      world.CreateSnapshot(metadata),
      SimpleStructurePattern.Parse(new[] { "0" }),
      new[] { new StructureDefinition("wide", 2, 1, 5, 0, allowReplaceExisting: false) },
      originX: 60,
      originY: 60,
      protection,
      ref state,
      new List<TileChangeCommand>()))
{
  throw new InvalidOperationException("A non-1x1 action was accepted.");
}

Console.WriteLine("PASS: SimpleStructure typed pattern contract");
