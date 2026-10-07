using System.Numerics;

namespace Terraria.Npc;

public static class NpcTargetSelectionSystem
{
  private const float NoTargetScore = 9999999f;

  public static NpcTargetSelectionResult Select(
    in NpcTargetSelectionInputs inputs)
  {
    ArgumentNullException.ThrowIfNull(inputs.Players);
    ArgumentNullException.ThrowIfNull(inputs.Npcs);

    return inputs.Strategy switch
    {
      NpcTargetSelectionStrategy.Normal => SelectNormal(in inputs, requireGross: false),
      NpcTargetSelectionStrategy.WallOfFlesh => SelectNormal(in inputs, requireGross: true),
      NpcTargetSelectionStrategy.Upgraded => SelectUpgraded(in inputs),
      _ => throw new ArgumentOutOfRangeException(nameof(inputs), inputs.Strategy, null),
    };
  }

  public static NpcTargetSelectionResult SelectAndCommit(
    in NpcTargetSelectionInputs inputs,
    NpcTargetSelectionStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    NpcTargetSelectionResult result = Select(in inputs);
    if (result.ShouldCommit)
    {
      state.Commit(in result);
    }
    return result;
  }

  public static bool ResetForTermination(NpcTargetSelectionStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return state.ResetForTermination();
  }

  private static NpcTargetSelectionResult SelectNormal(
    in NpcTargetSelectionInputs inputs,
    bool requireGross)
  {
    bool found = false;
    float distance = 0f;
    int tankPetSlot = -1;
    NpcPlayerTargetSnapshot? targetPlayer = null;
    NpcTankPetTargetSnapshot? targetPet = null;
    float selectedScore = 0f;

    foreach (NpcPlayerTargetSnapshot player in inputs.Players)
    {
      if (!player.IsSelectable || (requireGross && !player.Gross))
      {
        continue;
      }

      float real = LegacyTargetDistance(inputs.Source, player.Geometry);
      float score = real - player.Aggro;
      if (player.NoAggro && inputs.Direction != 0)
      {
        score += 1000f;
      }

      if (!found || score < distance)
      {
        found = true;
        tankPetSlot = -1;
        targetPlayer = player;
        targetPet = null;
        distance = score;
        selectedScore = score;
      }

      if (player.TankPet is NpcTankPetTargetSnapshot pet &&
          !player.NoAggro &&
          pet.CanHit is true)
      {
        float petScore = LegacyTargetDistance(inputs.Source, pet.Geometry) - 200f;
        if (petScore < distance && petScore < 200f)
        {
          tankPetSlot = pet.ProjectileSlot;
          targetPet = pet;
          selectedScore = petScore;
        }
      }
    }

    if (!found || targetPlayer is not NpcPlayerTargetSnapshot selectedPlayer)
    {
      return SelectNormalFallback(in inputs);
    }

    if (tankPetSlot >= 0 && targetPet is NpcTankPetTargetSnapshot selectedPet)
    {
      (int direction, int directionY) = ResolveLegacyDirection(
        inputs.Source,
        TruncateTargetRectCoordinates(selectedPet.Geometry));
      if (inputs.Confused)
      {
        direction *= -1;
      }
      return CreateResult(
        NpcTargetKind.PlayerTankPet,
        selectedPlayer.Slot,
        selectedPet.ProjectileSlot,
        TruncateTargetRectCoordinates(selectedPet.Geometry),
        selectedScore,
        direction,
        directionY,
        in inputs,
        requestNetUpdate: true);
    }

    NpcTargetGeometrySnapshot targetGeometry = selectedPlayer.Geometry;
    bool faceTarget = inputs.FaceTarget;
    if (selectedPlayer.IsDead || (selectedPlayer.NoAggro && inputs.Direction != 0))
    {
      faceTarget = false;
    }

    int directionResult = inputs.Direction;
    int directionYResult = inputs.DirectionY;
    if (faceTarget)
    {
      bool keepDirection =
        selectedPlayer.ItemAnimation == 0 &&
        selectedPlayer.Aggro < 0 &&
        inputs.OldTarget >= 0 &&
        inputs.OldTarget <= 254 &&
        !inputs.Boss;
      if (!keepDirection)
      {
        (directionResult, directionYResult) = ResolveLegacyDirection(
          inputs.Source,
          TruncateTargetRectCoordinates(targetGeometry));
      }
    }

    if (inputs.Confused)
    {
      directionResult *= -1;
    }

    return CreateResult(
      NpcTargetKind.Player,
      selectedPlayer.Slot,
      -1,
      TruncateTargetRectCoordinates(targetGeometry),
      selectedScore,
      directionResult,
      directionYResult,
      in inputs,
      requestNetUpdate: true);
  }

  private static NpcTargetSelectionResult SelectUpgraded(
    in NpcTargetSelectionInputs inputs)
  {
    Vector2 distanceOrigin = GetUpgradedDistanceOrigin(in inputs);
    int tankPetSlot = -1;
    float bestScore = NoTargetScore;
    NpcTargetGeometrySnapshot targetGeometry = default;
    NpcTargetKind targetKind = NpcTargetKind.None;
    NpcPlayerTargetSnapshot? selectedPlayer = null;

    bool directionIsZero = inputs.Direction == 0;
    foreach (NpcPlayerTargetSnapshot player in inputs.Players)
    {
      if (!player.IsSelectable)
      {
        continue;
      }

      float score = Vector2.Distance(distanceOrigin, player.Geometry.Center) - player.Aggro;
      if (player.NoAggro && !directionIsZero)
      {
        score += 1000f;
      }

      if (score < bestScore)
      {
        tankPetSlot = -1;
        targetGeometry = player.Geometry;
        targetKind = NpcTargetKind.Player;
        bestScore = score;
        selectedPlayer = player;
      }

      if (player.TankPet is NpcTankPetTargetSnapshot pet &&
          !player.NoAggro)
      {
        float petScore = Vector2.Distance(distanceOrigin, pet.Geometry.Center) - 200f;
        if (petScore < bestScore && petScore < 200f && pet.CanHit is true)
        {
          tankPetSlot = pet.ProjectileSlot;
          targetGeometry = pet.Geometry;
          targetKind = NpcTargetKind.PlayerTankPet;
          bestScore = petScore;
          selectedPlayer = player;
        }
      }
    }

    int npcSlot = -1;
    foreach (NpcNpcTargetSnapshot npc in inputs.Npcs)
    {
      if (!npc.IsSelectable)
      {
        continue;
      }

      float score = Vector2.Distance(distanceOrigin, npc.Geometry.Center);
      if (bestScore > score)
      {
        npcSlot = npc.Slot;
        tankPetSlot = -1;
        targetGeometry = npc.Geometry;
        targetKind = NpcTargetKind.Npc;
        bestScore = score;
        selectedPlayer = null;
      }
    }

    if (targetKind == NpcTargetKind.None)
    {
      return NpcTargetSelectionResult.NoTarget(
        inputs.Direction,
        inputs.DirectionY,
        shouldCommit: false);
    }

    if (targetKind == NpcTargetKind.Npc)
    {
      (int direction, int directionY) = ResolveUpgradedDirection(
        inputs.Source.Center,
        TruncateTargetRectCoordinates(targetGeometry));
      return CreateResult(
        NpcTargetKind.Npc,
        checked(npcSlot + 300),
        -1,
        TruncateTargetRectCoordinates(targetGeometry),
        bestScore,
        direction,
        directionY,
        in inputs,
        requestNetUpdate: false);
    }

    if (targetKind == NpcTargetKind.PlayerTankPet)
    {
      (int direction, int directionY) = ResolveUpgradedDirection(
        inputs.Source.Center,
        TruncateTargetRectCoordinates(targetGeometry));
      int ownerSlot = selectedPlayer!.Value.TankPet!.Value.OwnerSlot;
      if (ownerSlot < 0)
      {
        ownerSlot = selectedPlayer.Value.Slot;
      }
      return CreateResult(
        NpcTargetKind.PlayerTankPet,
        ownerSlot,
        tankPetSlot,
        TruncateTargetRectCoordinates(targetGeometry),
        bestScore,
        direction,
        directionY,
        in inputs,
        requestNetUpdate: false);
    }

    NpcPlayerTargetSnapshot playerTarget = selectedPlayer!.Value;
    bool faceTarget = inputs.FaceTarget;
    if (playerTarget.IsDead || (playerTarget.NoAggro && !directionIsZero))
    {
      faceTarget = false;
    }

    int directionResult = inputs.Direction;
    int directionYResult = inputs.DirectionY;
    if (faceTarget)
    {
      float facingThreshold =
        (playerTarget.Geometry.Width + playerTarget.Geometry.Height +
         inputs.Source.Width + inputs.Source.Height) / 4f + 800f;
      float adjustedScore = bestScore - playerTarget.Aggro;
      bool keepDirection =
        playerTarget.ItemAnimation == 0 &&
        playerTarget.Aggro < 0 &&
        adjustedScore > facingThreshold &&
        inputs.OldTarget >= 0 &&
        inputs.OldTarget <= 254;
      if (!keepDirection)
      {
        (directionResult, directionYResult) = ResolveUpgradedDirection(
          inputs.Source.Center,
          TruncateTargetRectCoordinates(targetGeometry));
      }
    }

    return CreateResult(
      NpcTargetKind.Player,
      playerTarget.Slot,
      -1,
      TruncateTargetRectCoordinates(targetGeometry),
      bestScore,
      directionResult,
      directionYResult,
      in inputs,
      requestNetUpdate: false);
  }

  private static NpcTargetSelectionResult CreateResult(
    NpcTargetKind targetKind,
    int legacyTargetIndex,
    int secondaryLegacySlot,
    NpcTargetGeometrySnapshot targetGeometry,
    float score,
    int direction,
    int directionY,
    in NpcTargetSelectionInputs inputs,
    bool requestNetUpdate)
  {
    bool directionChanged =
      direction != inputs.OldDirection || directionY != inputs.OldDirectionY ||
      legacyTargetIndex != inputs.OldTarget;
    return new NpcTargetSelectionResult(
      HasTarget: true,
      ShouldCommit: true,
      targetKind,
      legacyTargetIndex,
      secondaryLegacySlot,
      targetGeometry,
      score,
      direction,
      directionY,
      NetUpdateRequested:
        requestNetUpdate && !inputs.CollideX && !inputs.CollideY && directionChanged);
  }

  private static NpcTargetSelectionResult SelectNormalFallback(
    in NpcTargetSelectionInputs inputs)
  {
    int targetAtSelection = inputs.CurrentTarget == int.MinValue
      ? inputs.OldTarget
      : inputs.CurrentTarget;
    int fallbackSlot = targetAtSelection is >= 0 and <= 254
      ? targetAtSelection
      : 0;
    foreach (NpcPlayerTargetSnapshot player in inputs.Players)
    {
      if (player.Slot != fallbackSlot)
      {
        continue;
      }

      bool faceTarget = inputs.FaceTarget;
      if (player.IsDead || (player.NoAggro && inputs.Direction != 0))
      {
        faceTarget = false;
      }

      int direction = inputs.Direction;
      int directionY = inputs.DirectionY;
      if (faceTarget)
      {
        bool keepDirection =
          player.ItemAnimation == 0 &&
          player.Aggro < 0 &&
          inputs.OldTarget is >= 0 and <= 254 &&
          !inputs.Boss;
        if (!keepDirection)
        {
          (direction, directionY) = ResolveLegacyDirection(
            inputs.Source,
            TruncateTargetRectCoordinates(player.Geometry));
        }
      }

      if (inputs.Confused)
      {
        direction *= -1;
      }

      return CreateResult(
        NpcTargetKind.Player,
        fallbackSlot,
        -1,
        TruncateTargetRectCoordinates(player.Geometry),
        float.PositiveInfinity,
        direction,
        directionY,
        in inputs,
        requestNetUpdate: true);
    }

    return NpcTargetSelectionResult.NoTarget(
      inputs.Direction,
      inputs.DirectionY,
      shouldCommit: true);
  }

  private static Vector2 GetUpgradedDistanceOrigin(in NpcTargetSelectionInputs inputs)
  {
    return inputs.CheckPosition ?? inputs.Source.Center;
  }

  private static float LegacyTargetDistance(
    NpcTargetGeometrySnapshot source,
    NpcTargetGeometrySnapshot target)
  {
    float targetCenterX = target.Position.X + target.Width / 2;
    float targetCenterY = target.Position.Y + target.Height / 2;
    return MathF.Abs(targetCenterX - source.Position.X + source.Width / 2) +
      MathF.Abs(targetCenterY - source.Position.Y + source.Height / 2);
  }

  private static NpcTargetGeometrySnapshot TruncateTargetRectCoordinates(
    NpcTargetGeometrySnapshot geometry)
  {
    return geometry with
    {
      Position = new Vector2((int)geometry.Position.X, (int)geometry.Position.Y),
    };
  }

  private static (int Direction, int DirectionY) ResolveLegacyDirection(
    NpcTargetGeometrySnapshot sourceGeometry,
    NpcTargetGeometrySnapshot targetGeometry)
  {
    Vector2 targetCenter = new(
      targetGeometry.Position.X + targetGeometry.Width / 2,
      targetGeometry.Position.Y + targetGeometry.Height / 2);
    Vector2 sourceCenter = new(
      sourceGeometry.Position.X + sourceGeometry.Width / 2,
      sourceGeometry.Position.Y + sourceGeometry.Height / 2);
    return (
      targetCenter.X < sourceCenter.X ? -1 : 1,
      targetCenter.Y < sourceCenter.Y ? -1 : 1);
  }

  private static (int Direction, int DirectionY) ResolveUpgradedDirection(
    Vector2 sourceCenter,
    NpcTargetGeometrySnapshot targetGeometry)
  {
    Vector2 targetCenter = new(
      targetGeometry.Position.X + targetGeometry.Width / 2,
      targetGeometry.Position.Y + targetGeometry.Height / 2);
    return (
      targetCenter.X < sourceCenter.X ? -1 : 1,
      targetCenter.Y < sourceCenter.Y ? -1 : 1);
  }
}
