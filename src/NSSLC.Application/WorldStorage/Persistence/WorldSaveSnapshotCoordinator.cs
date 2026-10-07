using System;
using System.Threading;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Captures and persists a world while transformations are excluded by the shared world I/O gate.
/// </summary>
public sealed class WorldSaveSnapshotCoordinator
{
  private readonly WorldSaveCoordinator _saveCoordinator;
  private readonly WorldTransformTransactionComponent _transactions;
  private readonly IWorldStorageIoGate _ioGate;
  private readonly IWorldPersistenceSnapshotSource _snapshotSource;
  private readonly IWorldSaveEncoder _encoder;
  private readonly WorldSaveValidationQuery _validation;
  private readonly IWorldSaveSnapshotPreparation? _snapshotPreparation;

  public WorldSaveSnapshotCoordinator(
    WorldSaveCoordinator saveCoordinator,
    WorldTransformTransactionComponent transactions,
    IWorldStorageIoGate ioGate,
    IWorldPersistenceSnapshotSource snapshotSource,
    IWorldSaveEncoder encoder,
    WorldSaveValidationQuery validation,
    IWorldSaveSnapshotPreparation? snapshotPreparation = null)
  {
    _saveCoordinator = saveCoordinator ?? throw new ArgumentNullException(nameof(saveCoordinator));
    _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
    _ioGate = ioGate ?? throw new ArgumentNullException(nameof(ioGate));
    _snapshotSource = snapshotSource ?? throw new ArgumentNullException(nameof(snapshotSource));
    _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
    _validation = validation ?? throw new ArgumentNullException(nameof(validation));
    _snapshotPreparation = snapshotPreparation;
  }

  public WorldSaveProjection Save(
    string path,
    WorldBackupPolicy backupPolicy,
    CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return Reject(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidPath,
          "A world save path is required."));
    }

    try
    {
      WorldTransformTransactionSystem.WaitUntilIdle(_transactions, cancellationToken);
      using IDisposable ioLease = _ioGate.Enter(cancellationToken);
      if (_snapshotPreparation is not null)
      {
        WorldStorageOperationResult preparation = _snapshotPreparation.Prepare(cancellationToken);
        if (!preparation.Succeeded)
        {
          return Reject(preparation.Failure);
        }
      }

      WorldPersistenceDocument document = _snapshotSource.Capture(cancellationToken);
      if (document is null)
      {
        return Reject(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world snapshot source returned no persistence document."));
      }

      var command = new WorldSaveCommand(path, document, backupPolicy);
      return _saveCoordinator.Save(command, _encoder, _validation);
    }
    catch (Exception exception)
    {
      return Reject(
        WorldStorageExceptionClassifier.ClassifyExternalFailure(
          exception,
          WorldStorageFailureKind.Unknown));
    }
  }

  private static WorldSaveProjection Reject(WorldStorageFailure failure)
  {
    return new WorldSaveProjection(
      committed: false,
      stages: new[] { WorldSaveStage.Capture },
      failure: failure);
  }
}
