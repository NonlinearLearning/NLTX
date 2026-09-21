using Terraria.ClientPresentation.Ui.Layout;
using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.World;

public sealed class WorldInteractionVisualsComponent
{
  public WorldInteractionVisualsComponent(
    UiElementId hostId,
    int rarity = 0)
  {
    if (!hostId.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(hostId));
    }

    HostId = hostId;
    Rarity = rarity;
    RarityColor = UiRarityQuery.GetColor(rarity);
  }

  public UiElementId HostId { get; }

  public bool HasBubble { get; private set; }

  public int BubbleId { get; private set; }

  public int BubbleFrame { get; private set; }

  public int BubbleFrameCount { get; private set; }

  public long BubbleRemainingMilliseconds { get; private set; }

  public long BubbleFrameDurationMilliseconds { get; private set; }

  public UiWorldAnchor BubbleAnchor { get; private set; } = UiWorldAnchor.None;

  public int Rarity { get; private set; }

  public uint RarityColor { get; private set; }

  public bool RadialOpen { get; private set; }

  public ToolMode SelectedTool { get; private set; } = ToolMode.None;

  internal void SetBubble(
    int bubbleId,
    int frameCount,
    long lifetimeMilliseconds,
    long frameDurationMilliseconds,
    UiWorldAnchor anchor)
  {
    HasBubble = true;
    BubbleId = bubbleId;
    BubbleFrame = 0;
    BubbleFrameCount = frameCount;
    BubbleRemainingMilliseconds = lifetimeMilliseconds;
    BubbleFrameDurationMilliseconds = frameDurationMilliseconds;
    BubbleAnchor = anchor;
  }

  internal void AdvanceBubble(long elapsedMilliseconds)
  {
    if (!HasBubble || elapsedMilliseconds <= 0)
    {
      return;
    }

    BubbleRemainingMilliseconds -= elapsedMilliseconds;
    if (BubbleRemainingMilliseconds <= 0)
    {
      ClearBubble();
      return;
    }

    if (BubbleFrameDurationMilliseconds > 0)
    {
      int frames = (int)(elapsedMilliseconds / BubbleFrameDurationMilliseconds);
      BubbleFrame = (BubbleFrame + frames) % BubbleFrameCount;
    }
  }

  internal void ClearBubble()
  {
    HasBubble = false;
    BubbleId = 0;
    BubbleFrame = 0;
    BubbleFrameCount = 0;
    BubbleRemainingMilliseconds = 0;
    BubbleFrameDurationMilliseconds = 0;
    BubbleAnchor = UiWorldAnchor.None;
  }

  internal void SetRadial(bool isOpen)
  {
    RadialOpen = isOpen;
    if (!isOpen)
    {
      SelectedTool = ToolMode.None;
    }
  }

  internal bool SetTool(ToolMode tool)
  {
    if (!RadialOpen || tool == ToolMode.None)
    {
      return false;
    }

    SelectedTool = tool;
    return true;
  }

  public enum ToolMode
  {
    None,
    Wire,
    Actuator,
    Cutter
  }
}
