using Terraria.Relationships;

namespace Terraria.Player.Progression;

public sealed class PlayerMinionCapacityCommitSystem
{
  private readonly HashSet<Guid> _appliedTokens = [];
  private readonly PlayerMinionCapacityComponent _component;

  public PlayerMinionCapacityCommitSystem(PlayerMinionCapacityComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    _component = component;
  }

  public PlayerMinionCapacityCommitStatus Apply(
    in SubmitMinionCapacityDeltaCommand command)
  {
    if (!IsValidCommand(command))
    {
      return PlayerMinionCapacityCommitStatus.RejectedInvalidCommand;
    }

    if (_appliedTokens.Contains(command.IdempotencyToken))
    {
      return PlayerMinionCapacityCommitStatus.AlreadyApplied;
    }

    var delta = new PlayerMinionCapacityDelta(
      command.MinionCountDelta,
      command.SlotDelta);
    var status = ApplyDelta(delta);
    if (status == PlayerMinionCapacityCommitStatus.Committed)
    {
      _appliedTokens.Add(command.IdempotencyToken);
    }

    return status;
  }

  public void RebuildMaximum(int maxMinions)
  {
    if (maxMinions < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxMinions));
    }

    _component.MaxMinions = maxMinions;
  }

  public void Reset()
  {
    _component.MaxMinions = 1;
    _component.NumMinions = 0;
    _component.SlotsMinions = 0;
    _appliedTokens.Clear();
  }

  private PlayerMinionCapacityCommitStatus ApplyDelta(
    in PlayerMinionCapacityDelta delta)
  {
    if (!float.IsFinite(delta.SlotDelta))
    {
      return PlayerMinionCapacityCommitStatus.RejectedInvalidDelta;
    }

    var nextMinionCount = (long)_component.NumMinions + delta.MinionCountDelta;
    var nextSlots = _component.SlotsMinions + delta.SlotDelta;
    if (nextMinionCount < 0 || nextSlots < 0)
    {
      return PlayerMinionCapacityCommitStatus.RejectedInvalidDelta;
    }

    if (nextSlots > _component.MaxMinions)
    {
      return PlayerMinionCapacityCommitStatus.RejectedCapacityExceeded;
    }

    _component.NumMinions = checked((int)nextMinionCount);
    _component.SlotsMinions = nextSlots;
    return PlayerMinionCapacityCommitStatus.Committed;
  }

  private static bool IsValidCommand(
    in SubmitMinionCapacityDeltaCommand command)
  {
    return command.Owner is { IsEmpty: false, Scope: EntityReferenceScope.Player } &&
      command.Projectile.OwnerReference == command.Owner &&
      command.Projectile.OwnerReference.Scope == EntityReferenceScope.Player &&
      command.Projectile.Identity >= 0 &&
      command.SourceRevision >= 0 &&
      command.IdempotencyToken != Guid.Empty;
  }
}
