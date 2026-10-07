namespace Terraria.Npc;

public sealed class NpcTargetSelectionStateComponent
{
  public NpcTargetKind TargetKind { get; private set; }

  public int LegacyTargetIndex { get; private set; } = -1;

  public int SecondaryLegacySlot { get; private set; } = -1;

  public NpcTargetGeometrySnapshot TargetGeometry { get; private set; }

  public float Score { get; private set; } = float.PositiveInfinity;

  public int Direction { get; private set; }

  public int DirectionY { get; private set; }

  public bool NetUpdateRequested { get; private set; }

  public void Commit(in NpcTargetSelectionResult result)
  {
    TargetKind = result.TargetKind;
    LegacyTargetIndex = result.LegacyTargetIndex;
    SecondaryLegacySlot = result.SecondaryLegacySlot;
    TargetGeometry = result.TargetGeometry;
    Score = result.Score;
    Direction = result.Direction;
    DirectionY = result.DirectionY;
    NetUpdateRequested = result.NetUpdateRequested;
  }

  internal bool ResetForTermination()
  {
    bool changed = TargetKind != NpcTargetKind.None ||
      LegacyTargetIndex != -1 ||
      SecondaryLegacySlot != -1 ||
      TargetGeometry != default ||
      !float.IsPositiveInfinity(Score) ||
      Direction != 0 ||
      DirectionY != 0 ||
      NetUpdateRequested;

    TargetKind = NpcTargetKind.None;
    LegacyTargetIndex = -1;
    SecondaryLegacySlot = -1;
    TargetGeometry = default;
    Score = float.PositiveInfinity;
    Direction = 0;
    DirectionY = 0;
    NetUpdateRequested = false;
    return changed;
  }
}
