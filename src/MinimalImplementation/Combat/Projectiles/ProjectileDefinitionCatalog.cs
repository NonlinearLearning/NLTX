using System;
using System.Collections.Generic;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Contains only explicitly migrated Version4 projectile definitions.</summary>
public sealed class ProjectileDefinitionCatalog
{
  private readonly Dictionary<short, ProjectileDefinition> _definitions;

  /// <summary>
  /// Creates the currently migrated Version4 core content facts.
  ///
  /// The list is intentionally explicit. It contains only the type 1
  /// WoodenArrowFriendly facts confirmed in the complete Version4 source;
  /// adding another type requires the same content evidence and profile review.
  /// </summary>
  public static ProjectileDefinitionCatalog CreateVersion4Core()
  {
    return new ProjectileDefinitionCatalog(
      new[]
      {
        new ProjectileDefinition(
          type: 1,
          aiStyle: 1,
          width: 10,
          height: 10,
          defaultTimeLeft: 1200,
          friendly: true,
          hostile: false,
          netImportant: false,
          needsProtocolUuid: false,
          tileCollide: true,
          penetrate: 1,
          aiProfile: ProjectileAiProfileCatalog.Resolve(1))
      });
  }

  public ProjectileDefinitionCatalog(IReadOnlyCollection<ProjectileDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitions = new Dictionary<short, ProjectileDefinition>(definitions.Count);
    foreach (ProjectileDefinition definition in definitions)
    {
      ArgumentNullException.ThrowIfNull(definition);
      if (!_definitions.TryAdd(definition.Type, definition))
      {
        throw new ArgumentException(
          $"Projectile type {definition.Type} has more than one content definition.",
          nameof(definitions));
      }
    }
  }

  public bool TryGet(short type, out ProjectileDefinition definition)
  {
    return _definitions.TryGetValue(type, out definition!);
  }
}
