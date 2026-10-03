using System;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.WorldStorage;

public static class WorldStorageCoordinatorFactory
{
  public static WorldSaveCoordinator CreateSaveCoordinator()
  {
    return CreateSaveCoordinator(new WorldFileStoreAdapter());
  }

  public static WorldSaveCoordinator CreateSaveCoordinator(IWorldFileStore fileStore)
  {
    ArgumentNullException.ThrowIfNull(fileStore);
    return new WorldSaveCoordinator(fileStore);
  }

  public static IWorldSaveEncoder CreateSaveEncoder()
  {
    return new WorldFileDocumentEncoder();
  }

  public static WorldSaveValidationQuery CreateSaveValidationQuery()
  {
    return CreateSaveValidationQuery(
      new WorldFileDocumentDecoder(),
      new WorldFileDocumentValidator());
  }

  public static WorldSaveValidationQuery CreateSaveValidationQuery(
    IWorldPersistenceDocumentDecoder documentDecoder,
    IWorldPersistenceDocumentValidator documentValidator)
  {
    ArgumentNullException.ThrowIfNull(documentDecoder);
    ArgumentNullException.ThrowIfNull(documentValidator);
    return new WorldSaveValidationQuery(bytes =>
    {
      WorldPersistenceDecodeResult decoded = documentDecoder.Decode(bytes);
      return decoded.Succeeded &&
        decoded.Document is not null &&
        documentValidator.Validate(decoded.Document).Kind == WorldStorageFailureKind.None;
    });
  }

  public static WorldLoadCoordinator CreateLoadCoordinator(
    IWorldLoadApiCatalog apiCatalog)
  {
    return CreateLoadCoordinator(
      new WorldFileDocumentDecoder(),
      new WorldFileDocumentValidator(),
      apiCatalog);
  }

  public static WorldLoadCoordinator CreateLoadCoordinator(
    IWorldPersistenceDocumentDecoder documentDecoder,
    IWorldPersistenceDocumentValidator documentValidator,
    IWorldLoadApiCatalog apiCatalog)
  {
    return CreateLoadCoordinator(
      new WorldFileStoreAdapter(),
      documentDecoder,
      documentValidator,
      apiCatalog);
  }

  public static WorldLoadCoordinator CreateLoadCoordinator(
    IWorldFileStore fileStore,
    IWorldPersistenceDocumentDecoder documentDecoder,
    IWorldPersistenceDocumentValidator documentValidator,
    IWorldLoadApiCatalog apiCatalog)
  {
    return new WorldLoadCoordinator(
      fileStore,
      documentDecoder,
      documentValidator,
      apiCatalog);
  }
}
