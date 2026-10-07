namespace Terraria.Npc;

public readonly record struct NpcGuideDangerNpcSnapshot(
  int EntityId,
  bool Active,
  bool CritterThatCanTurnOnPlayers,
  int TypeId,
  bool Friendly,
  int Damage,
  bool Stinky,
  bool NoTileCollide,
  bool IsSelf,
  bool CanBeChasedBy,
  float Distance,
  float CenterX,
  bool CanHit);

public readonly record struct NpcGuideDangerPlayerSnapshot(
  int EntityId,
  bool Active,
  bool Dead,
  bool Stinky,
  float Distance,
  float CenterX);

public readonly record struct NpcGuideDangerScanInput(
  int TypeId,
  int NetId,
  int AiStyle,
  float CenterX,
  bool PlayerTalking,
  bool AttackTypeRequiresExtendedRange,
  float DangerDetectRange,
  IReadOnlyList<NpcGuideDangerNpcSnapshot> Npcs,
  IReadOnlyList<NpcGuideDangerPlayerSnapshot> Players);

[Flags]
public enum NpcGuideDangerScanBranch
{
  None = 0,
  NpcDanger = 1 << 0,
  NpcWithinBaseRange = 1 << 1,
  NpcStinky = 1 << 2,
  PlayerDanger = 1 << 3,
  PlayerStinky = 1 << 4,
}

public readonly record struct NpcGuideDangerScanResult(
  bool DangerDetected,
  bool DangerWithinBaseRange,
  bool StinkyDanger,
  float LeftNearestOffset,
  float RightNearestOffset,
  int LeftChaseEntityId,
  int RightChaseEntityId,
  float EffectiveDangerRange,
  float BaseDangerRange,
  NpcGuideDangerScanBranch Branches);

/// <summary>
/// Pure snapshot scan for the source danger-selection prelude in
/// AI_007_TownEntities. Entity stores and collision checks remain caller-owned.
/// </summary>
public static class NpcGuideDangerScan
{
  public static NpcGuideDangerScanResult Evaluate(
    in NpcGuideDangerScanInput input)
  {
    if (!NpcGuideSourceProfile.CanHandle(input.TypeId, input.NetId, input.AiStyle))
    {
      throw new InvalidOperationException(
        "Guide danger scan requires type=22, netID=22, and aiStyle=7.");
    }

    float baseRange = input.DangerDetectRange == -1f
      ? 200f
      : input.DangerDetectRange;
    float effectiveRange = input.AttackTypeRequiresExtendedRange
      ? MathF.Max(baseRange, 250f)
      : baseRange;
    bool dangerDetected = false;
    bool dangerWithinBaseRange = false;
    bool stinkyDanger = false;
    float leftNearestOffset = -1f;
    float rightNearestOffset = -1f;
    int leftChaseEntityId = -1;
    int rightChaseEntityId = -1;
    NpcGuideDangerScanBranch branches = NpcGuideDangerScanBranch.None;

    foreach (NpcGuideDangerNpcSnapshot npc in input.Npcs)
    {
      if (!npc.Active ||
          npc.CritterThatCanTurnOnPlayers ||
          npc.TypeId == 690 ||
          (npc.Friendly && !npc.Stinky) ||
          npc.Damage <= 0 && !npc.Stinky ||
          npc.IsSelf ||
          npc.Distance >= effectiveRange ||
          !npc.NoTileCollide && !npc.CanHit)
      {
        continue;
      }

      dangerDetected = true;
      branches |= NpcGuideDangerScanBranch.NpcDanger;
      if (npc.Distance >= baseRange)
      {
        continue;
      }

      dangerWithinBaseRange = true;
      branches |= NpcGuideDangerScanBranch.NpcWithinBaseRange;
      if (npc.Stinky)
      {
        stinkyDanger = true;
        branches |= NpcGuideDangerScanBranch.NpcStinky;
      }

      float offset = npc.CenterX - input.CenterX;
      if (offset < 0f &&
          (leftNearestOffset == -1f || offset > leftNearestOffset))
      {
        leftNearestOffset = offset;
        if (npc.CanBeChasedBy)
        {
          leftChaseEntityId = npc.EntityId;
        }
      }

      if (offset > 0f &&
          (rightNearestOffset == -1f || offset < rightNearestOffset))
      {
        rightNearestOffset = offset;
        if (npc.CanBeChasedBy)
        {
          rightChaseEntityId = npc.EntityId;
        }
      }
    }

    if (!dangerWithinBaseRange && !input.PlayerTalking)
    {
      foreach (NpcGuideDangerPlayerSnapshot player in input.Players)
      {
        if (!player.Active ||
            player.Dead ||
            !player.Stinky ||
            player.Distance >= baseRange)
        {
          continue;
        }

        dangerWithinBaseRange = true;
        stinkyDanger = true;
        branches |= NpcGuideDangerScanBranch.PlayerDanger |
          NpcGuideDangerScanBranch.PlayerStinky;
        float offset = player.CenterX - input.CenterX;
        if (offset < 0f &&
            (leftNearestOffset == -1f || offset > leftNearestOffset))
        {
          leftNearestOffset = offset;
          leftChaseEntityId = player.EntityId;
        }

        if (offset > 0f &&
            (rightNearestOffset == -1f || offset < rightNearestOffset))
        {
          rightNearestOffset = offset;
          rightChaseEntityId = player.EntityId;
        }
      }
    }

    return new NpcGuideDangerScanResult(
      dangerDetected,
      dangerWithinBaseRange,
      stinkyDanger,
      leftNearestOffset,
      rightNearestOffset,
      leftChaseEntityId,
      rightChaseEntityId,
      effectiveRange,
      baseRange,
      branches);
  }
}
