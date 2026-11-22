using Terraria.Relationships;

namespace Terraria.Chat;

/// <summary>One chat line the network layer must deliver.</summary>
/// <remarks>
/// <see cref="OnlyFor"/> is <c>null</c> for ordinary broadcasts (every logged-in session) and set
/// for private replies such as a command's usage text. <see cref="AuthorId"/> is the reference's
/// author byte: a player index, or <see cref="ChatNetTextFrame.ServerAuthorId"/> when the author
/// is not one player in particular.
/// </remarks>
public readonly record struct ChatBroadcast(
    ChatChannel Channel,
    string AuthorName,
    string Text,
    byte AuthorId,
    ChatColor Color,
    EntityRuntimeHandle? OnlyFor,
    long Tick)
{
  public bool IsPrivate => OnlyFor is not null;

  /// <summary>Text as the client renders it: spoken channels carry the speaker's name.</summary>
  /// <remarks>
  /// The author byte is the reference's display switch: a real player index makes the client prefix
  /// the player's own name, while <see cref="ChatNetTextFrame.ServerAuthorId"/> means "server text,
  /// show as-is". Because the server has no player index map yet, it composes the name itself for
  /// spoken lines and leaves server replies unprefixed.
  /// </remarks>
  public string DisplayText
    => MentionsSpeaker && AuthorName.Length > 0 ? $"{AuthorName}: {Text}" : Text;

  private bool MentionsSpeaker
    => Channel is ChatChannel.Say or ChatChannel.Party or ChatChannel.Announce;
}
