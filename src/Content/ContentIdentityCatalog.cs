using System.Collections.Frozen;

namespace Terraria.Content;

public sealed class ContentIdentityCatalog : IContentIdentityQuery
{
  private readonly FrozenDictionary<int, string> _itemPersistentIdByType;
  private readonly FrozenDictionary<string, int> _itemTypeByPersistentId;
  private readonly FrozenDictionary<int, string> _npcPersistentIdByNetId;
  private readonly FrozenDictionary<string, int> _npcNetIdByPersistentId;
  private readonly FrozenDictionary<int, string> _projectilePersistentIdByType;
  private readonly FrozenDictionary<string, int> _projectileTypeByPersistentId;
  private readonly FrozenDictionary<int, string> _npcBestiaryCreditIdByNetId;

  public ContentIdentityCatalog(
    IReadOnlyDictionary<int, string> itemPersistentIdByType,
    IReadOnlyDictionary<int, string> npcPersistentIdByNetId,
    IReadOnlyDictionary<int, string>? projectilePersistentIdByType = null,
    IReadOnlyDictionary<int, string>? npcBestiaryCreditIdByNetId = null)
  {
    ArgumentNullException.ThrowIfNull(itemPersistentIdByType);
    ArgumentNullException.ThrowIfNull(npcPersistentIdByNetId);
    _itemPersistentIdByType = itemPersistentIdByType.ToFrozenDictionary();
    _itemTypeByPersistentId = itemPersistentIdByType.ToFrozenDictionary(
      pair => pair.Value,
      pair => pair.Key,
      StringComparer.Ordinal);
    _npcPersistentIdByNetId = npcPersistentIdByNetId.ToFrozenDictionary();
    _npcNetIdByPersistentId = npcPersistentIdByNetId.ToFrozenDictionary(
      pair => pair.Value,
      pair => pair.Key,
      StringComparer.Ordinal);
    IReadOnlyDictionary<int, string> projectileIds = projectilePersistentIdByType ??
      new Dictionary<int, string>();
    _projectilePersistentIdByType = projectileIds.ToFrozenDictionary();
    _projectileTypeByPersistentId = projectileIds.ToFrozenDictionary(
      pair => pair.Value,
      pair => pair.Key,
      StringComparer.Ordinal);
    IReadOnlyDictionary<int, string> bestiaryCredits = npcBestiaryCreditIdByNetId ??
      new Dictionary<int, string>();
    _npcBestiaryCreditIdByNetId = bestiaryCredits.ToFrozenDictionary();
  }

  public bool TryGetItemPersistentId(int typeId, out string persistentId)
  {
    return _itemPersistentIdByType.TryGetValue(typeId, out persistentId!);
  }

  public FrozenDictionary<int, string> ItemPersistentIdByType => _itemPersistentIdByType;

  public FrozenDictionary<string, int> ItemTypeByPersistentId => _itemTypeByPersistentId;

  public FrozenDictionary<int, string> NpcPersistentIdByNetId => _npcPersistentIdByNetId;

  public FrozenDictionary<string, int> NpcNetIdByPersistentId => _npcNetIdByPersistentId;

  public FrozenDictionary<int, string> NpcBestiaryCreditIdByNetId => _npcBestiaryCreditIdByNetId;

  public FrozenDictionary<int, string> ProjectilePersistentIdByType => _projectilePersistentIdByType;

  public FrozenDictionary<string, int> ProjectileTypeByPersistentId => _projectileTypeByPersistentId;

  public bool TryGetItemTypeId(string persistentId, out int typeId)
  {
    ArgumentNullException.ThrowIfNull(persistentId);
    return _itemTypeByPersistentId.TryGetValue(persistentId, out typeId);
  }

  public bool TryGetNpcPersistentId(int netId, out string persistentId)
  {
    return _npcPersistentIdByNetId.TryGetValue(netId, out persistentId!);
  }

  public bool TryGetNpcNetId(string persistentId, out int netId)
  {
    ArgumentNullException.ThrowIfNull(persistentId);
    return _npcNetIdByPersistentId.TryGetValue(persistentId, out netId);
  }

  public bool TryGetProjectilePersistentId(int typeId, out string persistentId)
  {
    return _projectilePersistentIdByType.TryGetValue(typeId, out persistentId!);
  }

  public bool TryGetProjectileTypeId(string persistentId, out int typeId)
  {
    ArgumentNullException.ThrowIfNull(persistentId);
    return _projectileTypeByPersistentId.TryGetValue(persistentId, out typeId);
  }

  public bool TryGetNpcBestiaryCreditId(int netId, out string bestiaryCreditId)
  {
    return _npcBestiaryCreditIdByNetId.TryGetValue(netId, out bestiaryCreditId!);
  }
}
