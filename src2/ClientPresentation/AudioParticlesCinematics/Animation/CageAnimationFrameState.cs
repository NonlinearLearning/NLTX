namespace NLTX.ClientPresentation.AudioParticlesCinematics.Animation;

public readonly record struct CageAnimationFrameState
{
  public CageAnimationFrameState(int Frame, int TicksSinceAdvance, byte Mode)
  {
    if (Frame < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Frame));
    }

    if (TicksSinceAdvance < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(TicksSinceAdvance));
    }

    this.Frame = Frame;
    this.TicksSinceAdvance = TicksSinceAdvance;
    this.Mode = Mode;
  }

  public int Frame { get; }

  public int TicksSinceAdvance { get; }

  public byte Mode { get; }
}
