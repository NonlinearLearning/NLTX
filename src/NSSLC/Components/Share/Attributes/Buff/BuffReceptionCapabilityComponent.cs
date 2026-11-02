using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Attributes;

/// <summary>Declares participation in the shared Buff admission path.</summary>
/// <remarks>
/// This capability does not store active Buff state. BuffListComponent and BuffSystem own that state.
/// </remarks>
public readonly struct BuffReceptionCapabilityComponent
{
  private readonly FrozenSet<BuffId>? _allowedBuffs;

  public BuffReceptionMode Mode { get; }

  public IReadOnlySet<BuffId> AllowedBuffs => _allowedBuffs ?? EmptyAllowedBuffs;

  public BuffReceptionCapabilityComponent(
      BuffReceptionMode mode,
      IEnumerable<BuffId>? allowedBuffs = null)
  {
    if (!Enum.IsDefined(mode))
    {
      throw new ArgumentOutOfRangeException(nameof(mode));
    }

    if (mode == BuffReceptionMode.AllowListed && allowedBuffs is null)
    {
      throw new ArgumentNullException(nameof(allowedBuffs));
    }

    Mode = mode;
    _allowedBuffs = allowedBuffs?.ToFrozenSet();
  }

  public bool Allows(BuffId buffId)
  {
    return Mode switch
    {
      BuffReceptionMode.AllowAll => true,
      BuffReceptionMode.AllowListed => AllowedBuffs.Contains(buffId),
      _ => false
    };
  }

  private static IReadOnlySet<BuffId> EmptyAllowedBuffs { get; } =
      Array.Empty<BuffId>().ToFrozenSet();
}
