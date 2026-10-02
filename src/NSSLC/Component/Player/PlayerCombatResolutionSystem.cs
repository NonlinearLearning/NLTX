namespace Terraria.Player;

public sealed class PlayerCombatResolutionSystem
{
  private readonly HashSet<Guid> _committedEventIds = [];
  private readonly PlayerVitalStateComponent _vital;
  private readonly PlayerCombatDefenseModifierComponent _defense;
  private readonly PlayerDamageMitigationInputComponent _mitigation;
  private readonly PlayerDodgeAndImmunityStateComponent _dodge;
  private readonly PlayerDodgeCommitSystem _dodgeCommit;
  private readonly PlayerCombatProcSystem _proc;
  private readonly IPlayerDpsTelemetryCommitPort _telemetry;

  public PlayerCombatResolutionSystem(
    PlayerVitalStateComponent vital,
    PlayerCombatDefenseModifierComponent defense,
    PlayerDamageMitigationInputComponent mitigation,
    PlayerDodgeAndImmunityStateComponent dodge,
    PlayerDodgeCommitSystem dodgeCommit,
    PlayerCombatProcSystem proc,
    IPlayerDpsTelemetryCommitPort telemetry)
  {
    ArgumentNullException.ThrowIfNull(vital);
    ArgumentNullException.ThrowIfNull(defense);
    ArgumentNullException.ThrowIfNull(mitigation);
    ArgumentNullException.ThrowIfNull(dodge);
    ArgumentNullException.ThrowIfNull(dodgeCommit);
    ArgumentNullException.ThrowIfNull(proc);
    ArgumentNullException.ThrowIfNull(telemetry);

    _vital = vital;
    _defense = defense;
    _mitigation = mitigation;
    _dodge = dodge;
    _dodgeCommit = dodgeCommit;
    _proc = proc;
    _telemetry = telemetry;
  }

  public PlayerCombatResolutionResult Resolve(
    in PlayerCombatResolutionCommand command)
  {
    if (command.EventId == Guid.Empty)
    {
      return PlayerCombatResolutionResult.Rejected(
        PlayerDamageEligibilityRejectionReason.EmptyEvent,
        _vital.StatLife);
    }

    if (command.SourceId == Guid.Empty)
    {
      return PlayerCombatResolutionResult.Rejected(
        PlayerDamageEligibilityRejectionReason.EmptySource,
        _vital.StatLife);
    }

    if (command.SourceRevision < 0)
    {
      return PlayerCombatResolutionResult.Rejected(
        PlayerDamageEligibilityRejectionReason.InvalidSourceRevision,
        _vital.StatLife);
    }

    if (_committedEventIds.Contains(command.EventId))
    {
      return PlayerCombatResolutionResult.Rejected(
        PlayerDamageEligibilityRejectionReason.DuplicateEvent,
        _vital.StatLife);
    }

    PlayerDamageEligibilityResult eligibility =
      PlayerDamageEligibilityQuery.Evaluate(
        new PlayerDamageEligibilityInput(
          command.SourceId,
          command.DamageAmount,
          command.HasGeneralImmunity,
          command.SourceCooldownActive,
          command.Dodgeable),
        _dodge);
    if (!eligibility.IsEligible)
    {
      return PlayerCombatResolutionResult.Rejected(
        eligibility.RejectionReason,
        _vital.StatLife);
    }

    if (eligibility.UsesShadowDodge)
    {
      bool consumed = _dodgeCommit.ConsumeCommittedDamage(
        new PlayerDodgeCommitCommand(
          command.EventId,
          command.SourceId,
          command.SourceRevision,
          command.DamageAmount,
          IsCommitted: true));
      if (!consumed)
      {
        return PlayerCombatResolutionResult.Rejected(
          PlayerDamageEligibilityRejectionReason.ShadowDodge,
          _vital.StatLife);
      }

      _committedEventIds.Add(command.EventId);
      return PlayerCombatResolutionResult.Rejected(
        PlayerDamageEligibilityRejectionReason.ShadowDodge,
        _vital.StatLife);
    }

    int finalDamage = PlayerDamageMitigationQuery.Evaluate(
      new PlayerDamageMitigationInput(
        command.DamageAmount,
        _defense.StatDefense,
        _mitigation.Endurance,
        command.Critical));
    _vital.StatLife = Math.Max(0, _vital.StatLife - finalDamage);
    _committedEventIds.Add(command.EventId);

    bool procPublished = _proc.AcceptCommittedHit(
      new PlayerCommittedCombatHitEvent(
        command.EventId,
        command.SourceId,
        finalDamage,
        command.SourceRevision,
        IsCommitted: true,
        Effects: new PlayerCombatProcHitEffects(
          GhostDamage: 0f,
          LifeStealCost: 0f,
          OnHitDodge: false,
          OnHitRegen: false,
          OnHitPetal: false,
          OnHitTitaniumStorm: false)));
    bool telemetryPublished = _telemetry.AcceptCommittedDamage(
      new PlayerCommittedDamageEvent(
        command.EventId,
        finalDamage,
        command.CommittedAt,
        command.SourceRevision));

    return new PlayerCombatResolutionResult(
      Applied: true,
      Dodged: false,
      FinalDamage: finalDamage,
      LifeAfter: _vital.StatLife,
      Killed: _vital.StatLife <= 0,
      ProcPublished: procPublished,
      TelemetryPublished: telemetryPublished,
      RejectionReason: PlayerDamageEligibilityRejectionReason.None);
  }

  public void ResetForLifecycle()
  {
    _committedEventIds.Clear();
  }
}
