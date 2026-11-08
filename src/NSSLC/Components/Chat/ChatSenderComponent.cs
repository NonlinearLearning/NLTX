using System;

namespace Terraria.Chat;

/// <summary>Capability component: an entity that declares it may speak in chat.</summary>
/// <remarks>
/// Declaring the component is the whole admission rule — the Chat domain never inspects an entity's
/// kind, session or AI to decide whether it may talk. <see cref="IsOperator"/> gates commands that
/// Version4 restricts to administrators, and <see cref="DisplayName"/> is the name the server
/// prefixes to the line.
/// </remarks>
public readonly record struct ChatSenderComponent(
    ChatSpeakerKind Kind,
    string DisplayName,
    bool IsOperator)
{
  /// <summary>Name used until the client's player name is decoded.</summary>
  /// <remarks>
  /// Version4 sends the player name in the player-data packet, whose layout is not preserved in the
  /// available reference dump, so the component starts with this placeholder and the name can be
  /// replaced later without touching the Chat domain.
  /// </remarks>
  public const string FallbackName = "Player";

  public static ChatSenderComponent Player(string? displayName = null)
    => new(
        ChatSpeakerKind.Player,
        string.IsNullOrWhiteSpace(displayName) ? FallbackName : displayName.Trim(),
        IsOperator: false);

  public static ChatSenderComponent Server(string displayName)
    => new(ChatSpeakerKind.System, displayName, IsOperator: true);

  public bool IsValid => Enum.IsDefined(Kind) && DisplayName.Length > 0;
}
