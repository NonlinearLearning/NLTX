using Terraria.ClientPresentation.Ui.World;

namespace Terraria.ClientPresentation.Ui.Systems;

public sealed class UiEmoteProjectionSystem
{
  private readonly WorldInteractionVisualsComponent _visuals;
  private readonly UiEmoteTransportAdapter _transport;

  public UiEmoteProjectionSystem(
    WorldInteractionVisualsComponent visuals,
    UiEmoteTransportAdapter transport)
  {
    _visuals = visuals ?? throw new ArgumentNullException(nameof(visuals));
    _transport = transport ?? throw new ArgumentNullException(nameof(transport));
  }

  public bool ApplyInbound(UiEmoteTransportAdapter.Payload payload)
  {
    if (!_transport.TryValidate(
      payload,
      out UiEmoteTransportAdapter.ValidatedPayload validated))
    {
      return false;
    }

    _visuals.SetBubble(
      validated.BubbleId,
      validated.FrameCount,
      validated.LifetimeMilliseconds,
      validated.FrameDurationMilliseconds,
      validated.Anchor);
    return true;
  }

  public void Advance(long elapsedMilliseconds)
  {
    if (elapsedMilliseconds < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(elapsedMilliseconds));
    }

    _visuals.AdvanceBubble(elapsedMilliseconds);
  }

  public void Clear()
  {
    _visuals.ClearBubble();
  }
}
