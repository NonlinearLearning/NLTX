using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Unshared.Chat;

/// <summary>Locates the single entity that carries the chat broadcast queue.</summary>
public static class ChatBroadcastQueue
{
  public static ChatBroadcastQueueComponent Require(ArchWorld world)
    => SingletonComponent<ChatBroadcastQueueComponent>.Require(
        world,
        "A ChatBroadcastQueueComponent entity must exist before chat lines can be delivered.");
}
