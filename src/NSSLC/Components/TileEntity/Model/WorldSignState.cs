namespace Terraria.World;

/// <summary>One world sign: the tile it hangs on and the text it stores.</summary>
/// <remarks>
/// Version4 keeps the text on a <c>Sign</c> instance found by position and lets the client bound
/// the editor. The server has no editor, so it applies a packet-safe upper bound of its own and
/// normalizes line endings to <c>\n</c>, which is what the client's read message expects.
/// </remarks>
public sealed class WorldSignState
{
  /// <summary>
  /// Server-side bound on one sign's text.
  /// </summary>
  /// <remarks>
  /// This is not a reference constant: Version4 bounds the editor per line instead. The bound here
  /// is chosen so a sign fits one packet-47 write and is documented as a server policy.
  /// </remarks>
  public const int MaximumTextLength = 512;

  public WorldSignState(short index, WorldTileCoordinate position, string text)
  {
    Index = index;
    Position = position;
    // 载入路径放宽：存档文本超长时截断而不是拒绝整个世界。
    Text = Bound(NormalizeLineEndings(text));
  }

  public short Index { get; }

  public WorldTileCoordinate Position { get; }

  public string Text { get; private set; }

  public bool IsEmpty => Text.Length == 0;

  /// <summary>Replaces the text after normalizing; overlong edits are rejected, not truncated.</summary>
  public bool TryEdit(string text)
  {
    string normalized = NormalizeLineEndings(text);
    if (normalized.Length > MaximumTextLength)
    {
      return false;
    }

    Text = normalized;
    return true;
  }

  private static string NormalizeLineEndings(string? text)
  {
    if (string.IsNullOrEmpty(text))
    {
      return string.Empty;
    }

    return text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
  }

  private static string Bound(string text)
    => text.Length > MaximumTextLength ? text[..MaximumTextLength] : text;
}
