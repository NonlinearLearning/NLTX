namespace Terraria.NonAuthoritative.Persistence;

public interface IWorldPersistenceDocumentDecoder
{
  /// <summary>
  /// Transfers the decoded document to the caller. The decoder must not mutate or retain mutable
  /// section values after returning.
  /// </summary>
  WorldPersistenceDecodeResult Decode(ReadOnlyMemory<byte> fileBytes);
}
