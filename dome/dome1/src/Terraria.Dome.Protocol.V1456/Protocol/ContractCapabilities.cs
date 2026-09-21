namespace Terraria.Dome.Protocol.V1456.Protocol;

public enum ContractNegotiationState
{
  NotStarted,
  Negotiated,
  Rejected
}

public readonly record struct ContractCapabilityOffer(
  ushort SignDeletionVersions,
  ushort ChestTransferRevisionVersions)
{
  public ushort NpcProjectileVersions { get; init; }

  public ushort NpcStatusEffectVersions { get; init; }
}

public readonly record struct ContractCapabilityAck(
  ushort SignDeletionVersions,
  ushort ChestTransferRevisionVersions)
{
  public ushort NpcProjectileVersions { get; init; }

  public ushort NpcStatusEffectVersions { get; init; }
}

public readonly record struct SessionContractCapabilities(
  ContractNegotiationState State,
  ushort SignDeletionVersions,
  ushort ChestTransferRevisionVersions)
{
  public ushort NpcProjectileVersions { get; init; }

  public ushort NpcStatusEffectVersions { get; init; }

  public bool SupportsSignDeletion =>
    State == ContractNegotiationState.Negotiated && (SignDeletionVersions & 1) != 0;

  public bool SupportsChestTransferRevision =>
    State == ContractNegotiationState.Negotiated && (ChestTransferRevisionVersions & 1) != 0;

  public bool SupportsNpcProjectile =>
    SupportsNpcProjectileV1 || SupportsNpcProjectileV2 || SupportsNpcProjectileV3;

  public bool SupportsNpcProjectileV1 =>
    State == ContractNegotiationState.Negotiated && (NpcProjectileVersions & 1) != 0;

  public bool SupportsNpcProjectileV2 =>
    State == ContractNegotiationState.Negotiated && (NpcProjectileVersions & 2) != 0;

  public bool SupportsNpcProjectileV3 =>
    State == ContractNegotiationState.Negotiated && (NpcProjectileVersions & 4) != 0;

  public bool SupportsNpcStatusEffect =>
    State == ContractNegotiationState.Negotiated && (NpcStatusEffectVersions & 1) != 0;

  public static SessionContractCapabilities NotNegotiated => new(
    ContractNegotiationState.NotStarted,
    0,
    0);
}
