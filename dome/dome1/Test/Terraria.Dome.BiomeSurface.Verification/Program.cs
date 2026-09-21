using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Simulation.WorldGeneration.Systems;
using Terraria.Dome.Simulation.WorldModel;

WorldMetadata metadata = new("biome", new WorldSeed(1456), 200, 150);
WorldGrid world = new(200, 150, initializeLegacyEmptyFrames: true);
for (int y = 40; y < 43; y++)
{
  for (int x = 0; x < metadata.Width; x++)
  {
    _ = world.TrySetTile(x, y, new WorldTile(true, 1, FrameX: -1, FrameY: -1));
  }
}

BiomeSurfaceDefinition definition = new("frozen", 147, 40, depth: 3);
BiomeSurfaceComponent profile = new(definition, surfaceY: 40);
WorldGenerationStateComponent state = new(42);
_ = state.TryAdvance(WorldGenerationStage.Terrain);
List<TileChangeCommand> commands = new();
BiomeSurfaceResult result = new BiomeSurfaceSystem().AppendCommands(
  world.CreateSnapshot(metadata),
  profile,
  ref state,
  commands);
if (!result.Supported || commands.Count != metadata.Width * 3 * 2)
{
  throw new InvalidOperationException("Biome profile did not emit deterministic tile and wall commands.");
}

if (!new TileChangeCommitSystem().TryCommit(world, commands, out TileChangeCommitResult commit) ||
    commit.AppliedCount != commands.Count ||
    world.GetTile(20, 41).Type != 147 ||
    world.GetTile(20, 41).WallType != 40)
{
  throw new InvalidOperationException("Biome profile commands did not commit tile and wall state.");
}

BiomeSurfaceResult unsupported = new BiomeSurfaceSystem().AppendCommands(
  world.CreateSnapshot(metadata),
  new BiomeSurfaceComponent("unknown"),
  ref state,
  new List<TileChangeCommand>());
if (unsupported.Supported || unsupported.FailureReason is null)
{
  throw new InvalidOperationException("Unknown biome profile was accepted without a definition.");
}

Console.WriteLine("PASS: biome surface definitions emit explicit tile and wall commands");
