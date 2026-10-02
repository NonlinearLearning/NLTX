namespace Terraria.LeashedEntity;

/// <summary>
/// Stores the definition binding for one leashed entity.
/// status: implemented
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
public struct LeashedEntityStateComponent
{
  /// <summary>
  /// The Version4 registry definition reference. Zero means unbound.
  /// </summary>
  public int DefinitionId;

  /// <summary>
  /// Indicates whether the entity has a validated definition binding.
  /// </summary>
  public bool IsBound => DefinitionId > 0;

  public LeashedEntityStateComponent(int definitionId)
  {
    DefinitionId = definitionId;
  }
}
