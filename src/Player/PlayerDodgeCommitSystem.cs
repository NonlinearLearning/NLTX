namespace Terraria.Player;

public sealed class PlayerDodgeCommitSystem
{
  private const int DefaultAnimationTicks = 300;

  private readonly HashSet<Guid> _acceptedActivationIds = [];
  private readonly HashSet<Guid> _consumedDamageIds = [];
  private readonly PlayerDodgeAndImmunityStateComponent _component;

  public PlayerDodgeCommitSystem(
    PlayerDodgeAndImmunityStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    _component = component;
  }

  public void RebuildCapabilities(in PlayerDodgeCapabilityInput input)
  {
    _component.BlackBelt = input.BlackBelt;
    _component.BrainOfConfusionItem = input.BrainOfConfusionItem;
  }

  public bool ActivateShadowDodge(in PlayerDodgeActivationCommand command)
  {
    if (command.CommandId == Guid.Empty ||
      command.SourceRevision < 0 ||
      command.ShadowDodgeTimer <= 0 ||
      !_acceptedActivationIds.Add(command.CommandId))
    {
      return false;
    }

    _component.ShadowDodge = true;
    _component.ShadowDodgeTimer = command.ShadowDodgeTimer;
    _component.BrainOfConfusionDodgeAnimationCounter = Math.Max(
      0,
      command.AnimationTicks == 0
        ? DefaultAnimationTicks
        : command.AnimationTicks);
    return true;
  }

  public bool ConsumeCommittedDamage(in PlayerDodgeCommitCommand command)
  {
    if (command.CommandId == Guid.Empty ||
      command.SourceId == Guid.Empty ||
      command.SourceRevision < 0 ||
      command.DamageAmount <= 0 ||
      !command.IsCommitted ||
      !_consumedDamageIds.Add(command.CommandId))
    {
      return false;
    }

    if (!_component.ShadowDodge)
    {
      return false;
    }

    _component.ShadowDodge = false;
    return true;
  }

  public void AdvanceTick()
  {
    _component.BrainOfConfusionDodgeAnimationCounter = Decrement(
      _component.BrainOfConfusionDodgeAnimationCounter);
    _component.ShadowDodgeTimer = Decrement(_component.ShadowDodgeTimer);
    if (_component.ShadowDodgeTimer == 0)
    {
      _component.ShadowDodge = false;
    }
  }

  public void ResetEffects()
  {
    _component.BlackBelt = false;
    _component.BrainOfConfusionItem = ItemEntityRef.None;
    _component.ShadowDodge = false;
    _component.BrainOfConfusionDodgeAnimationCounter = 0;
  }

  public void ResetForLifecycle()
  {
    ResetEffects();
    _component.ShadowDodgeTimer = 0;
    _acceptedActivationIds.Clear();
    _consumedDamageIds.Clear();
  }

  private static int Decrement(int value)
  {
    return Math.Max(0, value - 1);
  }
}
