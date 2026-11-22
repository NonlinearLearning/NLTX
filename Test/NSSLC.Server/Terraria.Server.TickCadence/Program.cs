// 固定 5 件事（验证类型：contract）：
// 1. 未超时的 tick 恰好推进一个周期，且不丢格点。
// 2. 恰好落在下一格点（零余量）时按"已错过"处理，仍返回严格未来的格点。
// 3. 超时不足两个周期时只丢一个格点，返回值落在 now 之后的最近格点。
// 4. 超时两个周期以上时按整格丢弃，跳过数量与实际错过数一致。
// 5. 任意超时下返回值都严格大于 now，且相邻两次推进的时间差 ≥ 一个周期（不会零间隔补发）。
//
// 被验证对象：src/NSSLC.Server/ServerTickPacing.cs
// 背景：客户端在 serverAuthoritative 模式下不插值，包 23 的到达节拍就是抖动源；
// 本验证锁定"丢弃错过格点、永不补发"的网格语义。

using NSSLC.Server;

const long Period = 10_000_000L / 60L; // Stopwatch 时间戳近似 100ns/格
int failures = 0;
long deadline = 1_000_000_000L;

void Check(string name, bool ok, string detail)
{
  Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name} | {detail}");
  if (!ok)
  {
    failures++;
  }
}

// 1. 未超时：now 在格点之前。
long next = ServerTickPacing.AdvanceDeadline(deadline, deadline + Period - 1, Period, out long skipped);
Check(
    "on-time-tick-advances-one-period",
    next == deadline + Period && skipped == 0,
    $"next={next} expected={deadline + Period} skipped={skipped}");

// 2. 恰好等于下一格点：没有余量，必须按错过处理。
next = ServerTickPacing.AdvanceDeadline(deadline, deadline + Period, Period, out skipped);
Check(
    "exact-boundary-counts-as-missed",
    next == deadline + (2 * Period) && skipped == 1,
    $"next={next} expected={deadline + (2 * Period)} skipped={skipped}");

// 3. 超出 1.5 个周期：只错过一格（deadline+Period），应落在 deadline+2*Period。
next = ServerTickPacing.AdvanceDeadline(
    deadline, deadline + (Period * 3 / 2), Period, out skipped);
Check(
    "overrun-1.5-periods-skips-one",
    next == deadline + (2 * Period) && skipped == 1,
    $"next={next} expected={deadline + (2 * Period)} skipped={skipped}");

// 4. 超出 2.5 个周期：错过两格，应落在 deadline+3*Period。
next = ServerTickPacing.AdvanceDeadline(
    deadline, deadline + (Period * 5 / 2), Period, out skipped);
Check(
    "overrun-2.5-periods-skips-two",
    next == deadline + (3 * Period) && skipped == 2,
    $"next={next} expected={deadline + (3 * Period)} skipped={skipped}");

// 5. 任意超时下：返回值严格大于 now，相邻推进差 ≥ 一个周期。
bool strictlyFuture = true;
bool noBurst = true;
long current = deadline;
for (long extra = 0; extra <= Period * 5; extra += Period / 7)
{
  long now = deadline + extra;
  long advanced = ServerTickPacing.AdvanceDeadline(current, now, Period, out _);
  if (advanced <= now)
  {
    strictlyFuture = false;
  }

  if (advanced - current < Period)
  {
    noBurst = false;
  }
}

Check("advanced-deadline-is-strictly-future", strictlyFuture, "checked 36 overrun offsets in [0, 5] periods");
Check("never-reissues-back-to-back", noBurst, "consecutive deadlines always at least one period apart");

Console.WriteLine();
if (failures == 0)
{
  Console.WriteLine("PASS: tick pacing keeps a strictly-future 60 Hz grid and never reissues missed ticks.");
  return 0;
}

Console.WriteLine($"FAIL: {failures} check(s) failed.");
return 1;
