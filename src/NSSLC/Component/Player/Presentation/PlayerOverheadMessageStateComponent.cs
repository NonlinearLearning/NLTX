using System.Collections.ObjectModel;
using System.Numerics;

namespace Terraria.Player.Presentation;

public sealed class PlayerOverheadMessageStateComponent
{
  private string _chatText = string.Empty;

  private ReadOnlyCollection<string> _snippets =
    new ReadOnlyCollection<string>(Array.Empty<string>());

  private Vector2 _messageSize;

  private int _timeLeft;

  private PlayerAppearanceColor _color;

  public string ChatText => _chatText;

  public IReadOnlyList<string> Snippets => _snippets;

  public Vector2 MessageSize => _messageSize;

  public int TimeLeft => _timeLeft;

  public PlayerAppearanceColor Color => _color;

  internal void Replace(in PlayerOverheadMessageInput input)
  {
    ArgumentNullException.ThrowIfNull(input.ChatText);
    ArgumentNullException.ThrowIfNull(input.Snippets);

    _chatText = input.ChatText;
    _snippets = new ReadOnlyCollection<string>(input.Snippets.ToArray());
    _messageSize = input.MessageSize;
    _timeLeft = input.TimeLeft;
    _color = input.Color;
  }

  internal bool AdvancePresentationTick()
  {
    if (_timeLeft <= 0)
    {
      return false;
    }

    _timeLeft--;
    return true;
  }

  internal void Reset()
  {
    _chatText = string.Empty;
    _snippets = new ReadOnlyCollection<string>(Array.Empty<string>());
    _messageSize = Vector2.Zero;
    _timeLeft = 0;
    _color = default;
  }

  public PlayerOverheadMessageInput ToInput()
  {
    return new PlayerOverheadMessageInput(
      _chatText,
      _snippets,
      _messageSize,
      _timeLeft,
      _color);
  }
}
