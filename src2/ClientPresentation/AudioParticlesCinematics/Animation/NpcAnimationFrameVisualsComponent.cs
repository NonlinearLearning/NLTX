using System.Collections.ObjectModel;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Animation;

public sealed class NpcAnimationFrameVisualsComponent
{
  private readonly ReadOnlyCollection<int> _frameCounts;

  public NpcAnimationFrameVisualsComponent(IEnumerable<int> frameCounts)
  {
    ArgumentNullException.ThrowIfNull(frameCounts);

    int[] values = frameCounts.ToArray();
    if (values.Any(value => value <= 0))
    {
      throw new ArgumentOutOfRangeException(nameof(frameCounts), "Frame counts must be positive.");
    }

    _frameCounts = Array.AsReadOnly(values);
  }

  public IReadOnlyList<int> FrameCounts => _frameCounts;

  public bool TryGetFrameCount(int npcType, out int frameCount)
  {
    if ((uint)npcType >= (uint)_frameCounts.Count)
    {
      frameCount = 0;
      return false;
    }

    frameCount = _frameCounts[npcType];
    return true;
  }
}
