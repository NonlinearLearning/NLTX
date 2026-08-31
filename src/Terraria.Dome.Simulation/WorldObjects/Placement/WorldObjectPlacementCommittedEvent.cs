using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;

namespace Terraria.Dome.Simulation.WorldObjects.Placement;

public sealed record WorldObjectPlacementCommittedEvent(
  long Sequence,
  WorldObjectPlacementRequest Request,
  IReadOnlyList<WorldObjectTileMutation> Footprint,
  SignSnapshot? Sign,
  IReadOnlyList<WorldSectionCoordinates> Sections,
  ProjectileTombstoneReason ProjectileTombstoneReason = ProjectileTombstoneReason.Expired)
{
  public long SectionVersion { get; init; }

  public IReadOnlyDictionary<WorldSectionCoordinates, long> SectionVersions { get; init; } =
    new Dictionary<WorldSectionCoordinates, long>();

  public int ProjectileIdentity { get; init; } = -1;

  public Guid? ProjectileUuid { get; init; }

  public WorldObjectPlacementCommittedEvent(
    WorldObjectPlacementPlan plan,
    SignSnapshot? sign,
    IReadOnlyList<WorldSectionCoordinates> sections)
    : this(plan.Request.Sequence, plan.Request, plan.Footprint, sign, sections)
  {
    ArgumentNullException.ThrowIfNull(plan);
  }
}
