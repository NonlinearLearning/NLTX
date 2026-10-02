namespace Terraria.Items.InventoryContainers;

public sealed class ChestPresentationStateComponent
{
  public int FrameCounter { get; private set; }

  public int Frame { get; private set; }

  public int EatingAnimationTime { get; private set; }

  public void AdvanceFrame(int frameCount)
  {
    if (frameCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(frameCount));
    }

    Frame = (Frame + 1) % frameCount;
    FrameCounter++;
  }

  public void SetEatingAnimationTime(int ticks)
  {
    EatingAnimationTime = Math.Max(0, ticks);
  }
}
