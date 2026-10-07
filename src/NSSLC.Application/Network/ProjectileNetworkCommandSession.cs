using Terraria.Content;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Projectile;

namespace Terraria.Network;

/// <summary>
/// The current world dependencies used for one projectile network command.
/// The session query creates this value on the EntityRuntime owner thread.
/// </summary>
public sealed class ProjectileNetworkCommandSession
{
  public ProjectileNetworkCommandSession(
    NetworkSessionContext sender,
    LoadedWorldSession worldSession,
    IProjectileDefinitionQuery projectileDefinitions,
    ProjectileDefinitionHydrationContext hydrationContext)
  {
    ArgumentNullException.ThrowIfNull(sender);
    ArgumentNullException.ThrowIfNull(worldSession);
    ArgumentNullException.ThrowIfNull(projectileDefinitions);
    if (!ReferenceEquals(worldSession.EntityRuntime, worldSession.Storage.EntityRuntime))
    {
      throw new ArgumentException(
        "The projectile session must use the world's canonical EntityRuntime.",
        nameof(worldSession));
    }

    Sender = sender;
    WorldSession = worldSession;
    ProjectileDefinitions = projectileDefinitions;
    HydrationContext = hydrationContext;
  }

  public NetworkSessionContext Sender { get; }

  public LoadedWorldSession WorldSession { get; }

  public IProjectileDefinitionQuery ProjectileDefinitions { get; }

  public ProjectileDefinitionHydrationContext HydrationContext { get; }
}
