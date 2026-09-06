namespace Terraria.LeashedEntity;

/// <summary>
/// Stores the definition binding for one leashed entity.
/// status: proposed
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
public struct LeashedEntityStateComponent
{
  /// <summary>
  /// The Version4 registry definition reference. Zero means unbound.
  /// </summary>
  public int DefinitionId;

  public LeashedEntityStateComponent(int definitionId)
  {
    DefinitionId = definitionId;
  }
}
