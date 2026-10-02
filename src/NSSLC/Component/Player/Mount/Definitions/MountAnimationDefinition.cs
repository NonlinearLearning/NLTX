namespace Terraria.Player.Mount;

public readonly record struct MountAnimationDefinition(
  int TotalFrames,
  MountFrameRange Standing,
  MountFrameRange Running,
  MountFrameRange InAir,
  MountFrameRange Flying,
  MountFrameRange Swimming,
  MountFrameRange Dashing,
  MountFrameRange Idle,
  bool IdleFrameLoop)
{
  public MountFrameRange GetRange(MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind state)
  {
    return state switch
    {
      MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Standing => Standing,
      MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Running => Running,
      MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.InAir => InAir,
      MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Flying => Flying,
      MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Swimming => Swimming,
      MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Dashing => Dashing,
      _ => Standing,
    };
  }

  public void Validate()
  {
    if (TotalFrames < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(TotalFrames));
    }

    ValidateRange(Standing, nameof(Standing));
    ValidateRange(Running, nameof(Running));
    ValidateRange(InAir, nameof(InAir));
    ValidateRange(Flying, nameof(Flying));
    ValidateRange(Swimming, nameof(Swimming));
    ValidateRange(Dashing, nameof(Dashing));
    ValidateRange(Idle, nameof(Idle));
  }

  private static void ValidateRange(MountFrameRange range, string parameterName)
  {
    if (range.Start < 0 || range.Count < 0 || range.Delay < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
