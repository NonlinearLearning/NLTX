namespace Terraria.Unshared.Chat;

/// <summary>One accepted chat line kept in the server's short history.</summary>
public readonly record struct ChatMessage(
    ChatChannel Channel,
    string AuthorName,
    string Text,
    byte AuthorId,
    long Tick)
{
  public bool IsValid => Text.Length > 0 && Tick >= 0;

  /// <summary>The text as the client will render it.</summary>
  public string DisplayText
    => Channel is ChatChannel.Say or ChatChannel.Party or ChatChannel.Announce && AuthorName.Length > 0
        ? $"{AuthorName}: {Text}"
        : Text;
}
