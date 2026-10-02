namespace Terraria.Player;

public sealed class PlayerBarrierSystem
{
  private const byte BarrierFrameCount = 12;
  private readonly PlayerBarrierAndRegenComponent _component;

  public PlayerBarrierSystem(PlayerBarrierAndRegenComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    _component = component;
  }

  public void CommitCapabilities(in PlayerBarrierCapabilityInput input)
  {
    _component.PalladiumRegen = input.PalladiumRegen;
    _component.IceBarrier = input.IceBarrierBuffActive &&
      IsAtOrBelowHalfLife(input.CurrentLife, input.EffectiveLifeMaximum);
  }

  public void AdvanceTick()
  {
    if (!_component.IceBarrier)
    {
      return;
    }

    _component.IceBarrierFrameCounter++;
    if (_component.IceBarrierFrameCounter <= 2)
    {
      return;
    }

    _component.IceBarrierFrameCounter = 0;
    _component.IceBarrierFrame++;
    if (_component.IceBarrierFrame >= BarrierFrameCount)
    {
      _component.IceBarrierFrame = 0;
    }
  }

  public void ResetEffects()
  {
    _component.IceBarrier = false;
    _component.PalladiumRegen = false;
  }

  public void ResetForLifecycle()
  {
    _component.IceBarrier = false;
    _component.IceBarrierFrame = 0;
    _component.IceBarrierFrameCounter = 0;
    _component.PalladiumRegen = false;
  }

  private static bool IsAtOrBelowHalfLife(
    int currentLife,
    int effectiveLifeMaximum)
  {
    return effectiveLifeMaximum > 0 &&
      currentLife >= 0 &&
      (long)currentLife * 2L <= effectiveLifeMaximum;
  }
}
