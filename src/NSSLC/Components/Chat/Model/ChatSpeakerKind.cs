namespace Terraria.Chat;

/// <summary>Who produced a chat line, as far as the server cares.</summary>
/// <remarks>
/// Version4 carries the speaker as a player index plus a color; NPC dialogue and server messages
/// share the same display path with author <see cref="byte.MaxValue"/>. The kind only records what
/// the server itself knows about the author.
/// </remarks>
public enum ChatSpeakerKind : byte
{
  Player = 0,
  Npc = 1,
  System = 2
}
