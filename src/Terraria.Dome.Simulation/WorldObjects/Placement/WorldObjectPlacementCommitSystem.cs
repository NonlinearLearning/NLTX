using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldObjects.Placement;

public sealed class WorldObjectPlacementCommitSystem
{
  private readonly TileDefinitionRegistry _tileDefinitions;
  private readonly WorldObjectPlacementDefinitionRegistry _definitions;

  public WorldObjectPlacementCommitSystem(
    WorldObjectPlacementDefinitionRegistry? definitions = null,
    TileDefinitionRegistry? tileDefinitions = null)
  {
    _definitions = definitions ?? WorldObjectPlacementDefinitionRegistry.CreateVersion4Base();
    _tileDefinitions = tileDefinitions ?? TileDefinitionRegistry.CreateVersion4Base();
  }

  public WorldObjectPlacementResult Commit(
    WorldGrid world,
    WorldObjectPlacementPlan plan,
    bool ownerActive,
    bool projectileActive,
    ISet<long>? committedSequences = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(plan);
    WorldObjectPlacementFailureCode validation = plan.Request.Validate(world, _definitions);
    if (validation != WorldObjectPlacementFailureCode.None)
    {
      return WorldObjectPlacementResult.Rejected(
        plan.Request.Sequence,
        validation);
    }

    if (!ownerActive)
    {
      return WorldObjectPlacementResult.Rejected(
        plan.Request.Sequence,
        WorldObjectPlacementFailureCode.OwnerInactive);
    }

    if (!projectileActive)
    {
      return WorldObjectPlacementResult.Rejected(
        plan.Request.Sequence,
        WorldObjectPlacementFailureCode.ProjectileInactive);
    }

    if (committedSequences?.Contains(plan.Request.Sequence) == true)
    {
      return WorldObjectPlacementResult.Rejected(
        plan.Request.Sequence,
        WorldObjectPlacementFailureCode.DuplicateSequence);
    }

    if (!_definitions.TryGet(
          plan.Request.ObjectType,
          out WorldObjectPlacementDefinition definition))
    {
      return WorldObjectPlacementResult.Rejected(
        plan.Request.Sequence,
        WorldObjectPlacementFailureCode.UnsupportedObject);
    }

    WorldObjectPlacementFailureCode footprintValidation = ValidateFootprint(
      world,
      plan,
      definition);
    if (footprintValidation != WorldObjectPlacementFailureCode.None)
    {
      return WorldObjectPlacementResult.Rejected(
        plan.Request.Sequence,
        footprintValidation);
    }

    foreach (KeyValuePair<WorldSectionCoordinates, long> expected in plan.SectionVersions)
    {
      if (world.GetSectionVersion(expected.Key) != expected.Value)
      {
        return WorldObjectPlacementResult.Rejected(
          plan.Request.Sequence,
          WorldObjectPlacementFailureCode.VersionConflict);
      }
    }

    if (!definition.HasValidAnchor(
          world,
          plan.Request.OriginX,
          plan.Request.OriginY,
          _tileDefinitions))
    {
      return WorldObjectPlacementResult.Rejected(
        plan.Request.Sequence,
        WorldObjectPlacementFailureCode.MissingSupport);
    }

    if (plan.Request.ExpectedSectionVersion is long expectedOriginVersion)
    {
      WorldSectionCoordinates originSection =
        world.GetSectionCoordinates(plan.Request.OriginX, plan.Request.OriginY);
      if (world.GetSectionVersion(originSection) != expectedOriginVersion)
      {
        return WorldObjectPlacementResult.Rejected(
          plan.Request.Sequence,
          WorldObjectPlacementFailureCode.VersionConflict);
      }
    }

    for (int index = 0; index < plan.Footprint.Count; index++)
    {
      WorldObjectTileMutation mutation = plan.Footprint[index];
      if (world.GetTile(mutation.X, mutation.Y) != mutation.Expected)
      {
        return WorldObjectPlacementResult.Rejected(
          plan.Request.Sequence,
          WorldObjectPlacementFailureCode.OccupiedTile);
      }
    }

    Dictionary<WorldSectionCoordinates, int> sectionDeltas = new();
    HashSet<(int X, int Y)> coordinates = new();
    for (int index = 0; index < plan.Footprint.Count; index++)
    {
      WorldObjectTileMutation mutation = plan.Footprint[index];
      if (!coordinates.Add((mutation.X, mutation.Y)))
      {
        return WorldObjectPlacementResult.Rejected(
          plan.Request.Sequence,
          WorldObjectPlacementFailureCode.InvalidFootprint);
      }

      WorldSectionCoordinates section = world.GetSectionCoordinates(mutation.X, mutation.Y);
      if (!plan.SectionVersions.ContainsKey(section))
      {
        return WorldObjectPlacementResult.Rejected(
          plan.Request.Sequence,
          WorldObjectPlacementFailureCode.VersionConflict);
      }

      sectionDeltas[section] = sectionDeltas.TryGetValue(section, out int delta)
        ? checked(delta + 1)
        : 1;
    }

    foreach (KeyValuePair<WorldSectionCoordinates, int> delta in sectionDeltas)
    {
      long version = world.GetSectionVersion(delta.Key);
      if (version > long.MaxValue - delta.Value)
      {
        return WorldObjectPlacementResult.Rejected(
          plan.Request.Sequence,
          WorldObjectPlacementFailureCode.SectionVersionOverflow);
      }
    }

    for (int index = 0; index < plan.Footprint.Count; index++)
    {
      WorldObjectTileMutation mutation = plan.Footprint[index];
      if (!world.TrySetTile(mutation.X, mutation.Y, mutation.Replacement))
      {
        return WorldObjectPlacementResult.Rejected(
          plan.Request.Sequence,
          WorldObjectPlacementFailureCode.SectionVersionOverflow);
      }
    }

    committedSequences?.Add(plan.Request.Sequence);
    WorldSectionCoordinates[] sections = plan.Footprint
      .Select(mutation => world.GetSectionCoordinates(mutation.X, mutation.Y))
      .Distinct()
      .OrderBy(section => section.Y)
      .ThenBy(section => section.X)
      .ToArray();
    return new(WorldObjectPlacementStatus.Committed, WorldObjectPlacementFailureCode.None,
      plan.Request.Sequence, sections);
  }

  private static WorldObjectPlacementFailureCode ValidateFootprint(
    WorldGrid world,
    WorldObjectPlacementPlan plan,
    WorldObjectPlacementDefinition definition)
  {
    int expectedCount;
    try
    {
      expectedCount = checked(definition.Width * definition.Height);
    }
    catch (OverflowException)
    {
      return WorldObjectPlacementFailureCode.InvalidFootprint;
    }

    if (plan.Footprint.Count != expectedCount)
    {
      return WorldObjectPlacementFailureCode.InvalidFootprint;
    }

    HashSet<(int X, int Y)> coordinates = new();
    for (int index = 0; index < plan.Footprint.Count; index++)
    {
      WorldObjectTileMutation mutation = plan.Footprint[index];
      if (!mutation.IsValid || !world.Contains(mutation.X, mutation.Y))
      {
        return WorldObjectPlacementFailureCode.InvalidFootprint;
      }

      int column = mutation.X - plan.Request.OriginX;
      int row = mutation.Y - plan.Request.OriginY;
      if (column < 0 || column >= definition.Width || row < 0 || row >= definition.Height ||
          !coordinates.Add((mutation.X, mutation.Y)))
      {
        return WorldObjectPlacementFailureCode.InvalidFootprint;
      }

      if (mutation.Expected.IsActive)
      {
        return WorldObjectPlacementFailureCode.OccupiedTile;
      }

      if (!mutation.Replacement.IsActive || mutation.Replacement.IsInactive ||
          mutation.Replacement.Type != plan.Request.ObjectType)
      {
        return WorldObjectPlacementFailureCode.InvalidFootprint;
      }

      if (!definition.TryGetFrame(
            plan.Request.Style,
            column,
            row,
            out short frameX,
            out short frameY) ||
          mutation.Replacement.FrameX != frameX || mutation.Replacement.FrameY != frameY)
      {
        return WorldObjectPlacementFailureCode.InvalidFrame;
      }
    }

    return WorldObjectPlacementFailureCode.None;
  }
}
