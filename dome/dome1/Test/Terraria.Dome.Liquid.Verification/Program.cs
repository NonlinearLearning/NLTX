using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.Liquid.Definitions;
using Terraria.Dome.Simulation.Liquid.Systems;
using Terraria.Dome.Simulation.WorldModel.Definitions;
using Terraria.Dome.Simulation.WorldModel;
using SimulationLiquidMerge = Terraria.Dome.Simulation.Liquid.Components.LiquidMergeComponent;
using PipelineLiquidSourceComponent = Terraria.Dome.Simulation.Liquid.Components.LiquidSourceComponent;
using DomeSimulation = Terraria.Dome.Simulation.DomeSimulation;

TileDefinitionRegistry tileDefinitions = TileDefinitionRegistry.CreateVersion4Base();
if (tileDefinitions.Definitions.Count != 753 ||
    tileDefinitions.Definitions.Count(definition => definition.WaterDestroysTile) != 10 ||
    tileDefinitions.Definitions.Count(definition => definition.LavaDestroysTile) != 267 ||
    !tileDefinitions.TryGet(7, out TileDefinition solidDefinition) ||
    !solidDefinition.BlocksLiquid || solidDefinition.IsPlatform ||
    !tileDefinitions.TryGet(275, out TileDefinition platformDefinition) ||
    !platformDefinition.IsPlatform || platformDefinition.BlocksLiquid ||
    !tileDefinitions.TryGet(4, out TileDefinition waterDeathDefinition) ||
    !waterDeathDefinition.WaterDestroysTile ||
    waterDeathDefinition.LavaDestroysTile ||
    !tileDefinitions.TryGet(3, out TileDefinition lavaDeathDefinition) ||
    lavaDeathDefinition.WaterDestroysTile ||
    !lavaDeathDefinition.LavaDestroysTile ||
    !tileDefinitions.TryGet(435, out TileDefinition ropePlatformLavaDefinition) ||
    !ropePlatformLavaDefinition.LavaDestroysTile ||
    !tileDefinitions.TryGet(439, out TileDefinition finalRopePlatformLavaDefinition) ||
    !finalRopePlatformLavaDefinition.LavaDestroysTile ||
    !tileDefinitions.TryGet(7, out TileDefinition ordinaryDefinition) ||
    ordinaryDefinition.WaterDestroysTile || ordinaryDefinition.LavaDestroysTile ||
    !tileDefinitions.TryGet(546, out TileDefinition boulderDefinition) ||
    !boulderDefinition.BlocksLiquid ||
    tileDefinitions.TryGet(-1, out _) || tileDefinitions.TryGet(753, out _))
{
  throw new InvalidOperationException(
    "The source-derived Version4 Tile definition registry was incomplete or incorrect.");
}

Console.WriteLine("PASS: source-derived Version4 Tile definitions cover all 753 IDs");

WorldGrid contactWorld = new(400, 300);
_ = contactWorld.TrySetTile(20, 20, new WorldTile(IsActive: true, Type: 215));
_ = contactWorld.TrySetLiquid(20, 20, 128, (byte)LiquidType.Water);
LiquidUpdateQueueComponent contactQueue = new(maximumLength: 8);
LiquidWorldStateComponent contactState = new(maximumQueueLength: 8, tickBudget: 8);
LiquidInputSystem contactInput = new();
if (!contactInput.TryAccept(
      contactWorld,
      contactQueue,
      contactState,
      new PipelineLiquidSourceComponent(20, 20, 128, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Liquid contact fixture could not be seeded.");
}

LiquidPropagationResult contactResult = new LiquidPropagationSystem().Advance(
  contactWorld,
  contactQueue,
  contactState,
  firstSequence: 10);
if (contactResult.TileCommands.Count != 1 ||
    contactResult.TileCommands[0].Kind != TileChangeKind.Kill ||
    contactResult.TileCommands[0].X != 20 || contactResult.TileCommands[0].Y != 20 ||
    !contactWorld.GetTile(20, 20).IsActive)
{
  throw new InvalidOperationException(
    "Water contact did not produce a deferred Tile kill command for an active death Tile.");
}

WorldGrid inactiveContactWorld = new(400, 300);
_ = inactiveContactWorld.TrySetTile(21, 20, new WorldTile(IsActive: false, Type: 215));
_ = inactiveContactWorld.TrySetLiquid(21, 20, 128, (byte)LiquidType.Water);
LiquidUpdateQueueComponent inactiveContactQueue = new(maximumLength: 8);
LiquidWorldStateComponent inactiveContactState = new(maximumQueueLength: 8, tickBudget: 8);
if (!contactInput.TryAccept(
      inactiveContactWorld,
      inactiveContactQueue,
      inactiveContactState,
      new PipelineLiquidSourceComponent(21, 20, 128, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Inactive Liquid contact fixture could not be seeded.");
}

LiquidPropagationResult inactiveContactResult = new LiquidPropagationSystem().Advance(
  inactiveContactWorld,
  inactiveContactQueue,
  inactiveContactState,
  firstSequence: 10);
if (inactiveContactResult.TileCommands.Count != 0)
{
  throw new InvalidOperationException("Inactive Tiles must not receive Liquid contact kill commands.");
}

foreach ((LiquidType liquidType, ushort tileType) in new[]
         {
           (LiquidType.Water, (ushort)215),
           (LiquidType.Honey, (ushort)215),
           (LiquidType.Shimmer, (ushort)215),
           (LiquidType.Lava, (ushort)3)
         })
{
  using DomeSimulation contactSimulation = new(new WorldGrid(400, 300));
  _ = contactSimulation.WorldGrid.TrySetTile(
    30,
    30,
    new WorldTile(IsActive: true, Type: tileType));
  _ = contactSimulation.WorldGrid.TrySetLiquid(30, 30, 128, (byte)liquidType);
  if (!contactSimulation.TryQueueLiquidSource(
        new PipelineLiquidSourceComponent(30, 30, 128, liquidType, 1)))
  {
    throw new InvalidOperationException("Runtime Liquid contact fixture could not be seeded.");
  }

  contactSimulation.Tick(new SimulationInputBatch([]));
  WorldTile destroyedContactTile = contactSimulation.WorldGrid.GetTile(30, 30);
  if (destroyedContactTile.IsActive || destroyedContactTile.LiquidAmount == 0 ||
      destroyedContactTile.LiquidType != (byte)liquidType)
  {
    throw new InvalidOperationException(
      "Liquid contact Tile kill did not preserve the source liquid through Tile commit.");
  }
}

using DomeSimulation nonDeathSimulation = new(new WorldGrid(400, 300));
_ = nonDeathSimulation.WorldGrid.TrySetTile(30, 30, new WorldTile(IsActive: true, Type: 7));
_ = nonDeathSimulation.WorldGrid.TrySetLiquid(30, 30, 128, (byte)LiquidType.Water);
if (!nonDeathSimulation.TryQueueLiquidSource(
      new PipelineLiquidSourceComponent(30, 30, 128, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Non-death Liquid fixture could not be seeded.");
}

nonDeathSimulation.Tick(new SimulationInputBatch([]));
if (!nonDeathSimulation.WorldGrid.GetTile(30, 30).IsActive)
{
  throw new InvalidOperationException("Water contact destroyed a Tile outside the global death table.");
}

Console.WriteLine("PASS: global Liquid death tables produce deferred Tile commits");

TileObjectLiquidRuleRegistry objectRules = new([
  new TileObjectLiquidRule(215, LiquidType.Water, 2, 2, 0, 35, true)
]);
if (objectRules.Rules.Count != 1 || objectRules.Rules[0].TileType != 215)
{
  throw new InvalidOperationException("Tile-object Liquid rules did not preserve registration order.");
}

try
{
  ((IList<TileObjectLiquidRule>)objectRules.Rules).Clear();
  throw new InvalidOperationException("Tile-object Liquid rules projection was mutable.");
}
catch (NotSupportedException)
{
}
WorldGrid objectRuleWorld = new(400, 300);
for (int row = 0; row < 2; row++)
{
  for (int column = 0; column < 2; column++)
  {
    _ = objectRuleWorld.TrySetTile(
      50 + column,
      50 + row,
      new WorldTile(true, 215, FrameX: (short)(column * 18), FrameY: (short)(row * 18)));
  }
}

_ = objectRuleWorld.TrySetLiquid(51, 51, 128, (byte)LiquidType.Water);
LiquidUpdateQueueComponent objectRuleQueue = new(maximumLength: 8);
LiquidWorldStateComponent objectRuleState = new(maximumQueueLength: 8, tickBudget: 8);
if (!new LiquidInputSystem().TryAccept(
      objectRuleWorld,
      objectRuleQueue,
      objectRuleState,
      new PipelineLiquidSourceComponent(51, 51, 128, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Object-rule Liquid fixture could not be seeded.");
}

LiquidPropagationResult objectRuleResult = new LiquidPropagationSystem(
  tileObjectRules: objectRules).Advance(objectRuleWorld, objectRuleQueue, objectRuleState, 10);
if (objectRuleResult.TileCommands.Count != 4 || objectRuleResult.TileCommands[0] !=
    new TileChangeCommand(1, 50, 50, TileChangeKind.Kill, 0, PreserveLiquid: true) ||
    objectRuleResult.TileCommands[3] !=
    new TileChangeCommand(4, 51, 51, TileChangeKind.Kill, 0, PreserveLiquid: true))
{
  throw new InvalidOperationException(
    "TileObject Liquid rules did not override globally-derived contact destruction by footprint.");
}

WorldGrid unmatchedObjectRuleWorld = new(400, 300);
_ = unmatchedObjectRuleWorld.TrySetTile(
  60,
  60,
  new WorldTile(true, 215, FrameX: 36));
_ = unmatchedObjectRuleWorld.TrySetLiquid(60, 60, 128, (byte)LiquidType.Water);
LiquidUpdateQueueComponent unmatchedObjectRuleQueue = new(maximumLength: 8);
LiquidWorldStateComponent unmatchedObjectRuleState = new(maximumQueueLength: 8, tickBudget: 8);
_ = new LiquidInputSystem().TryAccept(
  unmatchedObjectRuleWorld,
  unmatchedObjectRuleQueue,
  unmatchedObjectRuleState,
  new PipelineLiquidSourceComponent(60, 60, 128, LiquidType.Water, 1));
if (new LiquidPropagationSystem(tileObjectRules: objectRules).Advance(
      unmatchedObjectRuleWorld,
      unmatchedObjectRuleQueue,
      unmatchedObjectRuleState,
      10).TileCommands.Count != 0)
{
  throw new InvalidOperationException("An unmatched TileObject Liquid style was not fail-closed.");
}

Console.WriteLine("PASS: TileObject Liquid rules override global tables and fail closed");

WorldGrid world = new(400, 300);
LiquidWorldStateComponent state = new(maximumQueueLength: 8, tickBudget: 8);
LiquidUpdateQueueComponent queue = new(maximumLength: 8);
LiquidInputSystem input = new();
if (!input.TryAccept(
      world,
      queue,
      state,
      new PipelineLiquidSourceComponent(20, 20, 128, LiquidType.Water, 1)) ||
    input.TryAccept(
      world,
      queue,
      state,
      new PipelineLiquidSourceComponent(-1, 20, 128, LiquidType.Water, 2)) ||
    input.TryAccept(
      world,
      queue,
      state,
      new PipelineLiquidSourceComponent(20, 20, 128, LiquidType.Water, 3)) ||
    input.TryAccept(
      world,
      queue,
      state,
      new PipelineLiquidSourceComponent(21, 20, 128, LiquidType.Water, long.MaxValue)) ||
    queue.Count != 1)
{
  throw new InvalidOperationException("Liquid input did not enforce bounds and deduplication.");
}

using DomeSimulation liquidSequenceSimulation = new(new WorldGrid(400, 300));
if (liquidSequenceSimulation.TryQueueLiquidSource(
      new PipelineLiquidSourceComponent(20, 20, 128, LiquidType.Water, long.MaxValue)))
{
  throw new InvalidOperationException(
    "DomeSimulation accepted a liquid sequence that would overflow the next allocator.");
}

if (liquidSequenceSimulation.TryQueueLiquidSource(
      new PipelineLiquidSourceComponent(20, 20, 128, LiquidType.Water, long.MaxValue - 1)))
{
  throw new InvalidOperationException(
    "DomeSimulation accepted the final liquid sequence without a successor.");
}

Console.WriteLine("PASS: liquid source rejects terminal sequences before allocator overflow");

WorldMetadata liquidBatchMetadata = new("liquid-batch-boundary", new WorldSeed(78), 400, 300);
WorldTile[,] liquidBatchTiles = new WorldTile[400, 300];
long[,] liquidBatchVersions = new long[2, 2];
liquidBatchVersions[0, 0] = long.MaxValue;
WorldGrid liquidBatchWorld = WorldGrid.FromSnapshot(new WorldGridSnapshot(
  liquidBatchMetadata,
  liquidBatchTiles,
  liquidBatchVersions));
bool liquidBatchRejected = false;
try
{
  liquidBatchWorld.CommitLiquidChanges([
    new LiquidChangeCommand(1, 10, 10, 1, 1),
    new LiquidChangeCommand(2, 11, 10, 1, 1)]);
}
catch (InvalidOperationException)
{
  liquidBatchRejected = true;
}

if (!liquidBatchRejected || liquidBatchWorld.GetTile(10, 10).LiquidAmount != 0 ||
    liquidBatchWorld.GetTile(11, 10).LiquidAmount != 0)
{
  throw new InvalidOperationException("Liquid batch overflow was not rejected atomically.");
}

Console.WriteLine("PASS: liquid batch section-version exhaustion is atomic");

WorldGrid solidTargetWorld = new(400, 300);
_ = solidTargetWorld.TrySetLiquid(20, 20, 128, (byte)LiquidType.Water);
_ = solidTargetWorld.TrySetTile(20, 19, new WorldTile(IsActive: true, Type: 7));
_ = solidTargetWorld.TrySetTile(19, 20, new WorldTile(IsActive: true, Type: 7));
_ = solidTargetWorld.TrySetTile(21, 20, new WorldTile(IsActive: true, Type: 7));
LiquidUpdateQueueComponent solidTargetQueue = new(maximumLength: 8);
LiquidWorldStateComponent solidTargetState = new(maximumQueueLength: 8, tickBudget: 8);
if (!input.TryAccept(
      solidTargetWorld,
      solidTargetQueue,
      solidTargetState,
      new PipelineLiquidSourceComponent(20, 20, 128, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Solid-target Liquid fixture could not be seeded.");
}

LiquidPropagationResult solidTargetResult = new LiquidPropagationSystem().Advance(
  solidTargetWorld,
  solidTargetQueue,
  solidTargetState,
  firstSequence: 10);
if (solidTargetResult.Commands.Count != 0)
{
  throw new InvalidOperationException("Liquid propagation entered an active solid Tile.");
}

WorldGrid platformTargetWorld = new(400, 300);
_ = platformTargetWorld.TrySetLiquid(30, 20, 128, (byte)LiquidType.Water);
_ = platformTargetWorld.TrySetTile(30, 19, new WorldTile(IsActive: true, Type: 275));
_ = platformTargetWorld.TrySetTile(29, 20, new WorldTile(IsActive: true, Type: 7));
_ = platformTargetWorld.TrySetTile(31, 20, new WorldTile(IsActive: true, Type: 7));
LiquidUpdateQueueComponent platformTargetQueue = new(maximumLength: 8);
LiquidWorldStateComponent platformTargetState = new(maximumQueueLength: 8, tickBudget: 8);
if (!input.TryAccept(
      platformTargetWorld,
      platformTargetQueue,
      platformTargetState,
      new PipelineLiquidSourceComponent(30, 20, 128, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Platform-target Liquid fixture could not be seeded.");
}

LiquidPropagationResult platformTargetResult = new LiquidPropagationSystem().Advance(
  platformTargetWorld,
  platformTargetQueue,
  platformTargetState,
  firstSequence: 10);
if (platformTargetResult.Commands.Count != 2 ||
    platformTargetResult.Commands[1].X != 30 || platformTargetResult.Commands[1].Y != 19)
{
  throw new InvalidOperationException("Liquid propagation did not enter an active platform Tile.");
}

WorldGrid unknownTargetWorld = new(400, 300);
_ = unknownTargetWorld.TrySetLiquid(40, 20, 128, (byte)LiquidType.Water);
_ = unknownTargetWorld.TrySetTile(40, 19, new WorldTile(IsActive: true, Type: 753));
_ = unknownTargetWorld.TrySetTile(39, 20, new WorldTile(IsActive: true, Type: 753));
_ = unknownTargetWorld.TrySetTile(41, 20, new WorldTile(IsActive: true, Type: 753));
LiquidUpdateQueueComponent unknownTargetQueue = new(maximumLength: 8);
LiquidWorldStateComponent unknownTargetState = new(maximumQueueLength: 8, tickBudget: 8);
if (!input.TryAccept(
      unknownTargetWorld,
      unknownTargetQueue,
      unknownTargetState,
      new PipelineLiquidSourceComponent(40, 20, 128, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Unknown-target Liquid fixture could not be seeded.");
}

LiquidPropagationResult unknownTargetResult = new LiquidPropagationSystem().Advance(
  unknownTargetWorld,
  unknownTargetQueue,
  unknownTargetState,
  firstSequence: 10);
if (unknownTargetResult.Commands.Count != 0)
{
  throw new InvalidOperationException("Liquid propagation entered an out-of-range Tile type.");
}

LiquidPropagationSystem propagation = new();
LiquidPropagationResult first = propagation.Advance(world, queue, state, firstSequence: 10);
LiquidUpdateQueueComponent replayQueue = new(maximumLength: 8);
LiquidWorldStateComponent replayState = new(maximumQueueLength: 8, tickBudget: 8);
if (!input.TryAccept(
      world,
      replayQueue,
      replayState,
      new PipelineLiquidSourceComponent(20, 20, 128, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Liquid replay fixture could not be seeded.");
}

LiquidPropagationResult second = propagation.Advance(world, replayQueue, replayState, firstSequence: 10);
if (!first.Commands.SequenceEqual(second.Commands) || first.Commands.Count < 2 ||
    first.Commands[1].X != 20 || first.Commands[1].Y != 19 ||
    world.GetTile(20, 19).LiquidAmount != 0)
{
  throw new InvalidOperationException("Liquid propagation was not deterministic or mutated the grid early.");
}

LiquidDirtySectionComponent dirtySections = new();
LiquidCommitSystem commit = new();
if (!commit.TryCommit(world, first.Commands, dirtySections, out LiquidCommitResult commitResult) ||
    commitResult.AppliedCount == 0 || dirtySections.Count == 0 ||
    world.GetTile(20, 19).LiquidAmount == 0)
{
  throw new InvalidOperationException("Liquid commit did not apply changes and section revisions.");
}

LiquidReplicationSystem replication = new();
if (replication.CreateSnapshots(first.Commands, revision: 1).Count !=
    first.Commands.Select(command => (command.X, command.Y)).Distinct().Count())
{
  throw new InvalidOperationException("Liquid replication did not coalesce coordinates.");
}

LiquidRuleRegistry rules = LiquidRuleRegistry.CreateDefault();
if (rules.OrderedDefinitions.Count != 4 ||
    rules.OrderedDefinitions[0].Type != LiquidType.Water ||
    rules.OrderedDefinitions[3].Type != LiquidType.Shimmer)
{
  throw new InvalidOperationException("Liquid definitions did not preserve deterministic order.");
}

try
{
  ((IDictionary<LiquidType, LiquidRuleDefinition>)rules.Definitions)[LiquidType.Water] =
    rules.Get(LiquidType.Water);
  throw new InvalidOperationException("Liquid definition projection was mutable.");
}
catch (NotSupportedException)
{
}
if (rules.Get(LiquidType.Water).Gravity != LiquidGravity.Down ||
    rules.Get(LiquidType.Shimmer).CanMergeWith(LiquidType.Lava))
{
  throw new InvalidOperationException("Liquid rule definitions were not explicit and immutable.");
}

(LiquidType First, LiquidType Second, LiquidType Result, ushort Tile)[] mergeCases =
[
  (LiquidType.Water, LiquidType.Lava, LiquidType.Lava, 56),
  (LiquidType.Water, LiquidType.Honey, LiquidType.Honey, 229),
  (LiquidType.Water, LiquidType.Shimmer, LiquidType.Shimmer, 659),
  (LiquidType.Lava, LiquidType.Water, LiquidType.Water, 56),
  (LiquidType.Lava, LiquidType.Honey, LiquidType.Honey, 230),
  (LiquidType.Lava, LiquidType.Shimmer, LiquidType.Shimmer, 659),
  (LiquidType.Honey, LiquidType.Water, LiquidType.Water, 229),
  (LiquidType.Honey, LiquidType.Lava, LiquidType.Lava, 230),
  (LiquidType.Honey, LiquidType.Shimmer, LiquidType.Shimmer, 659),
  (LiquidType.Shimmer, LiquidType.Water, LiquidType.Water, 659),
  (LiquidType.Shimmer, LiquidType.Lava, LiquidType.Lava, 659),
  (LiquidType.Shimmer, LiquidType.Honey, LiquidType.Honey, 659)
];
LiquidMergeSystem mergeSystem = new(rules);
for (int index = 0; index < mergeCases.Length; index++)
{
  (LiquidType firstType,
    LiquidType secondType,
    LiquidType result,
    ushort tileType) = mergeCases[index];
  if (!rules.TryGetMerge(firstType, secondType, out LiquidMergeRule rule) ||
      rule.ResultType != result || rule.ResultTileType != tileType)
  {
    throw new InvalidOperationException(
      "A liquid merge rule did not preserve its explicit outcome.");
  }

  WorldGrid mergeWorld = new(400, 300);
  _ = mergeWorld.TrySetLiquid(30, 20, 128, (byte)firstType);
  _ = mergeWorld.TrySetLiquid(31, 20, 128, (byte)secondType);
  if (!mergeSystem.TryEvaluate(mergeWorld, 30, 20, out SimulationLiquidMerge merge) ||
      merge.ResultType != result || merge.ResultTileType != tileType)
  {
    throw new InvalidOperationException(
      "Liquid merge evaluation did not project the explicit outcome.");
  }
}

using DomeSimulation runtimeMergeSimulation = new(new WorldGrid(400, 300));
_ = runtimeMergeSimulation.WorldGrid.TrySetLiquid(
  30,
  30,
  128,
  (byte)LiquidType.Water);
_ = runtimeMergeSimulation.WorldGrid.TrySetLiquid(
  31,
  30,
  128,
  (byte)LiquidType.Lava);
if (!runtimeMergeSimulation.TryQueueLiquidSource(
      new PipelineLiquidSourceComponent(30, 30, 128, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Runtime liquid merge fixture could not be seeded.");
}

runtimeMergeSimulation.Tick(new SimulationInputBatch([]));
WorldTile runtimeMergeTile = runtimeMergeSimulation.WorldGrid.GetTile(30, 30);
if (!runtimeMergeTile.IsActive || runtimeMergeTile.Type != 56)
{
  throw new InvalidOperationException(
    "Liquid merge output was not committed through the runtime Tile boundary.");
}

using DomeSimulation runtimeMergeSideEffectSimulation = new(new WorldGrid(400, 300));
_ = runtimeMergeSideEffectSimulation.WorldGrid.TrySetLiquid(
  30,
  30,
  128,
  (byte)LiquidType.Water);
_ = runtimeMergeSideEffectSimulation.WorldGrid.TrySetLiquid(
  31,
  30,
  128,
  (byte)LiquidType.Lava);
if (!runtimeMergeSideEffectSimulation.TryQueueLiquidSource(
      new PipelineLiquidSourceComponent(30, 30, 128, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Liquid merge side-effect fixture could not be seeded.");
}

runtimeMergeSideEffectSimulation.Tick(new SimulationInputBatch([]));
WorldTile sideEffectSource = runtimeMergeSideEffectSimulation.WorldGrid.GetTile(30, 30);
WorldTile sideEffectNeighbor = runtimeMergeSideEffectSimulation.WorldGrid.GetTile(31, 30);
if (sideEffectSource.LiquidAmount != 0 || sideEffectNeighbor.LiquidAmount != 0 ||
    sideEffectSource.Type != 56)
{
  throw new InvalidOperationException(
    "Liquid merge did not commit both consumed liquids and the result Tile atomically.");
}

for (int index = 0; index < mergeCases.Length; index++)
{
  (LiquidType firstType,
    LiquidType secondType,
    _,
    ushort resultTileType) = mergeCases[index];
  int mergeX = 40 + index * 5;
  const int MergeY = 40;
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  _ = simulation.WorldGrid.TrySetLiquid(mergeX, MergeY, 128, (byte)firstType);
  _ = simulation.WorldGrid.TrySetLiquid(mergeX + 1, MergeY, 128, (byte)secondType);
  if (!simulation.TryQueueLiquidSource(
        new PipelineLiquidSourceComponent(mergeX, MergeY, 128, firstType, 1)))
  {
    throw new InvalidOperationException("Runtime liquid merge matrix fixture could not be seeded.");
  }

  simulation.Tick(new SimulationInputBatch([]));
  WorldTile source = simulation.WorldGrid.GetTile(mergeX, MergeY);
  WorldTile neighbor = simulation.WorldGrid.GetTile(mergeX + 1, MergeY);
  if (source.LiquidAmount != 0 || neighbor.LiquidAmount != 0 ||
      !source.IsActive || source.Type != resultTileType)
  {
    throw new InvalidOperationException(
      "Runtime liquid merge matrix did not commit the expected consumed state and Tile.");
  }
}

Console.WriteLine("PASS: bounded liquid input, deterministic propagation, commit and replication are backed");

LiquidUpdateQueueComponent retryQueue = new(maximumLength: 1);
LiquidUpdateNode retryNode = new(2, 2, 1);
if (!retryQueue.TryEnqueue(retryNode.X, retryNode.Y, retryNode.Sequence) ||
    retryQueue.TryRequeue(retryNode, maximumRetries: 3) ||
    retryQueue.GetRetryCount(retryNode.X, retryNode.Y) != 0)
{
  throw new InvalidOperationException(
    "A failed liquid requeue consumed retry budget before entering the queue.");
}

_ = retryQueue.Drain(1);
if (!retryQueue.TryRequeue(retryNode, maximumRetries: 3) ||
    retryQueue.GetRetryCount(retryNode.X, retryNode.Y) != 1)
{
  throw new InvalidOperationException(
    "A successful liquid requeue did not consume exactly one retry budget unit.");
}

Console.WriteLine("PASS: liquid retry budget advances only after successful requeue");

using DomeSimulation blockedSimulation = new(new WorldGrid(400, 300));
_ = blockedSimulation.WorldGrid.TrySetLiquid(0, 0, 128, (byte)LiquidType.Water);
_ = blockedSimulation.WorldGrid.TrySetLiquid(1, 0, 1, (byte)LiquidType.Lava);
if (!blockedSimulation.TryQueueLiquidSource(
      new PipelineLiquidSourceComponent(0, 0, 128, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Blocked liquid fixture could not be seeded.");
}

for (int index = 0; index < 5; index++)
{
  blockedSimulation.Tick(new SimulationInputBatch([]));
}

if (blockedSimulation.GetLiquidRetryCount(0, 0) != 3 ||
    blockedSimulation.WorldGrid.GetTile(0, 0).LiquidAmount != 128)
{
  throw new InvalidOperationException("Liquid settle retries were not bounded and deterministic.");
}

Console.WriteLine("PASS: blocked liquid settle retries are bounded and observable");

WorldGrid panicWorld = new(400, 300);
LiquidPanicPolicy panicPolicy = new(
  highWaterQueueLength: 2,
  sustainedHighWaterTicks: 2,
  panicTickBudget: 3,
  recoveryQueueLength: 1);
LiquidWorldStateComponent panicState = new(
  maximumQueueLength: 8,
  tickBudget: 1,
  panicPolicy);
LiquidUpdateQueueComponent panicQueue = new(maximumLength: 8);
for (int index = 0; index < 4; index++)
{
  int x = 100 + index * 3;
  _ = panicWorld.TrySetLiquid(x, 100, 32, (byte)LiquidType.Water);
  if (!panicQueue.TryEnqueue(
        new PipelineLiquidSourceComponent(x, 100, 32, LiquidType.Water, index + 1)))
  {
    throw new InvalidOperationException("Panic Liquid fixture could not be seeded.");
  }
}

LiquidPropagationSystem panicPropagation = new();
LiquidPropagationResult firstPanicAdvance = panicPropagation.Advance(
  panicWorld,
  panicQueue,
  panicState,
  firstSequence: 100);
if (panicState.Mode != LiquidSimulationMode.Normal || firstPanicAdvance.ProcessedCount != 1)
{
  throw new InvalidOperationException(
    "Liquid Panic did not retain the normal budget before sustained high-water pressure.");
}

LiquidPropagationResult secondPanicAdvance = panicPropagation.Advance(
  panicWorld,
  panicQueue,
  panicState,
  firstSequence: 200);
if (panicState.Mode != LiquidSimulationMode.Panic || secondPanicAdvance.ProcessedCount != 3)
{
  throw new InvalidOperationException(
    "Liquid Panic did not use the configured bounded drain budget after sustained pressure.");
}

int queuedBeforeRecovery = panicQueue.Count;
panicState.ObserveQueueLength(panicPolicy.RecoveryQueueLength);
if (panicState.Mode != LiquidSimulationMode.Normal || panicQueue.Count != queuedBeforeRecovery)
{
  throw new InvalidOperationException(
    "Liquid Panic recovery did not preserve queued sources while returning to normal mode.");
}

Console.WriteLine("PASS: runtime Liquid Panic is consecutive, bounded and non-destructive");

WorldGrid sequenceOverflowWorld = new(400, 300);
_ = sequenceOverflowWorld.TrySetLiquid(1, 1, 100, (byte)LiquidType.Water);
_ = sequenceOverflowWorld.TrySetLiquid(2, 1, 0, (byte)LiquidType.Water);
LiquidTransferCommand overflowTransfer = new(1, 1, 1, 2, 1, 1, (byte)LiquidType.Water);
try
{
  _ = new LiquidTransferSystem().CreateChanges(
    sequenceOverflowWorld,
    [overflowTransfer],
    long.MaxValue);
  throw new InvalidOperationException(
    "Liquid transfer sequence allocation wrapped at Int64.MaxValue.");
}
catch (ArgumentOutOfRangeException)
{
}

Console.WriteLine("PASS: liquid transfer sequence exhaustion is rejected");

LiquidUpdateQueueComponent sequenceQueue = new(maximumLength: 4);
LiquidWorldStateComponent sequenceState = new(
  maximumQueueLength: 4,
  tickBudget: 1,
  new LiquidPanicPolicy(4, 2, 2, 1));
_ = sequenceOverflowWorld.TrySetLiquid(4, 4, 100, (byte)LiquidType.Water);
if (!sequenceQueue.TryEnqueue(
      new PipelineLiquidSourceComponent(4, 4, 100, LiquidType.Water, 1)))
{
  throw new InvalidOperationException("Liquid sequence exhaustion fixture could not be seeded.");
}

try
{
  _ = new LiquidPropagationSystem().Advance(
    sequenceOverflowWorld,
    sequenceQueue,
    sequenceState,
    long.MaxValue);
  throw new InvalidOperationException(
    "Liquid propagation sequence allocation wrapped at Int64.MaxValue.");
}
catch (ArgumentOutOfRangeException)
{
}

Console.WriteLine("PASS: liquid propagation sequence exhaustion is rejected");
