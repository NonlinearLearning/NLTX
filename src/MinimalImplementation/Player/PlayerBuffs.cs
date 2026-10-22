using System;
using System.Collections.Generic;
using System.Linq;

namespace MinimalImplementation.Server.Player;

public sealed class PlayerBuffs
{
  public const int MaximumBuffSlots = 44;
  public const int ClientReportedDurationTicks = 60;

  private readonly ushort[] _types;
  private readonly int[] _times;
  private readonly bool[] _authoritative;

  private PlayerBuffs(ushort[] types, int[] times, bool[] authoritative)
  {
    _types = types;
    _times = times;
    _authoritative = authoritative;
    Types = Array.AsReadOnly(_types);
    Times = Array.AsReadOnly(_times);
  }

  public static PlayerBuffs Empty => new(
    new ushort[MaximumBuffSlots], new int[MaximumBuffSlots], new bool[MaximumBuffSlots]);

  public IReadOnlyList<ushort> Types { get; }

  public IReadOnlyList<int> Times { get; }

  public bool HasBuff(ushort type)
  {
    return FindBuff(type) >= 0;
  }

  public int FindBuff(ushort type)
  {
    if (type == 0)
    {
      return -1;
    }

    for (int index = 0; index < MaximumBuffSlots; index++)
    {
      if (_types[index] == type && _times[index] > 0)
      {
        return index;
      }
    }

    return -1;
  }

  public bool TryGetAuthoritativeTime(ushort type, out int time)
  {
    int slot = FindBuff(type);
    time = slot >= 0 && _authoritative[slot] ? _times[slot] : 0;
    return time > 0;
  }

  /// <summary>
  /// Applies the same-version AddBuff duration, refresh, replacement and eviction rules.
  /// The caller supplies actual immunity/equipment facts; this method performs no I/O.
  /// </summary>
  public bool TryAddAuthoritative(
    ushort type,
    int durationTicks,
    PlayerBuffApplicationContext context,
    out PlayerBuffs updated)
  {
    return TryAddAuthoritative(type, durationTicks, context, out updated, out _);
  }

  /// <summary>Also returns the scaled add amount for a single self-directed packet 55.</summary>
  public bool TryAddAuthoritative(
      ushort type,
      int durationTicks,
      PlayerBuffApplicationContext context,
      out PlayerBuffs updated,
      out int durationToAdd) {
    ArgumentNullException.ThrowIfNull(context);
    updated = this;
    durationToAdd = 0;
    if (!Version4PlayerBuffCatalog.IsValidBuff(type) || durationTicks <= 0 || context.IsImmune)
    {
      return false;
    }

    int time = durationTicks;
    if (!context.FromNetPvp)
    {
      if (context.HasPotionStation && Version4PlayerBuffCatalog.IsExtendedByPotionStation(type))
      {
        time = ScaleDuration(time, 1.2f);
      }

      if (context.IsExpertMode && Version4PlayerBuffCatalog.IsExtendedByDifficulty(type))
      {
        time = ScaleDuration(time, context.DifficultyDebuffTimeMultiplier);
      }
    }

    if (time <= 0)
    {
      return false;
    }

    durationToAdd = time;
    ushort[] types = _types.ToArray();
    int[] times = _times.ToArray();
    bool[] authoritative = _authoritative.ToArray();
    if (Version4PlayerBuffCatalog.IsFedState(type))
    {
      RemoveMatching(types, times, authoritative, Version4PlayerBuffCatalog.IsFedState);
    }

    int existing = Array.IndexOf(types, type);
    if (existing >= 0)
    {
      int cap = Version4PlayerBuffCatalog.AdditiveTimeCap(type);
      times[existing] = cap > 0
        ? (int)Math.Min(cap, (long)times[existing] + time)
        : Math.Max(times[existing], time);
      authoritative[existing] = true;
      updated = new PlayerBuffs(types, times, authoritative);
      return true;
    }

    if (Version4PlayerBuffCatalog.IsLightPet(type))
    {
      RemoveMatching(types, times, authoritative, Version4PlayerBuffCatalog.IsLightPet);
    }

    if (Version4PlayerBuffCatalog.IsVanityPet(type))
    {
      RemoveMatching(types, times, authoritative, Version4PlayerBuffCatalog.IsVanityPet);
    }

    if (Version4PlayerBuffCatalog.IsMeleeBuff(type))
    {
      RemoveMatching(types, times, authoritative, Version4PlayerBuffCatalog.IsMeleeBuff);
    }

    int slot;
    while (true)
    {
      int removable = Array.FindIndex(types, value => !Version4PlayerBuffCatalog.IsDebuff(value));
      if (removable < 0)
      {
        return false;
      }

      slot = Array.IndexOf(types, (ushort)0, removable);
      if (slot >= 0)
      {
        break;
      }

      RemoveAt(types, times, authoritative, removable);
    }

    types[slot] = type;
    times[slot] = time;
    authoritative[slot] = true;
    updated = new PlayerBuffs(types, times, authoritative);
    return true;
  }

  public PlayerBuffs Remove(ushort type)
  {
    if (!HasBuff(type))
    {
      return this;
    }

    ushort[] types = _types.ToArray();
    int[] times = _times.ToArray();
    bool[] authoritative = _authoritative.ToArray();
    RemoveMatching(types, times, authoritative, value => value == type);
    return new PlayerBuffs(types, times, authoritative);
  }

  /// <summary>Clears nonpersistent buffs without advancing surviving durations.</summary>
  public PlayerBuffs UpdateDead() {
    if (!_types.Any(type => type != 0 && !Version4PlayerBuffCatalog.PersistsAfterDeath(type))) {
      return this;
    }

    ushort[] types = _types.ToArray();
    int[] times = _times.ToArray();
    bool[] authoritative = _authoritative.ToArray();
    RemoveMatching(types, times, authoritative,
      type => !Version4PlayerBuffCatalog.PersistsAfterDeath(type));
    return new PlayerBuffs(types, times, authoritative);
  }

  public PlayerBuffs AdvanceTick()
  {
    ushort[] types = _types.ToArray();
    int[] times = _times.ToArray();
    bool[] authoritative = _authoritative.ToArray();
    bool changed = false;
    for (int index = 0; index < times.Length; index++)
    {
      if (times[index] <= 0)
      {
        if (types[index] != 0 || times[index] != 0)
        {
          types[index] = 0;
          times[index] = 0;
          changed = true;
        }

        continue;
      }

      if (authoritative[index]
        && Version4PlayerBuffCatalog.TimeLeftDoesNotDecrease(types[index]))
      {
        continue;
      }

      times[index]--;
      changed = true;
      if (times[index] == 0)
      {
        types[index] = 0;
      }
    }

    if (!changed)
    {
      return this;
    }

    Compact(types, times, authoritative);
    return new PlayerBuffs(types, times, authoritative);
  }

  public bool TryReplaceClientTypes(
    IReadOnlyList<ushort> requestedTypes,
    Func<ushort, bool> isValidBuff,
    out PlayerBuffs updated)
  {
    ArgumentNullException.ThrowIfNull(requestedTypes);
    ArgumentNullException.ThrowIfNull(isValidBuff);
    if (requestedTypes.Count > MaximumBuffSlots
      || requestedTypes.Any(type => type == 0 || !isValidBuff(type)))
    {
      updated = this;
      return false;
    }

    ushort[] types = new ushort[MaximumBuffSlots];
    int[] times = new int[MaximumBuffSlots];
    bool[] authoritative = new bool[MaximumBuffSlots];
    int count = 0;
    for (int index = 0; index < requestedTypes.Count; index++)
    {
      ushort type = requestedTypes[index];
      if (Array.IndexOf(types, type) >= 0)
      {
        continue;
      }

      int previous = FindBuff(type);
      bool owned = previous >= 0 && _authoritative[previous];
      types[count] = type;
      times[count] = owned ? _times[previous] : ClientReportedDurationTicks;
      authoritative[count++] = owned;
    }

    // A type-only client report cannot remove or shorten a server-owned duration.
    for (int index = 0; index < MaximumBuffSlots; index++)
    {
      if (!_authoritative[index] || _times[index] <= 0
        || Array.IndexOf(types, _types[index]) >= 0)
      {
        continue;
      }

      if (count == MaximumBuffSlots)
      {
        updated = this;
        return false;
      }

      types[count] = _types[index];
      times[count] = _times[index];
      authoritative[count++] = true;
    }

    updated = new PlayerBuffs(types, times, authoritative);
    return true;
  }

  private static void RemoveMatching(
    ushort[] types, int[] times, bool[] authoritative, Func<ushort, bool> matches)
  {
    for (int index = 0; index < types.Length; index++)
    {
      if (matches.Invoke(types[index]))
      {
        types[index] = 0;
        times[index] = 0;
        authoritative[index] = false;
      }
    }

    Compact(types, times, authoritative);
  }

  private static int ScaleDuration(int time, float multiplier)
  {
    // Preserve the source's float multiplication and truncation for ordinary durations.
    // Clamp before converting: int.MaxValue itself rounds up when represented as float.
    float scaled = time * multiplier;
    return (double)scaled >= int.MaxValue ? int.MaxValue : (int)scaled;
  }

  private static void RemoveAt(ushort[] types, int[] times, bool[] authoritative, int slot)
  {
    types[slot] = 0;
    times[slot] = 0;
    authoritative[slot] = false;
    Compact(types, times, authoritative);
  }

  private static void Compact(ushort[] types, int[] times, bool[] authoritative)
  {
    int destination = 0;
    for (int index = 0; index < types.Length; index++)
    {
      if (types[index] == 0 || times[index] <= 0)
      {
        continue;
      }

      types[destination] = types[index];
      times[destination] = times[index];
      authoritative[destination++] = authoritative[index];
    }

    Array.Clear(types, destination, types.Length - destination);
    Array.Clear(times, destination, times.Length - destination);
    Array.Clear(authoritative, destination, authoritative.Length - destination);
  }
}
