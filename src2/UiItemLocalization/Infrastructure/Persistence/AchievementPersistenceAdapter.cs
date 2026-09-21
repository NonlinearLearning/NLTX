using System.Text.Json;
using Terraria.UiItemLocalization.Achievements;

namespace Terraria.UiItemLocalization.Infrastructure.Persistence;

public sealed class AchievementPersistenceAdapter
{
  private readonly IAchievementPersistenceStore _store;
  private readonly JsonSerializerOptions _serializerOptions;

  public AchievementPersistenceAdapter(
    IAchievementPersistenceStore store,
    JsonSerializerOptions? serializerOptions = null)
  {
    _store = store ?? throw new ArgumentNullException(nameof(store));
    _serializerOptions = serializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.General);
  }

  public SaveResult Save(
    string path,
    bool cloudSave,
    AchievementSaveSnapshot snapshot)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    ArgumentNullException.ThrowIfNull(snapshot);
    string content = JsonSerializer.Serialize(snapshot, _serializerOptions);
    _store.Write(path, content, cloudSave);
    return new SaveResult(true, null);
  }

  public LoadResult Load(string path, bool cloudSave)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    if (!_store.TryRead(path, cloudSave, out string? content) || string.IsNullOrWhiteSpace(content))
    {
      return new LoadResult(false, null, "not-found");
    }

    try
    {
      AchievementSaveSnapshot? snapshot = JsonSerializer.Deserialize<AchievementSaveSnapshot>(
        content,
        _serializerOptions);
      return snapshot is null
        ? new LoadResult(false, null, "empty-snapshot")
        : new LoadResult(true, snapshot, null);
    }
    catch (JsonException)
    {
      return new LoadResult(false, null, "invalid-json");
    }
  }

  public readonly record struct SaveResult(bool Succeeded, string? FailureReason);

  public readonly record struct LoadResult(
    bool Succeeded,
    AchievementSaveSnapshot? Snapshot,
    string? FailureReason);
}
