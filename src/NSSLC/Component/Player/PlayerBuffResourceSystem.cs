namespace Terraria.Player;

// status: partial-isolated-core
// source-members: P09-720..P09-723, P09-10272, P09-6209..P09-6224
// crossSubsystemOwner: environment collision, damage, and equipment definitions remain integration-review
public sealed class PlayerBuffResourceSystem
{
  private const float LavaOpacityStep = 0.04f;

  private readonly PlayerResourceStateComponent _resources;

  public PlayerBuffResourceSystem(
    PlayerResourceStateComponent resources)
  {
    ArgumentNullException.ThrowIfNull(resources);
    _resources = resources;
  }

  public PlayerBuffResourceRebuildResult Rebuild(
    in PlayerBuffResourceRebuildInput input)
  {
    if (input.BreathMax <= 0)
    {
      return Rejected(PlayerBuffResourceRebuildRejectionReason.InvalidBreathMax);
    }

    if ((uint)input.Breath > (uint)input.BreathMax)
    {
      return Rejected(PlayerBuffResourceRebuildRejectionReason.InvalidBreath);
    }

    if (input.LavaMax < 0)
    {
      return Rejected(PlayerBuffResourceRebuildRejectionReason.InvalidLavaMax);
    }

    if ((uint)input.LavaTime > (uint)input.LavaMax)
    {
      return Rejected(PlayerBuffResourceRebuildRejectionReason.InvalidLavaTime);
    }

    if (float.IsNaN(input.LavaOpacity) ||
      input.LavaOpacity < PlayerResourceStateComponent.MinimumLavaOpacity ||
      input.LavaOpacity > PlayerResourceStateComponent.MaximumLavaOpacity)
    {
      return Rejected(PlayerBuffResourceRebuildRejectionReason.InvalidLavaOpacity);
    }

    _resources.BreathMax = input.BreathMax;
    _resources.Breath = input.Breath;
    _resources.LavaMax = input.LavaMax;
    _resources.LavaTime = input.LavaTime;
    _resources.IgnoreWater = input.IgnoreWater;
    _resources.LavaVision = input.LavaVision;
    _resources.LavaOpacity = input.LavaOpacity;
    return new PlayerBuffResourceRebuildResult(
      Applied: true,
      Snapshot: Snapshot(),
      RejectionReason: PlayerBuffResourceRebuildRejectionReason.None);
  }

  public PlayerBuffResourceSnapshot ResetForTick()
  {
    _resources.LavaMax = 0;
    _resources.IgnoreWater = false;
    _resources.LavaVision = false;
    if (_resources.LavaTime > _resources.LavaMax)
    {
      _resources.LavaTime = _resources.LavaMax;
    }

    return Snapshot();
  }

  public PlayerBuffResourceSnapshot AdvanceTick(
    in PlayerBuffResourceTickInput input)
  {
    if (!input.LavaWet && _resources.LavaTime < _resources.LavaMax)
    {
      _resources.LavaTime++;
    }

    if (_resources.LavaTime > _resources.LavaMax)
    {
      _resources.LavaTime = _resources.LavaMax;
    }

    if (_resources.LavaVision && input.LavaWet &&
      _resources.LavaOpacity > PlayerResourceStateComponent.MinimumLavaOpacity)
    {
      _resources.LavaOpacity = Math.Max(
        PlayerResourceStateComponent.MinimumLavaOpacity,
        _resources.LavaOpacity - LavaOpacityStep);
    }
    else if (_resources.LavaOpacity < PlayerResourceStateComponent.MaximumLavaOpacity)
    {
      _resources.LavaOpacity = Math.Min(
        PlayerResourceStateComponent.MaximumLavaOpacity,
        _resources.LavaOpacity + LavaOpacityStep);
    }

    return Snapshot();
  }

  public PlayerBuffResourceSnapshot ResetForLifecycle()
  {
    _resources.Breath = _resources.BreathMax;
    _resources.LavaTime = _resources.LavaMax;
    _resources.IgnoreWater = false;
    _resources.LavaVision = false;
    _resources.LavaOpacity = PlayerResourceStateComponent.MaximumLavaOpacity;
    return Snapshot();
  }

  public PlayerBuffResourceSnapshot Snapshot()
  {
    return new PlayerBuffResourceSnapshot(
      _resources.BreathMax,
      _resources.Breath,
      _resources.LavaMax,
      _resources.LavaTime,
      _resources.IgnoreWater,
      _resources.LavaVision,
      _resources.LavaOpacity);
  }

  private PlayerBuffResourceRebuildResult Rejected(
    PlayerBuffResourceRebuildRejectionReason reason)
  {
    return new PlayerBuffResourceRebuildResult(
      Applied: false,
      Snapshot: Snapshot(),
      RejectionReason: reason);
  }
}
