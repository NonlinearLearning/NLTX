namespace Terraria.DeathPenaltyAndRevenge;

public readonly record struct RevengeMarkerIdentityProjection(RevengeMarkerId MarkerId)
{
  public int UniqueId => MarkerId.Value;
}
