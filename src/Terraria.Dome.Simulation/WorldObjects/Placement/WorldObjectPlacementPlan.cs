using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Arch.Core;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects.Placement;

public sealed class WorldObjectPlacementPlan
{
  public WorldObjectPlacementPlan(
    WorldObjectPlacementRequest request,
    IReadOnlyList<WorldObjectTileMutation> footprint,
    IReadOnlyDictionary<WorldSectionCoordinates, long> sectionVersions)
  {
    ArgumentNullException.ThrowIfNull(footprint);
    ArgumentNullException.ThrowIfNull(sectionVersions);
    Request = request;
    Footprint = Array.AsReadOnly(
      footprint.OrderBy(mutation => mutation.Y).ThenBy(mutation => mutation.X).ToArray());
    SectionVersions = new ReadOnlyDictionary<WorldSectionCoordinates, long>(
      new Dictionary<WorldSectionCoordinates, long>(sectionVersions));
  }

  public WorldObjectPlacementRequest Request { get; }
  public Entity Projectile => Request.Projectile;
  public IReadOnlyList<WorldObjectTileMutation> Footprint { get; }
  public IReadOnlyDictionary<WorldSectionCoordinates, long> SectionVersions { get; }
}
