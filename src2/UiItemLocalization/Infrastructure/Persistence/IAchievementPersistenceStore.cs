namespace Terraria.UiItemLocalization.Infrastructure.Persistence;

public interface IAchievementPersistenceStore
{
  void Write(string path, string content, bool cloudSave);

  bool TryRead(string path, bool cloudSave, out string? content);
}
