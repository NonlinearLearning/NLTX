namespace NLTX.ClientPresentation.AudioParticlesCinematics.Animation;

public sealed class CageAquaticAnimationVisualsComponent
{
  private readonly Dictionary<CageAnimationKey, CageAnimationFrameState> _frames = new();

  public IReadOnlyDictionary<CageAnimationKey, CageAnimationFrameState> Frames => _frames;

  public void Set(CageAnimationKey key, CageAnimationFrameState state)
  {
    _frames[key] = state;
  }

  public bool TryGet(CageAnimationKey key, out CageAnimationFrameState state)
  {
    return _frames.TryGetValue(key, out state);
  }

  public bool Remove(CageAnimationKey key)
  {
    return _frames.Remove(key);
  }
}
