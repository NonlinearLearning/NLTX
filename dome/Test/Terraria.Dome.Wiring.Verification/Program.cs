using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Systems;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.Wiring.Commands;
using Terraria.Dome.Simulation.Wiring.Components;
using Terraria.Dome.Simulation.Wiring.Definitions;
using Terraria.Dome.Simulation.Wiring.Systems;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldObjects.Chest.Systems;

IReadOnlySet<ushort> containerDefaults =
  TileBreakabilityProtectionRuleSystem.RegisterContainerDefaults();
IReadOnlySet<ushort> topRemovalDefaults =
  TileBreakabilityProtectionRuleSystem.RegisterTopRemovalDefaults();
if (containerDefaults.Count != 7 || topRemovalDefaults.Count != 17 ||
    !containerDefaults.Contains(21) || !topRemovalDefaults.Contains(634) ||
    containerDefaults.Contains(1) || topRemovalDefaults.Contains(1) ||
    !containerDefaults.SetEquals(TileBreakabilityProtectionRuleSystem.RegisterContainerDefaults()) ||
    !topRemovalDefaults.SetEquals(TileBreakabilityProtectionRuleSystem.RegisterTopRemovalDefaults()))
{
  throw new InvalidOperationException("Tile breakability defaults were not stable or bounded.");
}

IReadOnlySet<ushort> legacyContainerDefaults = LegacyTileContainerRegistry.RegisterDefaults();
HashSet<ushort> expectedLegacyContainerDefaults = [21, 88, 467, 470, 475];
if (!legacyContainerDefaults.SetEquals(expectedLegacyContainerDefaults) ||
    legacyContainerDefaults.Contains(441) || legacyContainerDefaults.Contains(468))
{
  throw new InvalidOperationException("Legacy tile-container defaults drifted from Version4.");
}

HashSet<ushort> expectedLegacySignDefaults = [55, 85, 425, 573];
if (!LegacySignTileRegistry.RegisterDefaults().SetEquals(expectedLegacySignDefaults) ||
    LegacySignTileRegistry.IsSign(1))
{
  throw new InvalidOperationException("Legacy sign-tile defaults drifted from Version4.");
}

if (LampDefinitionRegistry.SourceDerived.OrderedDefinitions.Count != 16 ||
    LampDefinitionRegistry.SourceDerived.OrderedDefinitions[0].TileType != 4 ||
    LampDefinitionRegistry.SourceDerived.OrderedDefinitions[15].TileType != 646)
{
  throw new InvalidOperationException("Lamp definitions did not preserve source registration order.");
}

IReadOnlySet<ushort> boulderDefaults = LegacyBoulderRuleSystem.RegisterDefaults();
IReadOnlySet<ushort> treeTrunkDefaults = LegacyTreeTrunkRuleSystem.RegisterDefaults();
if (boulderDefaults.Count != 10 || treeTrunkDefaults.Count != 12 ||
    !boulderDefaults.Contains(138) || !treeTrunkDefaults.Contains(634) ||
    !boulderDefaults.SetEquals(LegacyBoulderRuleSystem.RegisterDefaults()) ||
    !treeTrunkDefaults.SetEquals(LegacyTreeTrunkRuleSystem.RegisterDefaults()))
{
  throw new InvalidOperationException("Boulder or tree-trunk defaults were mutable or unstable.");
}

IReadOnlySet<ushort> actuationDefaults = LegacyActuationProtectionRuleSystem.RegisterDefaults();
IReadOnlySet<ushort> specialDeactivationDefaults =
  ActuatorDeactivationRuleSystem.RegisterSpecialNonActuatedDefaults();
if (actuationDefaults.Count != 11 || specialDeactivationDefaults.Count != 7 ||
    !actuationDefaults.Contains(21) || !actuationDefaults.Contains(468) ||
    !specialDeactivationDefaults.Contains(314) || !specialDeactivationDefaults.Contains(476) ||
    !actuationDefaults.SetEquals(LegacyActuationProtectionRuleSystem.RegisterDefaults()) ||
    !specialDeactivationDefaults.SetEquals(
      ActuatorDeactivationRuleSystem.RegisterSpecialNonActuatedDefaults()))
{
  throw new InvalidOperationException("Actuation protection defaults were mutable or unstable.");
}

IReadOnlySet<ushort> alwaysProtectedDefaults =
  SpecialTileProtectionRuleSystem.RegisterAlwaysProtectedDefaults();
if (alwaysProtectedDefaults.Count != 7 || !alwaysProtectedDefaults.Contains(21) ||
    !alwaysProtectedDefaults.Contains(488) ||
    !alwaysProtectedDefaults.SetEquals(SpecialTileProtectionRuleSystem.RegisterAlwaysProtectedDefaults()))
{
  throw new InvalidOperationException("Special tile protection defaults were mutable or unstable.");
}

try
{
  ((IDictionary<ushort, LampDefinition>)LampDefinitionRegistry.SourceDerived.Definitions)[4] =
    LampDefinitionRegistry.SourceDerived.OrderedDefinitions[0];
  throw new InvalidOperationException("Lamp definition projection was mutable.");
}
catch (NotSupportedException)
{
}

if (!ActuatorDeactivationRuleSystem.ShouldDeactivate(
      isActive: true,
      isActuated: true,
      tileType: 1,
      isSolid: true,
      isNotReallySolid: false,
      isType226: false,
      belowWorldSurface: false,
      defeatedPlantera: false,
      tileAboveIsActive: false,
      tileAbovePreventsActuation: false,
      canKillTile: true) ||
    ActuatorDeactivationRuleSystem.ShouldDeactivate(
      isActive: true,
      isActuated: true,
      tileType: 314,
      isSolid: true,
      isNotReallySolid: false,
      isType226: false,
      belowWorldSurface: false,
      defeatedPlantera: false,
      tileAboveIsActive: false,
      tileAbovePreventsActuation: false,
      canKillTile: true))
{
  throw new InvalidOperationException("Actuator source eligibility rule did not preserve its guards.");
}

IReadOnlyList<PressurePlateComponent> overflowPlates =
[
  new PressurePlateComponent(1, 0, 0, 10, requiresPlayer: false),
  new PressurePlateComponent(2, 0, 0, 10, requiresPlayer: false)
];
IReadOnlyList<MechanismActivationCommand> overflowActivations =
  new PressurePlateDetectionSystem().Detect(
    overflowPlates,
    [new WiringActorSnapshot(new SimulationVector(0.0f, 0.0f), IsPlayer: true)],
    long.MaxValue);
if (overflowActivations.Count != 0)
{
  throw new InvalidOperationException(
    "Pressure-plate sequence allocation wrapped at Int64.MaxValue.");
}

Console.WriteLine("PASS: wiring pressure-plate sequence exhaustion is rejected");

WorldGrid lampSequenceWorld = new(400, 300);
for (int y = 10; y < 13; y++)
{
  for (int x = 10; x < 13; x++)
  {
    _ = lampSequenceWorld.TrySetTile(x, y, new WorldTile(IsActive: true, Type: 34));
  }
}

if (new LampCommandSystem().CreateFrameCommands(
      lampSequenceWorld,
      LampDefinitionRegistry.SourceDerived,
      10,
      10,
      MechanismActivationKind.Activate,
      long.MaxValue).Count != 0)
{
  throw new InvalidOperationException(
    "Lamp frame sequence allocation wrapped at Int64.MaxValue.");
}

Console.WriteLine("PASS: wiring lamp sequence exhaustion is rejected");

if (ActuatorDeactivationRuleSystem.ShouldDeactivate(
      isActive: true,
      isActuated: true,
      tileType: 1,
      isSolid: true,
      isNotReallySolid: false,
      isType226: false,
      belowWorldSurface: false,
      defeatedPlantera: false,
      tileAboveIsActive: true,
      tileAbovePreventsActuation: false,
      canKillTile: false))
{
  throw new InvalidOperationException("Actuator source rule must reject an unbreakable active-above tile.");
}

if (!LockedDoorRuleSystem.IsLocked(new WorldTile(true, 10, FrameX: 53, FrameY: 594)) ||
    !LockedDoorRuleSystem.IsLocked(new WorldTile(false, 10, FrameX: 0, FrameY: 646)) ||
    LockedDoorRuleSystem.IsLocked(new WorldTile(true, 10, FrameX: 54, FrameY: 594)) ||
    LockedDoorRuleSystem.IsLocked(new WorldTile(true, 10, FrameX: 0, FrameY: 593)) ||
    LockedDoorRuleSystem.IsLocked(new WorldTile(true, 10, FrameX: 0, FrameY: 647)) ||
    LockedDoorRuleSystem.IsLocked(new WorldTile(true, 11, FrameX: 0, FrameY: 594)))
{
  throw new InvalidOperationException("Locked-door frame classification did not match Version4.");
}

if (!BoulderChestProtectionRuleSystem.TryGetAboveCoordinates(
      new WorldTile(true, 546, FrameX: 54, FrameY: 54),
      tileX: 20,
      tileY: 30,
      out int boulderLeftX,
      out int boulderAboveY,
      out int boulderRightX) ||
    boulderLeftX != 19 ||
    boulderRightX != 20 ||
    boulderAboveY != 28 ||
    !BoulderChestProtectionRuleSystem.IsBlocked(
      isBoulder: true,
      leftAboveHasBreakabilityBlock: false,
      rightAboveHasBreakabilityBlock: true) ||
    BoulderChestProtectionRuleSystem.IsBlocked(
      isBoulder: false,
      leftAboveHasBreakabilityBlock: false,
      rightAboveHasBreakabilityBlock: true))
{
  throw new InvalidOperationException("Boulder chest source relation was not preserved.");
}

if (BoulderChestProtectionRuleSystem.TryGetAboveCoordinates(
      new WorldTile(false, 546, FrameX: 54, FrameY: 54),
      tileX: 20,
      tileY: 30,
      out _,
      out _,
      out _) ||
    BoulderChestProtectionRuleSystem.TryGetAboveCoordinates(
      new WorldTile(true, 546, FrameX: -1, FrameY: 54),
      tileX: 20,
      tileY: 30,
      out _,
      out _,
      out _))
{
  throw new InvalidOperationException("Invalid boulder frame state was accepted.");
}

foreach (ushort boulderTileType in new ushort[]
         {
           138, 484, 664, 665, 711, 712, 713, 714, 715, 716
         })
{
  if (!LegacyBoulderRuleSystem.IsBoulder(boulderTileType))
  {
    throw new InvalidOperationException("A Version4 boulder tile type was not classified.");
  }
}

foreach (ushort nonBoulderTileType in new ushort[] { 137, 139, 483, 485, 663, 666, 710, 717 })
{
  if (LegacyBoulderRuleSystem.IsBoulder(nonBoulderTileType))
  {
    throw new InvalidOperationException("A non-boulder tile type was classified as a boulder.");
  }
}

foreach (ushort treeTrunkTileType in new ushort[]
         {
           5, 72, 583, 584, 585, 586, 587, 588, 589, 596, 616, 634
         })
{
  if (!LegacyTreeTrunkRuleSystem.IsTreeTrunk(treeTrunkTileType))
  {
    throw new InvalidOperationException("A Version4 tree-trunk tile type was not classified.");
  }
}

foreach (ushort nonTreeTrunkTileType in new ushort[] { 4, 6, 71, 73, 582, 590, 595, 635 })
{
  if (LegacyTreeTrunkRuleSystem.IsTreeTrunk(nonTreeTrunkTileType))
  {
    throw new InvalidOperationException("A non-tree-trunk tile type was classified as a trunk.");
  }
}

if (!TreeTrunkProtectionRuleSystem.ShouldProtectAbove(
      candidateTileType: 1,
      aboveTile: new WorldTile(true, 5, FrameX: 0, FrameY: 0)) ||
    TreeTrunkProtectionRuleSystem.ShouldProtectAbove(
      candidateTileType: 5,
      aboveTile: new WorldTile(true, 5, FrameX: 0, FrameY: 0)) ||
    TreeTrunkProtectionRuleSystem.ShouldProtectAbove(
      candidateTileType: 1,
      aboveTile: new WorldTile(true, 4, FrameX: 0, FrameY: 0)))
{
  throw new InvalidOperationException("Tree-trunk base protection did not preserve Version4.");
}

foreach ((short frameX, short frameY) in new[]
         {
           ((short)66, (short)0), ((short)66, (short)44),
           ((short)88, (short)66), ((short)88, (short)110)
         })
{
  if (TreeTrunkProtectionRuleSystem.ShouldProtectAbove(
        candidateTileType: 1,
        aboveTile: new WorldTile(true, 5, FrameX: frameX, FrameY: frameY)))
  {
    throw new InvalidOperationException("A source-exempt tree-trunk frame was protected.");
  }
}

foreach ((short frameX, short frameY) in new[]
         {
           ((short)66, (short)45), ((short)88, (short)65),
           ((short)88, (short)111), ((short)0, (short)197)
         })
{
  if (!TreeTrunkProtectionRuleSystem.ShouldProtectAbove(
        candidateTileType: 1,
        aboveTile: new WorldTile(true, 5, FrameX: frameX, FrameY: frameY)))
  {
    throw new InvalidOperationException("A source-protected tree-trunk frame was allowed.");
  }
}

if (!SpecialTileProtectionRuleSystem.ShouldProtectAbove(
      candidateTileType: 1,
      aboveTile: new WorldTile(true, 323, FrameX: 66)) ||
    !SpecialTileProtectionRuleSystem.ShouldProtectAbove(
      candidateTileType: 1,
      aboveTile: new WorldTile(true, 323, FrameX: 220)) ||
    SpecialTileProtectionRuleSystem.ShouldProtectAbove(
      candidateTileType: 1,
      aboveTile: new WorldTile(true, 323, FrameX: 68)) ||
    SpecialTileProtectionRuleSystem.ShouldProtectAbove(
      candidateTileType: 323,
      aboveTile: new WorldTile(true, 323, FrameX: 66)))
{
  throw new InvalidOperationException("Type 323 special frame relation was not preserved.");
}

foreach (ushort specialTileType in new ushort[] { 21, 26, 72, 77, 88, 467, 488 })
{
  if (!SpecialTileProtectionRuleSystem.ShouldProtectAbove(
        candidateTileType: 1,
        aboveTile: new WorldTile(true, specialTileType, FrameX: 123, FrameY: 456)) ||
      SpecialTileProtectionRuleSystem.ShouldProtectAbove(
        candidateTileType: specialTileType,
        aboveTile: new WorldTile(true, specialTileType)))
  {
    throw new InvalidOperationException("A fixed special tile relation was not preserved.");
  }
}

foreach (short frameColumn in new short[] { 0, 1, 4, 5 })
{
  if (!SpecialTileProtectionRuleSystem.ShouldProtectAbove(
        candidateTileType: 1,
        aboveTile: new WorldTile(true, 80, FrameX: (short)(frameColumn * 18))))
  {
    throw new InvalidOperationException("A protected Type 80 frame column was allowed.");
  }
}

foreach (short frameColumn in new short[] { 2, 3, 6 })
{
  if (SpecialTileProtectionRuleSystem.ShouldProtectAbove(
        candidateTileType: 1,
        aboveTile: new WorldTile(true, 80, FrameX: (short)(frameColumn * 18))) ||
      SpecialTileProtectionRuleSystem.ShouldProtectAbove(
        candidateTileType: 1,
        aboveTile: new WorldTile(false, 80, FrameX: (short)(frameColumn * 18))))
  {
    throw new InvalidOperationException("An unprotected Type 80 frame was blocked.");
  }
}

if (!MultiTileProtectionRuleSystem.IsBlocked(
      candidateTileType: 235,
      aboveTiles: new[]
      {
        new WorldTile(true, 1),
        new WorldTile(true, 21),
        new WorldTile(true, 1)
      },
      isHardMode: true) ||
    MultiTileProtectionRuleSystem.IsBlocked(
      candidateTileType: 234,
      aboveTiles: new[]
      {
        new WorldTile(true, 21),
        new WorldTile(true, 1),
        new WorldTile(true, 1)
      },
      isHardMode: true) ||
    MultiTileProtectionRuleSystem.IsBlocked(
      candidateTileType: 235,
      aboveTiles: new[]
      {
        new WorldTile(false, 21),
        new WorldTile(false, 1),
        new WorldTile(false, 1)
      },
      isHardMode: true))
{
  throw new InvalidOperationException("Type 235 multi-tile protection was not preserved.");
}

if (MultiTileProtectionRuleSystem.IsBlocked(
      candidateTileType: 235,
      aboveTiles: new[] { new WorldTile(true, 1), new WorldTile(true, 1) },
      isHardMode: true))
{
  throw new InvalidOperationException("An incomplete Type 235 footprint was accepted.");
}

foreach (ushort preventsActuationType in new ushort[]
         { 21, 467, 26, 77, 88, 470, 475, 237, 597, 441, 468 })
{
  if (!LegacyActuationProtectionRuleSystem.PreventsActuationUnder(preventsActuationType))
  {
    throw new InvalidOperationException("A Version4 PreventsActuationUnder ID was not classified.");
  }
}

foreach (ushort ordinaryActuationType in new ushort[] { 1, 20, 22, 236, 238, 440, 469 })
{
  if (LegacyActuationProtectionRuleSystem.PreventsActuationUnder(ordinaryActuationType))
  {
    throw new InvalidOperationException("An ordinary tile was classified as actuation-protected.");
  }
}

WorldGrid multiTileProtectionWorld = new(400, 300);
_ = multiTileProtectionWorld.TrySetTile(
  20,
  30,
  new WorldTile(IsActive: true, Type: 235, FrameX: 18));
_ = multiTileProtectionWorld.TrySetTile(20, 29, new WorldTile(IsActive: true, Type: 1));
_ = multiTileProtectionWorld.TrySetTile(21, 29, new WorldTile(IsActive: true, Type: 21));
_ = multiTileProtectionWorld.TrySetTile(22, 29, new WorldTile(IsActive: true, Type: 1));
if (!LegacyMultiTileProtectionQuery.TryGetIsBlocked(
      multiTileProtectionWorld,
      tileX: 20,
      tileY: 30,
      isHardMode: true,
      out bool multiTileBlocked) ||
    !multiTileBlocked)
{
  throw new InvalidOperationException("Type 235 frame origin did not protect its upper footprint.");
}

_ = multiTileProtectionWorld.TrySetTile(21, 29, new WorldTile(IsActive: true, Type: 1));
if (!LegacyMultiTileProtectionQuery.TryGetIsBlocked(
      multiTileProtectionWorld,
      tileX: 20,
      tileY: 30,
      isHardMode: true,
      out multiTileBlocked) ||
    multiTileBlocked)
{
  throw new InvalidOperationException("An ordinary Type 235 upper footprint was blocked.");
}

if (LegacyMultiTileProtectionQuery.TryGetIsBlocked(
      multiTileProtectionWorld,
      tileX: 0,
      tileY: 0,
      isHardMode: true,
      out _))
{
  throw new InvalidOperationException("An out-of-bounds Type 235 query returned a fact.");
}

WorldGrid boulderProtectionWorld = new(400, 300);
_ = boulderProtectionWorld.TrySetTile(
  20,
  30,
  new WorldTile(IsActive: true, Type: 138, FrameX: 54, FrameY: 54));
_ = boulderProtectionWorld.TrySetTile(19, 28, new WorldTile(IsActive: true, Type: 21));
if (!LegacyBoulderChestProtectionQuery.TryGetIsBlocked(
      boulderProtectionWorld,
      tileX: 20,
      tileY: 30,
      isHardMode: true,
      out bool boulderChestBlocked) ||
    !boulderChestBlocked)
{
  throw new InvalidOperationException("A boulder above a protected container was allowed.");
}

_ = boulderProtectionWorld.TrySetTile(19, 28, new WorldTile(IsActive: true, Type: 1));
if (!LegacyBoulderChestProtectionQuery.TryGetIsBlocked(
      boulderProtectionWorld,
      tileX: 20,
      tileY: 30,
      isHardMode: true,
      out boulderChestBlocked) ||
    boulderChestBlocked)
{
  throw new InvalidOperationException("A boulder without protected upper tiles was blocked.");
}

_ = boulderProtectionWorld.TrySetTile(21, 30, new WorldTile(IsActive: true, Type: 1));
if (LegacyBoulderChestProtectionQuery.TryGetIsBlocked(
      boulderProtectionWorld,
      tileX: 0,
      tileY: 0,
      isHardMode: true,
      out _) ||
    LegacyBoulderChestProtectionQuery.TryGetIsBlocked(
      boulderProtectionWorld,
      tileX: 21,
      tileY: 30,
      isHardMode: true,
      out _))
{
  throw new InvalidOperationException("Boulder protection accepted an invalid source state.");
}

if (!TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
      ignoredTileType: 1,
      targetTile: new WorldTile(true, 77),
      isHardMode: false,
      scanForContainer: false) ||
    TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
      ignoredTileType: 77,
      targetTile: new WorldTile(true, 77),
      isHardMode: false,
      scanForContainer: false) ||
    !TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
      ignoredTileType: 1,
      targetTile: new WorldTile(true, 5),
      isHardMode: true,
      scanForContainer: false) ||
    !TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
      ignoredTileType: 1,
      targetTile: new WorldTile(true, 10, FrameX: 0, FrameY: 594),
      isHardMode: true,
      scanForContainer: false) ||
    !TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
      ignoredTileType: 1,
      targetTile: new WorldTile(true, 21),
      isHardMode: true,
      scanForContainer: true) ||
    TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
      ignoredTileType: 1,
      targetTile: new WorldTile(true, 21),
      isHardMode: true,
      scanForContainer: false) ||
    TileBreakabilityProtectionRuleSystem.HasReasonToReturnEarly(
      ignoredTileType: 1,
      targetTile: new WorldTile(true, 1),
      isHardMode: true,
      scanForContainer: true))
{
  throw new InvalidOperationException("Tile breakability protection did not preserve Version4 guards.");
}

foreach ((bool isActive, bool isActuated, bool isSolid, bool isNotReallySolid) in new[]
         {
           (false, true, true, false),
           (true, false, true, false),
           (true, true, false, false),
           (true, true, true, true)
         })
{
  if (ActuatorDeactivationRuleSystem.ShouldDeactivate(
        isActive: isActive,
        isActuated: isActuated,
        tileType: 1,
        isSolid: isSolid,
        isNotReallySolid: isNotReallySolid,
        isType226: false,
        belowWorldSurface: false,
        defeatedPlantera: false,
        tileAboveIsActive: false,
        tileAbovePreventsActuation: false,
        canKillTile: true))
  {
    throw new InvalidOperationException("Actuator source rule accepted an invalid target guard.");
  }
}

if (ActuatorDeactivationRuleSystem.ShouldDeactivate(
      isActive: true,
      isActuated: true,
      tileType: 226,
      isSolid: true,
      isNotReallySolid: false,
      isType226: true,
      belowWorldSurface: true,
      defeatedPlantera: false,
      tileAboveIsActive: false,
      tileAbovePreventsActuation: false,
      canKillTile: true) ||
    !ActuatorDeactivationRuleSystem.ShouldDeactivate(
      isActive: true,
      isActuated: true,
      tileType: 226,
      isSolid: true,
      isNotReallySolid: false,
      isType226: true,
      belowWorldSurface: true,
      defeatedPlantera: true,
      tileAboveIsActive: false,
      tileAbovePreventsActuation: false,
      canKillTile: true) ||
    ActuatorDeactivationRuleSystem.ShouldDeactivate(
      isActive: true,
      isActuated: true,
      tileType: 1,
      isSolid: true,
      isNotReallySolid: false,
      isType226: false,
      belowWorldSurface: false,
      defeatedPlantera: false,
      tileAboveIsActive: true,
      tileAbovePreventsActuation: true,
      canKillTile: true))
{
  throw new InvalidOperationException("Actuator source rule lost type-226 or upper-tile guards.");
}

if (!ActuatorCanKillTileRuleSystem.CanKill(
      isInsideWorld: true,
      tileExists: true,
      isActive: true,
      wallType: 0,
      activeAboveHasProtectedTreeRelation: false,
      activeAboveHasProtectedSpecialRelation: false,
      activeAboveHasProtectedFrameRelation: false,
      isBoulderBlockedByChest: false,
      isLockedDoor: false,
      isMultiTileBlocked: false,
      isChestBlocked: false))
{
  throw new InvalidOperationException("CanKillTile pure rule rejected an eligible target.");
}

foreach ((bool inside, bool exists, bool active, ushort wall) in new[]
         {
           (false, true, true, (ushort)0),
           (true, false, true, (ushort)0),
           (true, true, false, (ushort)0),
           (true, true, true, (ushort)350)
         })
{
  if (ActuatorCanKillTileRuleSystem.CanKill(
        inside,
        exists,
        active,
        wall,
        activeAboveHasProtectedTreeRelation: false,
        activeAboveHasProtectedSpecialRelation: false,
        activeAboveHasProtectedFrameRelation: false,
        isBoulderBlockedByChest: false,
        isLockedDoor: false,
        isMultiTileBlocked: false,
        isChestBlocked: false))
  {
    throw new InvalidOperationException("CanKillTile pure rule accepted an invalid base guard.");
  }
}

foreach (int blockedIndex in Enumerable.Range(0, 7))
{
  bool result = ActuatorCanKillTileRuleSystem.CanKill(
    isInsideWorld: true,
    tileExists: true,
    isActive: true,
    wallType: 0,
    activeAboveHasProtectedTreeRelation: blockedIndex == 0,
    activeAboveHasProtectedSpecialRelation: blockedIndex == 1,
    activeAboveHasProtectedFrameRelation: blockedIndex == 2,
    isBoulderBlockedByChest: blockedIndex == 3,
    isLockedDoor: blockedIndex == 4,
    isMultiTileBlocked: blockedIndex == 5,
    isChestBlocked: blockedIndex == 6);
  if (result)
  {
    throw new InvalidOperationException("CanKillTile pure rule accepted a protected target.");
  }
}

ChestComponent emptyChest = new(1, 40, 40);
if (!ChestDestructionRuleSystem.CanDestroy(emptyChest))
{
  throw new InvalidOperationException("An empty chest must satisfy the legacy destruction rule.");
}

emptyChest.SetSlot(0, new Terraria.Dome.Simulation.Items.ItemStack(1, 1));
if (ChestDestructionRuleSystem.CanDestroy(emptyChest))
{
  throw new InvalidOperationException("A populated chest must block destruction.");
}

if (ActuatorCanKillTileRuleSystem.CanKill(
      isInsideWorld: true,
      tileExists: true,
      isActive: true,
      wallType: 0,
      activeAboveHasProtectedTreeRelation: false,
      activeAboveHasProtectedSpecialRelation: false,
      activeAboveHasProtectedFrameRelation: false,
      isBoulderBlockedByChest: false,
      isLockedDoor: false,
      isMultiTileBlocked: false,
      isChestBlocked: true))
{
  throw new InvalidOperationException("A populated chest must block the CanKillTile contract.");
}

WireNetworkComponent network = new();
network.SetMask(10, 10, 1);
network.SetMask(11, 10, 1);
network.SetMask(11, 11, 1);
WireTraversalResult traversal = new WireTraversalSystem().Traverse(
  network,
  new WireTraversalStateComponent(),
  10,
  10,
  WireColor.Red,
  maximumNodes: 10);
if (traversal.BudgetExceeded || !traversal.Nodes.Select(node => (node.X, node.Y)).SequenceEqual(
      new[] { (10, 10), (11, 10), (11, 11) }))
{
  throw new InvalidOperationException("Wire traversal order or deduplication was not deterministic.");
}

PressurePlateComponent plate = new(7, 10, 10, 2, true);
IReadOnlyList<MechanismActivationCommand> plateCommands = new PressurePlateDetectionSystem().Detect(
  new[] { plate },
  new[]
  {
    new WiringActorSnapshot(new SimulationVector(10, 10), IsPlayer: true),
    new WiringActorSnapshot(new SimulationVector(10, 10), IsPlayer: false)
  },
  firstSequence: 20);
if (plateCommands.Count != 1 || plateCommands[0].MechanismId != 7)
{
  throw new InvalidOperationException("Pressure plate detection did not filter actors deterministically.");
}

Dictionary<int, MechanismComponent> mechanisms = new()
{
  [7] = new MechanismComponent(7, MechanismType.Actuator)
};
IReadOnlyList<MechanismActivationCommand> accepted = new MechanismActivationSystem().Apply(
  mechanisms,
  [plateCommands[0], plateCommands[0]]);
if (accepted.Count != 1 || !mechanisms[7].IsActive)
{
  throw new InvalidOperationException("Mechanism activation did not deduplicate commands.");
}

ActuatorComponent actuator = new(
  7,
  new[] { new WiringTileCoordinate(20, 20), new WiringTileCoordinate(21, 20) },
  tileType: 1);
WorldGrid actuatorWorld = new(400, 300);
WorldTile actuatorTile = new(
  IsActive: true,
  Type: 1,
  FrameX: 18,
  FrameY: 36,
  WallType: 4,
  IsActuated: true);
_ = actuatorWorld.TrySetTile(20, 20, actuatorTile);
_ = actuatorWorld.TrySetTile(21, 20, actuatorTile);
IReadOnlyList<TileChangeCommand> tileCommands = new ActuatorCommandSystem().CreateCommands(
  world: actuatorWorld,
  actuator: actuator,
  activation: accepted[0],
  firstSequence: 30);
if (tileCommands.Count != 2 || tileCommands[0].Kind != TileChangeKind.SetInactive ||
    !tileCommands[0].IsInactive || tileCommands[1].Sequence != 31)
{
  throw new InvalidOperationException(
    "Actuator did not produce ordered tile-preserving inactive-state commands.");
}

actuatorWorld.EnqueueTileChange(tileCommands[0]);
actuatorWorld.EnqueueTileChange(tileCommands[1]);
actuatorWorld.CommitTileChanges();
WorldTile inactiveActuatorTile = actuatorWorld.GetTile(20, 20);
if (!inactiveActuatorTile.IsInactive || inactiveActuatorTile.Type != actuatorTile.Type ||
    inactiveActuatorTile.FrameX != actuatorTile.FrameX ||
    inactiveActuatorTile.FrameY != actuatorTile.FrameY ||
    inactiveActuatorTile.WallType != actuatorTile.WallType ||
    !inactiveActuatorTile.IsActuated)
{
  throw new InvalidOperationException(
    "Actuator deactivation did not preserve the target tile while changing inactive state.");
}

IReadOnlyList<TileChangeCommand> reactivationCommands = new ActuatorCommandSystem().CreateCommands(
  world: actuatorWorld,
  actuator: actuator,
  activation: new MechanismActivationCommand(32, 7, MechanismActivationKind.Activate, 7),
  firstSequence: 32);
if (reactivationCommands.Count != 2 ||
    reactivationCommands[0].Kind != TileChangeKind.SetInactive ||
    reactivationCommands[0].IsInactive)
{
  throw new InvalidOperationException("Actuator did not create a reactivation state command.");
}

actuatorWorld.EnqueueTileChange(reactivationCommands[0]);
actuatorWorld.EnqueueTileChange(reactivationCommands[1]);
actuatorWorld.CommitTileChanges();
if (actuatorWorld.GetTile(20, 20) != actuatorTile ||
    actuatorWorld.GetTile(21, 20) != actuatorTile)
{
  throw new InvalidOperationException(
    "Actuator reactivation did not restore the original tile state.");
}

if (!LegacyActuatorTileDefinitionRuleSystem.TryGet(
      2,
      out bool type2IsSolid,
      out bool type2IsNotReallySolid) ||
    !type2IsSolid ||
    type2IsNotReallySolid)
{
  throw new InvalidOperationException(
    "Source-backed Type 2 solid classification was not preserved.");
}

ushort[] expectedStaticSolidTileTypes =
[
  0, 1, 2, 6, 7, 8, 9, 10, 19, 22, 23, 25, 30, 37, 38, 39, 40, 41, 43, 44, 45,
  46, 47, 48, 53, 54, 56, 57, 58, 59, 60, 63, 64, 65, 66, 67, 68, 70, 75, 76,
  107, 108, 109, 111, 112, 116, 117, 118, 119, 120, 121, 122, 123, 127, 130, 137,
  138, 140, 145, 146, 147, 148, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159,
  160, 161, 162, 163, 164, 166, 167, 168, 169, 170, 175, 176, 177, 179, 180, 181,
  182, 183, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 202,
  203, 204, 206, 208, 211, 221, 222, 223, 224, 225, 226, 229, 230, 232, 234, 235,
  239, 248, 249, 250, 251, 252, 253, 255, 256, 257, 258, 259, 260, 261, 262, 263,
  264, 265, 266, 267, 268, 272, 273, 274, 284, 311, 312, 313, 315, 321, 322, 325,
  326, 327, 328, 329, 345, 346, 347, 348, 350, 357, 367, 368, 369, 370, 371, 379,
  380, 381, 383, 384, 385, 387, 388, 396, 397, 398, 399, 400, 401, 402, 403, 404,
  407, 408, 409, 415, 416, 417, 418, 421, 422, 426, 427, 430, 431, 432, 433, 434,
  435, 436, 437, 438, 439, 446, 447, 448, 458, 459, 460, 472, 473, 474, 476, 477,
  478, 479, 481, 482, 483, 484, 492, 495, 496, 498, 500, 501, 502, 503, 507, 508,
  512, 513, 514, 515, 516, 517, 534, 535, 536, 537, 539, 540, 541, 546, 557, 562,
  563, 566, 618, 625, 626, 627, 628, 633, 635, 641, 659, 661, 662, 664, 666, 667,
  668, 669, 670, 671, 672, 673, 674, 675, 676, 677, 678, 679, 680, 681, 682, 683,
  684, 685, 686, 687, 688, 689, 690, 691, 692, 708, 711, 712, 713, 714, 715, 716,
  717, 718, 719, 722, 726, 727, 728, 729, 730, 731, 732, 734, 735, 736, 737, 738,
  739, 740, 741, 742, 743, 744, 745, 746, 747, 748, 749, 750
];
foreach (ushort tileType in expectedStaticSolidTileTypes)
{
  if (!LegacyActuatorTileDefinitionRuleSystem.TryGet(
        tileType,
        out bool isStaticSolid,
        out _)
      || !isStaticSolid)
  {
    throw new InvalidOperationException(
      $"V1456 static tileSolid type {tileType} was not classified as solid.");
  }
}

foreach (ushort tileType in new ushort[] { 3, 4, 5, 11, 110, 634 })
{
  if (LegacyActuatorTileDefinitionRuleSystem.TryGet(tileType, out bool isFalseSolid, out _) &&
      isFalseSolid)
  {
    throw new InvalidOperationException(
      $"V1456 static tileSolid false type {tileType} was classified as solid.");
  }
}

WorldGrid type2World = new(400, 300);
WorldTile type2Tile = actuatorTile with { Type = 2 };
_ = type2World.TrySetTile(22, 20, type2Tile);
IReadOnlyList<TileChangeCommand> type2Commands = new ActuatorCommandSystem().CreateCommands(
  type2World,
  new ActuatorComponent(13, [new WiringTileCoordinate(22, 20)], tileType: 2),
  new MechanismActivationCommand(45, 13, MechanismActivationKind.Activate, 13),
  firstSequence: 45);
if (type2Commands.Count != 1 ||
    type2Commands[0].Kind != TileChangeKind.SetInactive ||
    !type2Commands[0].IsInactive)
{
  throw new InvalidOperationException(
    "Source-backed Type 2 actuator did not produce an inactive-state command.");
}

WorldGrid rejectedActuatorWorld = new(400, 300);
WorldTile nonActuatorTile = actuatorTile with { IsActuated = false };
_ = rejectedActuatorWorld.TrySetTile(25, 20, nonActuatorTile);
long nonActuatorVersion = rejectedActuatorWorld.GetSectionVersion(
  rejectedActuatorWorld.GetSectionCoordinates(25, 20));
IReadOnlyList<TileChangeCommand> nonActuatorCommands = new ActuatorCommandSystem().CreateCommands(
  rejectedActuatorWorld,
  new ActuatorComponent(8, [new WiringTileCoordinate(25, 20)], tileType: 1),
  new MechanismActivationCommand(40, 8, MechanismActivationKind.Activate, 8),
  firstSequence: 40);
if (nonActuatorCommands.Count != 0 ||
    rejectedActuatorWorld.GetTile(25, 20) != nonActuatorTile ||
    rejectedActuatorWorld.GetSectionVersion(rejectedActuatorWorld.GetSectionCoordinates(25, 20)) !=
    nonActuatorVersion)
{
  throw new InvalidOperationException(
    "A tile without an actuator bit must be rejected without world mutation.");
}

_ = rejectedActuatorWorld.TrySetTile(26, 20, actuatorTile);
_ = rejectedActuatorWorld.TrySetTile(26, 19, new WorldTile(IsActive: true, Type: 1));
IReadOnlyList<TileChangeCommand> activeAboveCommands = new ActuatorCommandSystem().CreateCommands(
  rejectedActuatorWorld,
  new ActuatorComponent(9, [new WiringTileCoordinate(26, 20)], tileType: 1),
  new MechanismActivationCommand(41, 9, MechanismActivationKind.Activate, 9),
  firstSequence: 41);
if (activeAboveCommands.Count != 0 || rejectedActuatorWorld.GetTile(26, 20) != actuatorTile)
{
  throw new InvalidOperationException(
    "An actuator requiring the unported active-above branch must be rejected without mutation.");
}

IReadOnlyList<TileChangeCommand> activeAboveEligibleCommands = new ActuatorCommandSystem().CreateCommands(
  rejectedActuatorWorld,
  new ActuatorComponent(11, [new WiringTileCoordinate(26, 20)], tileType: 1),
  new MechanismActivationCommand(43, 11, MechanismActivationKind.Activate, 11),
  firstSequence: 43,
  canKillTileQuery: (tileX, tileY) => tileX == 26 && tileY == 20);
if (activeAboveEligibleCommands.Count != 1 || !activeAboveEligibleCommands[0].IsInactive)
{
  throw new InvalidOperationException(
    "A source-eligible active-above actuator did not use the authoritative CanKillTile query.");
}

_ = rejectedActuatorWorld.TrySetTile(26, 19, new WorldTile(IsActive: true, Type: 21));
IReadOnlyList<TileChangeCommand> protectedAboveCommands = new ActuatorCommandSystem().CreateCommands(
  rejectedActuatorWorld,
  new ActuatorComponent(12, [new WiringTileCoordinate(26, 20)], tileType: 1),
  new MechanismActivationCommand(44, 12, MechanismActivationKind.Activate, 12),
  firstSequence: 44,
  canKillTileQuery: (_, _) => true);
if (protectedAboveCommands.Count != 0)
{
  throw new InvalidOperationException("PreventsActuationUnder did not reject the active-above path.");
}

_ = rejectedActuatorWorld.TrySetTile(27, 20, actuatorTile);
_ = rejectedActuatorWorld.TrySetTile(28, 20, nonActuatorTile);
IReadOnlyList<TileChangeCommand> mixedCommands = new ActuatorCommandSystem().CreateCommands(
  rejectedActuatorWorld,
  new ActuatorComponent(
    10,
    [new WiringTileCoordinate(27, 20), new WiringTileCoordinate(28, 20)],
    tileType: 1),
  new MechanismActivationCommand(42, 10, MechanismActivationKind.Activate, 10),
  firstSequence: 42);
if (mixedCommands.Count != 0 || rejectedActuatorWorld.GetTile(27, 20) != actuatorTile ||
    rejectedActuatorWorld.GetTile(28, 20) != nonActuatorTile)
{
  throw new InvalidOperationException(
    "A multi-tile actuator must reject atomically when any target is unsupported.");
}

WorldGrid type226World = new(400, 300);
WorldTile type226Tile = new(IsActive: true, Type: 226, IsActuated: true);
_ = type226World.TrySetTile(30, 200, type226Tile);
WorldMetadata type226Metadata = new(
  "Type 226",
  new WorldSeed(226),
  400,
  300,
  worldSurface: 100.0);
MechanismActivationCommand type226Activation = new(
  50,
  50,
  MechanismActivationKind.Activate,
  50);
IReadOnlyList<TileChangeCommand> type226BlockedCommands = new ActuatorCommandSystem().CreateCommands(
  type226World,
  new ActuatorComponent(50, [new WiringTileCoordinate(30, 200)], tileType: 226),
  type226Activation,
  firstSequence: 50,
  worldMetadata: type226Metadata,
  progression: new WorldProgressionState());
if (type226BlockedCommands.Count != 0)
{
  throw new InvalidOperationException(
    "Type 226 below world surface was enabled before Plantera.");
}

IReadOnlyList<TileChangeCommand> type226PlanteraCommands = new ActuatorCommandSystem().CreateCommands(
  type226World,
  new ActuatorComponent(51, [new WiringTileCoordinate(30, 200)], tileType: 226),
  type226Activation with { MechanismId = 51 },
  firstSequence: 51,
  worldMetadata: type226Metadata,
  progression: new WorldProgressionState(defeatedPlantera: true));
if (type226PlanteraCommands.Count != 1 || !type226PlanteraCommands[0].IsInactive)
{
  throw new InvalidOperationException(
    "Defeated Plantera did not permit the source-backed Type 226 actuator path.");
}

WorldGrid aboveSurfaceWorld = new(400, 300);
_ = aboveSurfaceWorld.TrySetTile(30, 50, type226Tile);
IReadOnlyList<TileChangeCommand> aboveSurfaceCommands = new ActuatorCommandSystem().CreateCommands(
  aboveSurfaceWorld,
  new ActuatorComponent(53, [new WiringTileCoordinate(30, 50)], tileType: 226),
  type226Activation with { MechanismId = 53 },
  firstSequence: 53,
  worldMetadata: type226Metadata,
  progression: new WorldProgressionState());
if (aboveSurfaceCommands.Count != 1 || !aboveSurfaceCommands[0].IsInactive)
{
  throw new InvalidOperationException(
    "Type 226 above world surface was rejected without a source reason.");
}

WorldGrid unknownSurfaceWorld = new(400, 300);
_ = unknownSurfaceWorld.TrySetTile(30, 200, type226Tile);
IReadOnlyList<TileChangeCommand> unknownSurfaceCommands = new ActuatorCommandSystem().CreateCommands(
  unknownSurfaceWorld,
  new ActuatorComponent(52, [new WiringTileCoordinate(30, 200)], tileType: 226),
  type226Activation with { MechanismId = 52 },
  firstSequence: 52,
  progression: new WorldProgressionState(defeatedPlantera: true));
if (unknownSurfaceCommands.Count != 0)
{
  throw new InvalidOperationException(
    "Type 226 accepted an unknown world surface instead of failing closed.");
}

foreach (ushort specialActuatorType in new ushort[] { 314, 379, 386, 387, 388, 389, 476 })
{
  if (!LegacyActuatorTileDefinitionRuleSystem.TryGet(
        specialActuatorType,
        out _,
        out _))
  {
    throw new InvalidOperationException(
      $"Legacy special actuator type {specialActuatorType} was not classified as explicit non-actuated.");
  }

  WorldGrid specialActuatorWorld = new(400, 300);
  _ = specialActuatorWorld.TrySetTile(
    35,
    100,
    new WorldTile(IsActive: true, Type: specialActuatorType, IsActuated: true));
  IReadOnlyList<TileChangeCommand> specialCommands = new ActuatorCommandSystem().CreateCommands(
    specialActuatorWorld,
    new ActuatorComponent(
      60 + specialActuatorType,
      [new WiringTileCoordinate(35, 100)],
      tileType: specialActuatorType),
    new MechanismActivationCommand(
      60 + specialActuatorType,
      60 + specialActuatorType,
      MechanismActivationKind.Activate,
      60 + specialActuatorType),
    firstSequence: 60 + specialActuatorType,
    worldMetadata: new WorldMetadata("Special actuator", new WorldSeed(314), 400, 300));
  if (specialCommands.Count != 0)
  {
    throw new InvalidOperationException(
      $"Legacy special actuator type {specialActuatorType} was incorrectly enabled.");
  }
}

LampComponent lamp = new(
  9,
  new WiringTileCoordinate(25, 20),
  litTileType: 6,
  unlitTileType: 7);
LampCommandSystem lampCommandSystem = new();
IReadOnlyList<TileChangeCommand> litLampCommands = lampCommandSystem.CreateCommands(
  lamp,
  new MechanismActivationCommand(32, 9, MechanismActivationKind.Activate, 7),
  firstSequence: 33);
bool litState = lamp.IsLit;
IReadOnlyList<TileChangeCommand> unlitLampCommands = lampCommandSystem.CreateCommands(
  lamp,
  new MechanismActivationCommand(34, 9, MechanismActivationKind.Close, 7),
  firstSequence: 35);
if (litLampCommands.Count != 1 || litLampCommands[0].Kind != TileChangeKind.Place ||
    litLampCommands[0].TileType != 6 || litLampCommands[0].X != 25 ||
    !litState || unlitLampCommands.Count != 1 || unlitLampCommands[0].TileType != 7 ||
    lamp.IsLit)
{
  throw new InvalidOperationException(
    "Lamp activation did not project explicit lit and unlit tiles.");
}

WorldGrid frameLampWorld = new(400, 300);
for (int row = 0; row < 2; row++)
{
  for (int column = 0; column < 2; column++)
  {
    _ = frameLampWorld.TrySetTile(
      30 + column,
      30 + row,
      new WorldTile(true, 95, FrameX: (short)(column * 18), FrameY: (short)(row * 18)));
  }
}

IReadOnlyList<TileFrameCommand> frameLampCommands = lampCommandSystem.CreateFrameCommands(
  frameLampWorld,
  LampDefinitionRegistry.SourceDerived,
  31,
  31,
  MechanismActivationKind.Activate,
  firstSequence: 40);
if (frameLampCommands.Count != 4 || frameLampCommands[0] !=
    new TileFrameCommand(40, 30, 30, 36, 0) || frameLampCommands[3] !=
    new TileFrameCommand(43, 31, 31, 54, 18))
{
  throw new InvalidOperationException(
    "Frame-backed 2x2 light did not derive its origin or emit deterministic frame commands.");
}

foreach ((ushort tileType, int width, int height) in new[]
         {
           ((ushort)4, 1, 1), ((ushort)42, 1, 2), ((ushort)93, 1, 3),
           ((ushort)92, 1, 6), ((ushort)95, 2, 2), ((ushort)34, 3, 3),
           ((ushort)405, 3, 2)
         })
{
  WorldGrid familyWorld = new(400, 300);
  for (int row = 0; row < height; row++)
  {
    for (int column = 0; column < width; column++)
    {
      _ = familyWorld.TrySetTile(
        70 + column,
        70 + row,
        new WorldTile(true, tileType, FrameX: (short)(column * 18), FrameY: (short)(row * 18)));
    }
  }

  if (!lampCommandSystem.TryCreateFrameCommands(
        familyWorld,
        LampDefinitionRegistry.SourceDerived,
        70 + width - 1,
        70 + height - 1,
        MechanismActivationKind.Activate,
        100,
        out IReadOnlyList<TileFrameCommand> familyCommands) ||
      familyCommands.Count != width * height)
  {
    throw new InvalidOperationException("A registered lamp footprint did not emit all frame commands.");
  }
}

WorldGrid invalidLampWorld = new(400, 300);
_ = invalidLampWorld.TrySetTile(80, 80, new WorldTile(true, 753));
if (lampCommandSystem.TryCreateFrameCommands(
      invalidLampWorld,
      LampDefinitionRegistry.SourceDerived,
      80,
      80,
      MechanismActivationKind.Toggle,
      200,
      out _))
{
  throw new InvalidOperationException("An unknown lamp Tile was accepted by the frame registry.");
}

PumpComponent pump = new(8, 20, 20, 21, 20, 16, 2);
LiquidTransferCommand? transfer = new PumpCommandSystem().CreateCommand(
  pump,
  new MechanismActivationCommand(40, 8, MechanismActivationKind.Activate, 7),
  sequence: 41,
  LiquidType.Water);
if (transfer is null || transfer.Value.Amount != 16 || transfer.Value.TargetX != 21)
{
  throw new InvalidOperationException("Pump activation did not produce a bounded liquid transfer command.");
}

WorldGrid world = new(400, 300);
if (!new WiringInputValidationSystem().TryValidate(
      world,
      network,
      new WiringInputCommand(50, new PlayerHandle(1), 10, 10, WireColor.Red),
      maximumRadius: 10) ||
    new WiringInputValidationSystem().TryValidate(
      world,
      network,
      new WiringInputCommand(51, new PlayerHandle(1), 99, 99, WireColor.Red),
      maximumRadius: 10))
{
  throw new InvalidOperationException("Wiring input validation accepted an invalid source.");
}

Console.WriteLine("PASS: bounded wire traversal, pressure plates, mechanisms, actuators and pumps are backed");

using DomeSimulation wiringSequenceSimulation = new(new WorldGrid(400, 300));
PlayerHandle wiringSequencePlayer = wiringSequenceSimulation.CreatePlayer(
  new SimulationVector(10.0f, 10.0f));
wiringSequenceSimulation.SetWireMask(10, 10, 1);
if (wiringSequenceSimulation.TryQueueWiringInput(
      new WiringInputCommand(long.MaxValue, wiringSequencePlayer, 10, 10, WireColor.Red)))
{
  throw new InvalidOperationException(
    "DomeSimulation accepted a wiring sequence that would overflow the next allocator.");
}

if (wiringSequenceSimulation.TryQueueWiringInput(
      new WiringInputCommand(long.MaxValue - 1, wiringSequencePlayer, 10, 10, WireColor.Red)))
{
  throw new InvalidOperationException(
    "DomeSimulation accepted the final sequence even though no successor sequence remains.");
}

Console.WriteLine("PASS: wiring input rejects terminal sequences before allocator overflow");

using DomeSimulation extractinatorSimulation = new(new WorldGrid(400, 300));
const int extractinatorX = 100;
const int extractinatorY = 100;
_ = extractinatorSimulation.WorldGrid.TrySetTile(
  extractinatorX,
  extractinatorY,
  new WorldTile(
    IsActive: true,
    Type: ExtractinatorSystem.ExtractinatorTileType,
    FrameX: 18,
    FrameY: 36));
int firstChestId = extractinatorSimulation.CreateChest(100, 100);
int secondChestId = extractinatorSimulation.CreateChest(101, 100);
extractinatorSimulation.GetChest(firstChestId).SetSlot(39, new ItemStack(5395, 2));
extractinatorSimulation.GetChest(firstChestId).SetSlot(0, new ItemStack(5395, 2));
extractinatorSimulation.GetChest(secondChestId).SetSlot(39, new ItemStack(5395, 2));

extractinatorSimulation.QueueTriggerExtractinator(extractinatorX, extractinatorY);
extractinatorSimulation.Tick(new SimulationInputBatch());
if (extractinatorSimulation.GetChest(firstChestId).GetSlot(39) != new ItemStack(5395, 1) ||
    extractinatorSimulation.GetChest(firstChestId).GetSlot(0) != new ItemStack(5395, 2) ||
    extractinatorSimulation.GetChest(secondChestId).GetSlot(39) != new ItemStack(5395, 2) ||
    extractinatorSimulation.CreateWorldItemSnapshots().Count != 1 ||
    extractinatorSimulation.CreateWorldItemSnapshots()[0].Position != new SimulationVector(99, 98) ||
    extractinatorSimulation.CreateExtractinatorResultEvents().Single().SourceKind !=
      Terraria.Dome.Simulation.Items.Events.ExtractinatorSourceKind.Wiring)
{
  throw new InvalidOperationException(
    "Wiring Extractinator did not normalize frames, choose the first chest and commit world output.");
}

extractinatorSimulation.QueueTriggerExtractinator(extractinatorX, extractinatorY);
extractinatorSimulation.Tick(new SimulationInputBatch());
if (extractinatorSimulation.GetChest(firstChestId).GetSlot(39) != new ItemStack(5395, 1) ||
    extractinatorSimulation.CreateWorldItemSnapshots().Count != 1)
{
  throw new InvalidOperationException("Wiring Extractinator did not enforce its 60-tick cooldown.");
}

Console.WriteLine("PASS: Wiring Extractinator commits normalized chest input atomically");

using DomeSimulation lockedChestSimulation = new(new WorldGrid(400, 300));
_ = lockedChestSimulation.WorldGrid.TrySetTile(
  extractinatorX,
  extractinatorY,
  new WorldTile(IsActive: true, Type: ExtractinatorSystem.ExtractinatorTileType));
int lockedChestId = lockedChestSimulation.CreateChest(extractinatorX, extractinatorY);
lockedChestSimulation.GetChest(lockedChestId).SetSlot(39, new ItemStack(5395, 1));
lockedChestSimulation.GetChest(lockedChestId).SetLocked(true);
lockedChestSimulation.QueueTriggerExtractinator(extractinatorX, extractinatorY);
lockedChestSimulation.Tick(new SimulationInputBatch());
if (lockedChestSimulation.GetChest(lockedChestId).GetSlot(39) != new ItemStack(5395, 1) ||
    lockedChestSimulation.CreateWorldItemSnapshots().Count != 0 ||
    lockedChestSimulation.CreateExtractinatorResultEvents().Count != 0)
{
  throw new InvalidOperationException("Wiring Extractinator did not reject a locked chest atomically.");
}

Console.WriteLine("PASS: Wiring Extractinator rejects locked chest state without mutation");

if (!LegacyChestOriginQuery.TryGetOrigin(
      new WorldTile(IsActive: true, Type: 21, FrameX: 18, FrameY: 36),
      tileX: 20,
      tileY: 12,
      out int chestOriginX,
      out int chestOriginY) ||
    chestOriginX != 19 ||
    chestOriginY != 10)
{
  throw new InvalidOperationException("Type 21 chest frame did not project to its legacy origin.");
}

if (!LegacyChestOriginQuery.TryGetOrigin(
      new WorldTile(IsActive: true, Type: 467, FrameX: 54, FrameY: 18),
      tileX: 20,
      tileY: 12,
      out chestOriginX,
      out chestOriginY) ||
    chestOriginX != 19 ||
    chestOriginY != 11)
{
  throw new InvalidOperationException("Type 467 chest frame did not project to its legacy origin.");
}

if (!LegacyChestOriginQuery.TryGetOrigin(
      new WorldTile(IsActive: true, Type: 88, FrameX: 36, FrameY: 18),
      tileX: 20,
      tileY: 12,
      out chestOriginX,
      out chestOriginY) ||
    chestOriginX != 18 ||
    chestOriginY != 11)
{
  throw new InvalidOperationException("Type 88 dresser frame did not project to its legacy origin.");
}

foreach (WorldTile invalidChestTile in new[]
         {
           new WorldTile(IsActive: false, Type: 21, FrameX: 18, FrameY: 18),
           new WorldTile(IsActive: true, Type: 88, FrameX: 17, FrameY: 18),
           new WorldTile(IsActive: true, Type: 21, FrameX: -1, FrameY: 18),
           new WorldTile(IsActive: true, Type: 1, FrameX: 18, FrameY: 18)
         })
{
  if (LegacyChestOriginQuery.TryGetOrigin(
        invalidChestTile,
        tileX: 20,
        tileY: 12,
        out _,
        out _))
  {
    throw new InvalidOperationException("An unsupported or malformed chest tile was accepted.");
  }
}

Console.WriteLine("PASS: legacy chest origin query accepts only source-backed tile frames");

ChestIndexSystem chestIndexSystem = new();
Dictionary<int, ChestComponent> indexedChests = [];
ChestComponent originChest = new(chestId: 1, tileX: 19, tileY: 10);
indexedChests.Add(originChest.ChestId, originChest);
if (!chestIndexSystem.TryAdd(originChest) ||
    !LegacyChestDestructionQuery.TryGetIsChestBlocked(
      new WorldTile(IsActive: true, Type: 21, FrameX: 18, FrameY: 36),
      tileX: 20,
      tileY: 12,
      indexedChests,
      chestIndexSystem,
      out bool isChestBlocked) ||
    isChestBlocked)
{
  throw new InvalidOperationException("An empty chest at its projected legacy origin was blocked.");
}

originChest.SetSlot(slot: 0, new ItemStack(1, 1));
if (!LegacyChestDestructionQuery.TryGetIsChestBlocked(
      new WorldTile(IsActive: true, Type: 21, FrameX: 18, FrameY: 36),
      tileX: 20,
      tileY: 12,
      indexedChests,
      chestIndexSystem,
      out isChestBlocked) ||
    !isChestBlocked)
{
  throw new InvalidOperationException("A populated chest at its projected legacy origin was allowed.");
}

ChestComponent adjacentChest = new(chestId: 2, tileX: 20, tileY: 10);
indexedChests.Add(adjacentChest.ChestId, adjacentChest);
adjacentChest.SetSlot(slot: 0, new ItemStack(1, 1));
originChest.SetSlot(slot: 0, ItemStack.Empty);
if (!chestIndexSystem.TryAdd(adjacentChest) ||
    !LegacyChestDestructionQuery.TryGetIsChestBlocked(
      new WorldTile(IsActive: true, Type: 21, FrameX: 18, FrameY: 36),
      tileX: 20,
      tileY: 12,
      indexedChests,
      chestIndexSystem,
      out isChestBlocked) ||
    isChestBlocked ||
    LegacyChestDestructionQuery.TryGetIsChestBlocked(
      new WorldTile(IsActive: true, Type: 1),
      tileX: 20,
      tileY: 12,
      indexedChests,
      chestIndexSystem,
      out _))
{
  throw new InvalidOperationException("Chest protection did not use the exact legacy origin.");
}

Console.WriteLine("PASS: chest destruction projection uses the exact legacy origin and inventory");

WorldGrid canKillWorld = new(400, 300);
_ = canKillWorld.TrySetTile(50, 50, new WorldTile(IsActive: true, Type: 1));
if (!LegacyCanKillTileQuery.TryGetCanKill(
      canKillWorld,
      tileX: 50,
      tileY: 50,
      isHardMode: true,
      indexedChests,
      chestIndexSystem,
      out bool canKill) ||
    !canKill)
{
  throw new InvalidOperationException("An ordinary tile was not accepted by the composed CanKillTile query.");
}

_ = canKillWorld.TrySetTile(50, 49, new WorldTile(IsActive: true, Type: 5, FrameY: 0));
if (!LegacyCanKillTileQuery.TryGetCanKill(
      canKillWorld,
      tileX: 50,
      tileY: 50,
      isHardMode: true,
      indexedChests,
      chestIndexSystem,
      out canKill) ||
    canKill)
{
  throw new InvalidOperationException("A protected active-above tree was allowed by CanKillTile.");
}

_ = canKillWorld.TrySetTile(50, 49, default);
_ = canKillWorld.TrySetTile(51, 50, new WorldTile(IsActive: true, Type: 10, FrameX: 0, FrameY: 594));
if (!LegacyCanKillTileQuery.TryGetCanKill(
      canKillWorld,
      tileX: 51,
      tileY: 50,
      isHardMode: true,
      indexedChests,
      chestIndexSystem,
      out canKill) ||
    canKill ||
    LegacyCanKillTileQuery.TryGetCanKill(
      canKillWorld,
      tileX: -1,
      tileY: 50,
      isHardMode: true,
      indexedChests,
      chestIndexSystem,
      out _))
{
  throw new InvalidOperationException("CanKillTile did not retain locked-door or bounds guards.");
}
