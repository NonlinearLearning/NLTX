using System.Collections.ObjectModel;
using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

public sealed record LeashedSectionSnapshot
{
  public LeashedSectionSnapshot(
    SectionCoordinate section,
    bool isActive,
    IReadOnlyList<LeashedEntityHandle> handles,
    int emptySlots)
  {
    Section = section;
    IsActive = isActive;
    Handles = new ReadOnlyCollection<LeashedEntityHandle>(handles.ToArray());
    EmptySlots = emptySlots;
  }

  public SectionCoordinate Section { get; }

  public bool IsActive { get; }

  public IReadOnlyList<LeashedEntityHandle> Handles { get; }

  public int SlotCount => Handles.Count + EmptySlots;

  public int ActiveCount => Handles.Count;

  public int EmptySlots { get; }
}

