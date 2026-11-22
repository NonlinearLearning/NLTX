using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Chat;

/// <summary>Locates the single entity that carries the chat request queue.</summary>
public static class ChatMessageRequestQueue
{
  public static ChatMessageRequestQueueComponent Require(ArchWorld world)
    => SingletonComponent<ChatMessageRequestQueueComponent>.Require(
        world,
        "A ChatMessageRequestQueueComponent entity must exist before chat lines can arrive.");
}
