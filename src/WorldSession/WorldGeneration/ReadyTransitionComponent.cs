using System;

namespace Terraria.WorldGeneration.Components;

public readonly record struct ReadyTransitionComponent
{
  public ReadyTransitionComponent(
    long generationId,
    ulong generationRevision,
    bool passesValidated = false,
    bool liquidStable = false,
    bool housingComplete = false,
    bool persistenceCommitted = false,
    ulong? publicationRevision = null,
    string? failureReason = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    GenerationRevision = generationRevision;
    PassesValidated = passesValidated;
    LiquidStable = liquidStable;
    HousingComplete = housingComplete;
    PersistenceCommitted = persistenceCommitted;
    PublicationRevision = publicationRevision;
    FailureReason = failureReason;
  }

  public long GenerationId { get; }

  public ulong GenerationRevision { get; }

  public bool PassesValidated { get; }

  public bool LiquidStable { get; }

  public bool HousingComplete { get; }

  public bool PersistenceCommitted { get; }

  public ulong? PublicationRevision { get; }

  public string? FailureReason { get; }

  public bool IsReadyCandidate =>
    FailureReason is null &&
    PassesValidated &&
    LiquidStable &&
    HousingComplete &&
    PersistenceCommitted &&
    PublicationRevision.HasValue;
}
