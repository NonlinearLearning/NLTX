namespace Terraria.EntityLifecycleAttribution;

public readonly record struct ItemAnimationDefinition
{
  public ItemAnimationDefinition(int contentType, int frameCount, int frameDurationTicks)
  {
    if (contentType < 0 || frameCount <= 0 || frameDurationTicks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ContentType));
    }

    ContentType = contentType;
    FrameCount = frameCount;
    FrameDurationTicks = frameDurationTicks;
  }

  public int ContentType { get; }

  public int FrameCount { get; }

  public int FrameDurationTicks { get; }
}
