namespace Terraria.Network.Section;

public sealed class ActiveSectionObservationStateComponent
{
  private readonly uint[,] _lastActiveTime;

  public ActiveSectionObservationStateComponent(
    int sectionWidth,
    int sectionHeight,
    uint sectionInactiveTime = 60,
    uint[,]? lastActiveTime = null)
  {
    if (sectionWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sectionWidth));
    }

    if (sectionHeight <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sectionHeight));
    }

    if (lastActiveTime is not null &&
        (lastActiveTime.GetLength(0) != sectionWidth ||
         lastActiveTime.GetLength(1) != sectionHeight))
    {
      throw new ArgumentException(
        "Last-active timestamps must match the section dimensions.",
        nameof(lastActiveTime));
    }

    SectionWidth = sectionWidth;
    SectionHeight = sectionHeight;
    SectionInactiveTime = sectionInactiveTime;
    _lastActiveTime = lastActiveTime is null
      ? new uint[sectionWidth, sectionHeight]
      : (uint[,])lastActiveTime.Clone();
  }

  public int SectionWidth { get; }

  public int SectionHeight { get; }

  public uint SectionInactiveTime { get; }

  public uint[,] LastActiveTime => (uint[,])_lastActiveTime.Clone();
}
