using Terraria.Relationships;

namespace Terraria.LeashedEntity;

/// <summary>
/// Runtime reverse links for leashed entities that use this entity as their anchor.
/// </summary>
/// <remarks>
/// <para>职责：保存锚点所绑定的拴系实体集合。</para>
/// <para>拆分来源：由 LeashedEntity.AnchorPosition 的锚点关联流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.GameContent/LeashedEntity.cs。</para>
/// <para>重组说明：锚点到拴系实体的反向成员集合是拆分时新增的索引。</para>
/// </remarks>
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
