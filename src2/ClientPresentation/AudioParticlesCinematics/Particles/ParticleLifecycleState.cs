namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public sealed class ParticleLifecycleState
{
  public bool ShouldBeRemovedFromRenderer { get; private set; }

  public bool IsRestingInPool { get; private set; }

  public bool IsActive { get; private set; }

  public void Activate()
  {
    IsActive = true;
    IsRestingInPool = false;
    ShouldBeRemovedFromRenderer = false;
  }

  public void MarkRemoved()
  {
    ShouldBeRemovedFromRenderer = true;
    IsActive = false;
  }

  public void ResetToPool()
  {
    ShouldBeRemovedFromRenderer = false;
    IsRestingInPool = true;
    IsActive = false;
  }
}
