namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeMarkerIdentityComponent
{
  public RevengeMarkerIdentityComponent()
    : this(RevengeMarkerId.Unassigned)
  {
  }

  public RevengeMarkerIdentityComponent(RevengeMarkerId markerId)
  {
    MarkerId = markerId;
  }

  public RevengeMarkerId MarkerId { get; }

  public int LegacyId => MarkerId.Value;

  public bool IsAssigned => MarkerId.IsAssigned;
}
