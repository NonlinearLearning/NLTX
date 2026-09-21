namespace Terraria.EntityLifecycleAttribution;

public sealed class ProjectileIdentityIndex
{
  private readonly Dictionary<(int Owner, int Identity), EntitySlotHandle> _entries = new();

  public bool TryRegister(int owner, int identity, EntitySlotHandle handle)
  {
    if (owner < 0 || identity < 0 || !handle.IsValid || _entries.ContainsKey((owner, identity)))
    {
      return false;
    }

    _entries.Add((owner, identity), handle);
    return true;
  }

  public bool TryResolve(int owner, int identity, out EntitySlotHandle handle)
  {
    return _entries.TryGetValue((owner, identity), out handle);
  }

  public bool Remove(int owner, int identity, EntitySlotHandle handle)
  {
    return _entries.TryGetValue((owner, identity), out EntitySlotHandle current) &&
           current == handle &&
           _entries.Remove((owner, identity));
  }

  public void Clear()
  {
    _entries.Clear();
  }
}
