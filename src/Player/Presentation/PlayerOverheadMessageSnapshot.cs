using System.Collections.ObjectModel;
using System.Numerics;

namespace Terraria.Player.Presentation;

public sealed class PlayerOverheadMessageSnapshot
{
  public PlayerOverheadMessageSnapshot(
    PlayerCompositeArmSnapshot frontArm,
    PlayerCompositeArmSnapshot backArm,
    string chatText,
    IReadOnlyList<string> snippets,
    Vector2 messageSize,
    int timeLeft,
    PlayerAppearanceColor color)
  {
    ArgumentNullException.ThrowIfNull(chatText);
    ArgumentNullException.ThrowIfNull(snippets);

    FrontArm = frontArm;
    BackArm = backArm;
    ChatText = chatText;
    Snippets = new ReadOnlyCollection<string>(snippets.ToArray());
    MessageSize = messageSize;
    TimeLeft = timeLeft;
    Color = color;
  }

  public PlayerCompositeArmSnapshot FrontArm { get; }

  public PlayerCompositeArmSnapshot BackArm { get; }

  public string ChatText { get; }

  public IReadOnlyList<string> Snippets { get; }

  public Vector2 MessageSize { get; }

  public int TimeLeft { get; }

  public PlayerAppearanceColor Color { get; }

  public bool IsVisible => TimeLeft > 0;
}
