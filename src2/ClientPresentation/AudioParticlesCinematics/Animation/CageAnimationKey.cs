namespace NLTX.ClientPresentation.AudioParticlesCinematics.Animation;

public readonly record struct CageAnimationKey
{
  public CageAnimationKey(int ContainerId, int SlotIndex)
  {
    if (ContainerId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ContainerId));
    }

    if (SlotIndex < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(SlotIndex));
    }

    this.ContainerId = ContainerId;
    this.SlotIndex = SlotIndex;
  }

  public int ContainerId { get; }

  public int SlotIndex { get; }
}
