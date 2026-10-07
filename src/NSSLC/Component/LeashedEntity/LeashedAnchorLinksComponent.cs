using Terraria.Relationships;

namespace Terraria.LeashedEntity;

/// <summary>
/// Runtime reverse links for leashed entities that use this entity as their anchor.
/// </summary>
public readonly struct LeashedAnchorLinksComponent
{
  private readonly EntityReference[]? _members;

  public int Count => _members?.Length ?? 0;

  public bool Contains(EntityReference member)
  {
    return _members is not null && Array.IndexOf(_members, member) >= 0;
  }

  public EntityReference[] CopyMembers()
  {
    return _members is null ? Array.Empty<EntityReference>() : (EntityReference[])_members.Clone();
  }

  internal static LeashedAnchorLinksComponent FromMembers(IEnumerable<EntityReference> members)
  {
    ArgumentNullException.ThrowIfNull(members);
    EntityReference[] copy = members.Distinct().ToArray();
    return copy.Length == 0 ? default : new LeashedAnchorLinksComponent(copy);
  }

  public LeashedAnchorLinksComponent WithAdded(EntityReference member)
  {
    if (Contains(member))
    {
      return this;
    }

    EntityReference[] members = CopyMembers();
    Array.Resize(ref members, members.Length + 1);
    members[^1] = member;
    return new LeashedAnchorLinksComponent(members);
  }

  public LeashedAnchorLinksComponent WithRemoved(EntityReference member)
  {
    if (_members is null)
    {
      return this;
    }

    int index = Array.IndexOf(_members, member);
    if (index < 0)
    {
      return this;
    }

    if (_members.Length == 1)
    {
      return default;
    }

    EntityReference[] members = new EntityReference[_members.Length - 1];
    if (index > 0)
    {
      Array.Copy(_members, 0, members, 0, index);
    }

    if (index < _members.Length - 1)
    {
      Array.Copy(_members, index + 1, members, index, _members.Length - index - 1);
    }

    return new LeashedAnchorLinksComponent(members);
  }

  private LeashedAnchorLinksComponent(EntityReference[] members)
  {
    _members = members;
  }
}
