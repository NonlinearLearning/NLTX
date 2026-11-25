using System;

namespace Terraria.Share.Combat;

/// <summary>Stores one post-hit immunity window per damage channel, as an absolute expiry tick.</summary>
/// <remarks>
/// 时间表示是到期戳而不是剩余计数：递减模型每 tick 都要写每一个激活中的实体，
/// 写放大约 O(激活数 x tick)；到期戳只在授予时写一次，读的时候比一次整数。
/// <para>
/// 边界惯例：授予于 tick N、时长 D，则 _untilTick = N + D，且 IsActive(t) 在 t &lt; N + D 时为真。
/// 也就是 N..N+D-1 全部生效、N+D 起放行，受保护的 tick 数恰好是 D。
/// 授予 tick 本身算激活（同 tick 内第二发命中会被挡），与参考端一致：参考端在 tick N 把
/// immuneTime 设为 D，tick N+1 先递减到 D-1，到 tick N+D 才递减到 0 放行。
/// </para>
/// <para>
/// 0 是「未激活」哨兵。用严格小于而不是小于等于，是因为 tick 从 0 起算：
/// 小于等于会让「从未授予」的 0 号哨兵在 tick 0 被判成激活，把该帧所有伤害吞掉。
/// </para>
/// </remarks>
public sealed class ImmunityWindowComponent
{
  private readonly long[] _untilTick = new long[(int)ImmunityChannel.Count];

  /// <summary>True when the last grant asked the presentation layer not to blink.</summary>
  public bool SuppressBlink { get; private set; }

  /// <summary>True when the channel's window is still covering the given tick.</summary>
  public bool IsActive(ImmunityChannel channel, long tick)
  {
    long until = _untilTick[Validate(channel)];
    return until != 0 && tick < until;
  }

  /// <summary>Last protected tick for one channel; 0 means never granted.</summary>
  public long Until(ImmunityChannel channel)
  {
    return _untilTick[Validate(channel)];
  }

  /// <summary>Extends one channel's window. Shorter grants never shrink an existing window.</summary>
  public void Grant(
      ImmunityChannel channel,
      long tick,
      int durationTicks,
      bool suppressBlink = false)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (durationTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(durationTicks));
    }

    if (durationTicks == 0)
    {
      return;
    }

    int index = Validate(channel);
    long until = checked(tick + durationTicks);
    if (until > _untilTick[index])
    {
      _untilTick[index] = until;
    }

    if (suppressBlink)
    {
      SuppressBlink = true;
    }
  }

  /// <summary>Clears every expired slot so stored state never carries a stale tick.</summary>
  /// <returns>True when at least one slot was cleared.</returns>
  public bool Expire(long tick)
  {
    bool cleared = false;
    for (int index = 0; index < _untilTick.Length; index++)
    {
      long until = _untilTick[index];
      if (until != 0 && tick >= until)
      {
        _untilTick[index] = 0;
        cleared = true;
      }
    }

    if (cleared && !AnyActive(tick))
    {
      SuppressBlink = false;
    }

    return cleared;
  }

  /// <summary>Drops every channel window immediately.</summary>
  public void Clear()
  {
    Array.Clear(_untilTick);
    SuppressBlink = false;
  }

  private bool AnyActive(long tick)
  {
    for (int index = 0; index < _untilTick.Length; index++)
    {
      long until = _untilTick[index];
      if (until != 0 && tick < until)
      {
        return true;
      }
    }

    return false;
  }

  private static int Validate(ImmunityChannel channel)
  {
    int index = (int)channel;
    if (index < 0 || index >= (int)ImmunityChannel.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(channel));
    }

    return index;
  }
}
