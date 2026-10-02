namespace Terraria.NpcTownBestiary;

public sealed class TownRoomAvailabilityProjection
{
  public TownRoomAvailabilityProjection(
    ulong sourceRevision,
    IReadOnlySet<int> residentsWithRooms)
  {
    SourceRevision = sourceRevision;
    ResidentsWithRooms = residentsWithRooms.ToHashSet();
  }

  public ulong SourceRevision { get; }

  public IReadOnlySet<int> ResidentsWithRooms { get; }

  public static TownRoomAvailabilityProjection Create(TownRoomRegistryComponent registry)
  {
    ArgumentNullException.ThrowIfNull(registry);
    return new TownRoomAvailabilityProjection(
      registry.Revision,
      registry.RoomsByNpcType.Keys.ToHashSet());
  }
}
