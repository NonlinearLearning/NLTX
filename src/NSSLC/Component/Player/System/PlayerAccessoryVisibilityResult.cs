namespace Terraria.Player;

public readonly record struct PlayerAccessoryVisibilityResult(
  bool Applied,
  PlayerAccessoryVisibilitySnapshot Snapshot,
  PlayerAccessoryVisibilityRejectionReason RejectionReason)
{
  public static PlayerAccessoryVisibilityResult Rejected(
    PlayerAccessoryVisibilitySnapshot snapshot,
    PlayerAccessoryVisibilityRejectionReason rejectionReason)
  {
    return new PlayerAccessoryVisibilityResult(
      Applied: false,
      Snapshot: snapshot,
      RejectionReason: rejectionReason);
  }
}
