using System.Collections.Frozen;
using Terraria.Relationships;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.PressurePlates;

public sealed class PressurePlateOccupancyComponent
{
  private readonly Dictionary<TileCoordinate, HashSet<EntityReference>> _occupantsByPlate = new();

  public IReadOnlyDictionary<TileCoordinate, IReadOnlySet<EntityReference>> OccupantsByPlate
  {
    get
    {
      var occupants = new Dictionary<TileCoordinate, IReadOnlySet<EntityReference>>();
      foreach ((TileCoordinate coordinate, HashSet<EntityReference> occupantsAtPlate) in _occupantsByPlate)
      {
        occupants.Add(coordinate, occupantsAtPlate.ToFrozenSet());
      }

      return occupants.ToFrozenDictionary();
    }
  }

  public bool NeedsFirstUpdate { get; internal set; }
}
