using Terraria.Share.Relationships;

namespace Terraria.Unshared.Chat;

/// <summary>One cross-domain intention to speak: text typed by an entity that can chat.</summary>
/// <remarks>
/// The Communication layer decodes the text module frame and raises this request. The Chat domain
/// owns the text rules, the command catalog and the broadcast facts; the sender is identified by
/// its runtime handle so the Chat domain never reads session state.
/// </remarks>
public readonly record struct ChatMessageRequest(
    EntityRuntimeHandle Sender,
    ChatChannel Channel,
    string Text);
