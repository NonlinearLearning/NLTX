using Terraria.Items;

namespace Terraria.DeathPenaltyAndRevenge;

public static class RevengeEnemyContextPolicy
{
  public const float EnemyBoxWidth = 2160.0f;
  public const float EnemyBoxHeight = 1440.0f;
  public const float WorldCaptureLeftMargin = 656.0f;
  public const float WorldCaptureTopMargin = 656.0f;
  public const float WorldCaptureRightMargin = 672.0f;
  public const float WorldCaptureBottomMargin = 672.0f;

  public static RevengeProximityBox CreateEnemyHitbox(WorldPosition location)
  {
    return RevengeProximityBox.CenteredAt(location, EnemyBoxWidth, EnemyBoxHeight);
  }

  public static RevengeEnemyCaptureResult EvaluateCapture(
    in RevengeEnemyCaptureInput input)
  {
    if (input.IsBoss)
    {
      return Rejected(RevengeEnemyCaptureRejectionReason.Boss);
    }

    if (input.HasNonRootRealLife)
    {
      return Rejected(RevengeEnemyCaptureRejectionReason.NonRootRealLife);
    }

    if (input.Rarity > 0)
    {
      return Rejected(RevengeEnemyCaptureRejectionReason.PositiveRarity);
    }

    if (!RevengeCachePolicy.CanCache(input.CoinValue))
    {
      return Rejected(RevengeEnemyCaptureRejectionReason.InsufficientCoins);
    }

    if (!IsInsideWorldCaptureBounds(input))
    {
      return Rejected(RevengeEnemyCaptureRejectionReason.OutsideWorldBounds);
    }

    if (input.SpawnNpcNetId.Value == 0)
    {
      return Rejected(RevengeEnemyCaptureRejectionReason.UnmappedSpawnNpc);
    }

    return new RevengeEnemyCaptureResult(
      CanCapture: true,
      RejectionReason: RevengeEnemyCaptureRejectionReason.None);
  }

  private static bool IsInsideWorldCaptureBounds(
    in RevengeEnemyCaptureInput input)
  {
    return input.Position.X >= input.WorldBounds.Left + WorldCaptureLeftMargin &&
      input.Position.X + input.Width <=
      input.WorldBounds.Right - WorldCaptureRightMargin &&
      input.Position.Y >= input.WorldBounds.Top + WorldCaptureTopMargin &&
      input.Position.Y <=
      input.WorldBounds.Bottom - WorldCaptureBottomMargin - input.Height;
  }

  private static RevengeEnemyCaptureResult Rejected(
    RevengeEnemyCaptureRejectionReason reason)
  {
    return new RevengeEnemyCaptureResult(
      CanCapture: false,
      RejectionReason: reason);
  }
}
