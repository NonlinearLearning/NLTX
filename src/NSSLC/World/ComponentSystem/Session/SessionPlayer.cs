using System.Numerics;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Session;

/// <summary>One player bound to an active session, as read by other domains.</summary>
/// <remarks>
/// This is a projection, not a second state store: the session binding is authoritative and the
/// spatial fields are copied at read time. Domains that need "who is in the world right now"
/// consume this through <see cref="ISessionPlayerSource"/> instead of querying player components
/// themselves.
/// </remarks>
public readonly record struct SessionPlayer(
    SessionEpoch Epoch,
    EntityRuntimeHandle Handle,
    Vector2 Position,
    Vector2 Size,
    bool IsAlive);
