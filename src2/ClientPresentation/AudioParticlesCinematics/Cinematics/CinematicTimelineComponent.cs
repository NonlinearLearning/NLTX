using System.Collections.ObjectModel;

namespace NLTX.ClientPresentation.AudioParticlesCinematics.Cinematics;

public sealed class CinematicTimelineComponent
{
  private readonly ReadOnlyCollection<(int Start, int Duration, int EventCode)> _sequences;

  public CinematicTimelineComponent(
    IEnumerable<(int Start, int Duration, int EventCode)>? sequences = null,
    int frame = 0,
    int frameCount = 0,
    int nextSequenceAppendTime = 0,
    bool isActive = false)
  {
    if (frame < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(frame));
    }

    if (frameCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(frameCount));
    }

    if (nextSequenceAppendTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(nextSequenceAppendTime));
    }

    (int Start, int Duration, int EventCode)[] values =
      sequences?.ToArray() ?? [];
    int maximumSequenceEnd = 0;
    foreach ((int start, int duration, int eventCode) in values)
    {
      _ = eventCode;
      if (start < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(sequences));
      }

      if (duration <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(sequences));
      }

      long sequenceEnd = (long)start + duration;
      if (sequenceEnd > int.MaxValue)
      {
        throw new ArgumentOutOfRangeException(nameof(sequences));
      }

      maximumSequenceEnd = Math.Max(maximumSequenceEnd, (int)sequenceEnd);
    }

    FrameCount = Math.Max(frameCount, maximumSequenceEnd);
    if (frame > FrameCount)
    {
      throw new ArgumentOutOfRangeException(nameof(frame));
    }

    Frame = frame;
    NextSequenceAppendTime = Math.Max(nextSequenceAppendTime, maximumSequenceEnd);
    IsActive = isActive;
    _sequences = Array.AsReadOnly(values);
  }

  public int Frame { get; internal set; }

  public int FrameCount { get; internal set; }

  public int NextSequenceAppendTime { get; internal set; }

  public bool IsActive { get; internal set; }

  public IReadOnlyList<(int Start, int Duration, int EventCode)> Sequences => _sequences;
}
