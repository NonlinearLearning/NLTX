namespace Terraria.Unshared.Chat;

/// <summary>Delivery scope of one chat line.</summary>
/// <remarks>
/// <see cref="Say"/> is the default channel Version4 uses for <c>SayChatCommand</c>;
/// <see cref="Party"/> is the team channel of <c>PartyChatCommand</c>; <see cref="Announce"/> and
/// <see cref="Server"/> describe world/system text, and <see cref="Private"/> marks a reply that
/// only the requesting speaker may see.
/// </remarks>
public enum ChatChannel : byte
{
  Say = 0,
  Party = 1,
  Announce = 2,
  Server = 3,
  Private = 4
}
