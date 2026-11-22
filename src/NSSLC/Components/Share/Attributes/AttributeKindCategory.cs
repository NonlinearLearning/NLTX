namespace Terraria.Attributes;

/// <summary>
/// Describes which kind of state an attribute key represents.
/// </summary>
public enum AttributeKindCategory : byte
{
  /// <summary>A derived numeric value that may receive modifiers.</summary>
  ComputedScalar,

  /// <summary>A derived numeric capacity, such as maximum shield.</summary>
  Capacity,

  /// <summary>A current resource pool. It must have a dedicated state owner.</summary>
  ResourcePool,

  /// <summary>A remaining timer owned by a cooldown state.</summary>
  Cooldown,

  /// <summary>An identity or relationship value, not a numeric attribute.</summary>
  Identity,

  /// <summary>A state flag that requires a concrete behavior or status owner.</summary>
  StatusFlag,

  /// <summary>A derived rule parameter consumed by a dedicated system.</summary>
  Policy
}
