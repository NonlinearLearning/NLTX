namespace NLTX.ClientPresentation.AudioParticlesCinematics.Particles;

public sealed class ParticleRendererAdapterState
{
  private object? _rendererHandle;

  public bool IsAttached => _rendererHandle is not null;

  public ParticleRepelValue? RepelValue { get; private set; }

  public int ParticleCount { get; private set; }

  public void Attach(object rendererHandle)
  {
    ArgumentNullException.ThrowIfNull(rendererHandle);
    _rendererHandle = rendererHandle;
  }

  public void SetRepelValue(ParticleRepelValue repelValue)
  {
    RepelValue = repelValue;
  }

  public void SetParticleCount(int particleCount)
  {
    if (particleCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(particleCount));
    }

    ParticleCount = particleCount;
  }

  public void Detach()
  {
    _rendererHandle = null;
    RepelValue = null;
    ParticleCount = 0;
  }
}
