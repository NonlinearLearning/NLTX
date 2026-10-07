using System.Collections.ObjectModel;
using System.Numerics;

namespace Terraria.Player.Presentation;

/// <summary>
/// 保存玩家头顶消息的文本、片段、尺寸、颜色和剩余时间。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player.OverheadMessage。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：chatText（第 457 行）； snippets（第 459 行）； messageSize（第 461 行）； timeLeft（第 463 行）； color（第
/// 465 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p11-player-presentation-derived-component-design.md。
/// </para>
/// <para>依据位置：第 102 行。</para>
/// </remarks>
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
