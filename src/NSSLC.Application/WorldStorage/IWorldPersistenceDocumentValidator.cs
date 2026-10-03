namespace Terraria.NonAuthoritative.Persistence;

public interface IWorldPersistenceDocumentValidator
{
  WorldStorageFailure Validate(WorldPersistenceDocument document);
}
