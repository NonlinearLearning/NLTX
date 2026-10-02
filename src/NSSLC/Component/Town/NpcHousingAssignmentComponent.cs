namespace Terraria.Town;

public sealed class NpcHousingAssignmentComponent
{
  public NpcHousingAssignmentComponent(
    TownHousingStatus status,
    TownRoomTilePoint? homeTile,
    int searchCooldownTicks,
    bool despawnWhenHomeless,
    uint assignmentRevision)
  {
    Status = status;
    HomeTile = homeTile;
    SearchCooldownTicks = searchCooldownTicks;
    DespawnWhenHomeless = despawnWhenHomeless;
    AssignmentRevision = assignmentRevision;
  }

  public TownHousingStatus Status { get; }

  public TownRoomTilePoint? HomeTile { get; }

  public int SearchCooldownTicks { get; }

  public bool DespawnWhenHomeless { get; }

  public uint AssignmentRevision { get; }

  public bool IsHomeless => Status == TownHousingStatus.Homeless;

  public bool HasHome => HomeTile.HasValue && !IsHomeless;

  public bool IsEligibleForSearch =>
    (Status is TownHousingStatus.Homeless or TownHousingStatus.LookingForHome) &&
    SearchCooldownTicks == 0;
}
