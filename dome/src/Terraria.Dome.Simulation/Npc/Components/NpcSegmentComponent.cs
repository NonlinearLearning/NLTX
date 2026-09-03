using System;

namespace Terraria.Dome.Simulation.Npc.Components;

public enum NpcSegmentLifePolicy
{
  Independent = 0,
  RootShared = 1
}

public struct NpcSegmentComponent
{
  public NpcSegmentComponent(
    NpcHandle root,
    NpcHandle parent,
    NpcHandle child,
    int segmentIndex,
    bool isRoot,
    NpcSegmentLifePolicy lifePolicy)
  {
    if (!root.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(root));
    }

    if (segmentIndex < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(segmentIndex));
    }

    if (!Enum.IsDefined(lifePolicy))
    {
      throw new ArgumentOutOfRangeException(nameof(lifePolicy));
    }

    if (isRoot && parent.IsValid)
    {
      throw new ArgumentException(
        "A segment root cannot have a parent and may only point to a valid child.",
        nameof(parent));
    }

    if (!isRoot && !parent.IsValid)
    {
      throw new ArgumentException("A non-root segment must have a parent.", nameof(parent));
    }

    Root = root;
    Parent = parent;
    Child = child;
    SegmentIndex = segmentIndex;
    IsRoot = isRoot;
    LifePolicy = lifePolicy;
  }

  public NpcHandle Root;
  public NpcHandle Parent;
  public NpcHandle Child;
  public int SegmentIndex;
  public bool IsRoot;
  public NpcSegmentLifePolicy LifePolicy;
}
