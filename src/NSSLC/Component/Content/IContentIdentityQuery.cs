namespace Terraria.Content;

public interface IContentIdentityQuery
{
  bool TryGetItemPersistentId(int typeId, out string persistentId);

  bool TryGetItemTypeId(string persistentId, out int typeId);

  bool TryGetNpcPersistentId(int netId, out string persistentId);

  bool TryGetNpcNetId(string persistentId, out int netId);

  bool TryGetProjectilePersistentId(int typeId, out string persistentId);

  bool TryGetProjectileTypeId(string persistentId, out int typeId);

  bool TryGetNpcBestiaryCreditId(int netId, out string bestiaryCreditId);
}
