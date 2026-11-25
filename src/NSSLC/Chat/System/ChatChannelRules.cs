namespace Terraria.Unshared.Chat;

/// <summary>Maps the protocol's text-command name onto a chat channel.</summary>
/// <remarks>
/// The client's text module carries an explicit command name next to the typed text (the server-side
/// codec in this repository's earlier protocol layer read the pair as <c>CommandName</c> then
/// <c>Text</c>). <c>SayChatCommand</c> and <c>PartyChatCommand</c> are the two chat commands that
/// carry free text; anything else is treated as an unknown text command and refused.
/// </remarks>
public static class ChatChannelRules
{
  /// <summary>Command name of <c>SayChatCommand</c>.</summary>
  public const string SayCommandName = "Say";

  /// <summary>Command name of <c>PartyChatCommand</c>.</summary>
  public const string PartyCommandName = "Party";

  public static bool TryResolve(string? commandName, out ChatChannel channel)
  {
    channel = ChatChannel.Say;
    if (string.IsNullOrEmpty(commandName))
    {
      return false;
    }

    if (string.Equals(commandName, SayCommandName, System.StringComparison.OrdinalIgnoreCase))
    {
      channel = ChatChannel.Say;
      return true;
    }

    if (string.Equals(commandName, PartyCommandName, System.StringComparison.OrdinalIgnoreCase))
    {
      channel = ChatChannel.Party;
      return true;
    }

    return false;
  }
}
