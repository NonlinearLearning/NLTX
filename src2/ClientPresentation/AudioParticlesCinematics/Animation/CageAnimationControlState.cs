namespace NLTX.ClientPresentation.AudioParticlesCinematics.Animation;

public sealed class CageAnimationControlState
{
  public int CageFrames { get; private set; }

  public bool CritterCage { get; private set; }

  public void Set(int cageFrames, bool critterCage)
  {
    if (cageFrames < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cageFrames));
    }

    CageFrames = cageFrames;
    CritterCage = critterCage;
  }
}
