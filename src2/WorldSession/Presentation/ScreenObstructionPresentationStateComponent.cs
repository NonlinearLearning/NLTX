using System;

namespace Terraria.WorldSession.Presentation;

public sealed class ScreenObstructionPresentationStateComponent
{
  public ScreenObstructionPresentationStateComponent(
    float lastTransitionSpeed = 0.1f,
    float currentAmount = 0.0f)
  {
    LastTransitionSpeed = lastTransitionSpeed;
    CurrentAmount = currentAmount;
    Validate();
  }

  public float LastTransitionSpeed { get; internal set; }

  public float CurrentAmount { get; internal set; }

  public void Validate()
  {
    if (!float.IsFinite(LastTransitionSpeed) || LastTransitionSpeed <= 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(LastTransitionSpeed));
    }

    if (!float.IsFinite(CurrentAmount) || CurrentAmount < 0.0f || CurrentAmount > 0.95f)
    {
      throw new ArgumentOutOfRangeException(nameof(CurrentAmount));
    }
  }
}
