namespace Terraria.ClientPresentation.Ui.World;

public sealed class UiEmoteTransportAdapter
{
  public bool TryValidate(Payload payload, out ValidatedPayload validated)
  {
    if (payload.BubbleId <= 0
      || payload.FrameCount <= 0
      || payload.LifetimeMilliseconds <= 0
      || payload.LifetimeMilliseconds > 120000
      || payload.FrameDurationMilliseconds < 0
      || !payload.Anchor.IsValid)
    {
      validated = default;
      return false;
    }

    validated = new ValidatedPayload(
      payload.BubbleId,
      payload.FrameCount,
      payload.LifetimeMilliseconds,
      payload.FrameDurationMilliseconds,
      payload.Anchor);
    return true;
  }

  public readonly record struct Payload(
    int BubbleId,
    int FrameCount,
    long LifetimeMilliseconds,
    long FrameDurationMilliseconds,
    UiWorldAnchor Anchor);

  public readonly record struct ValidatedPayload(
    int BubbleId,
    int FrameCount,
    long LifetimeMilliseconds,
    long FrameDurationMilliseconds,
    UiWorldAnchor Anchor);
}
