namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Observes the file and validation boundaries of one world-load attempt.
/// </summary>
public interface IWorldLoadAttemptObserver
{
  void OnFileOpened();

  void OnDocumentValidated();
}
