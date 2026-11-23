using System;
using System.Collections.Generic;
using Terraria.Ai.Htn.Debug;
using Terraria.Ai.Htn.Factory;

namespace Terraria.Ai.Htn.Contexts;

/// <summary>Named state access over the upstream world-state stack and rollback mechanism.</summary>
public abstract class EnumContext<TState> : BaseContext where TState : struct, Enum {
  private static readonly Dictionary<TState, int> _indices = CreateIndices();

  public override IFactory Factory { get; protected set; } = new DefaultFactory();
  public override IPlannerState PlannerState { get; protected set; } = new DefaultPlannerState();
  public override byte[] WorldState { get; } = new byte[_indices.Count];
  public override List<string> MTRDebug { get; set; } = [];
  public override List<string> LastMTRDebug { get; set; } = [];
  public override bool DebugMTR { get; }
  public override Queue<IBaseDecompositionLogEntry> DecompositionLog { get; set; } = new();
  public override bool LogDecomposition { get; }

  protected EnumContext(bool debugMtr = false, bool logDecomposition = false) {
    DebugMTR = debugMtr;
    LogDecomposition = logDecomposition;
  }

  public byte GetState(TState state) {
    return base.GetState(GetIndex(state));
  }

  public bool HasState(TState state, byte value) {
    return GetState(state) == value;
  }

  public bool IsSet(TState state) {
    return HasState(state, 1);
  }

  public void SetState(TState state, byte value, bool setAsDirty = true,
                       EffectType effect = EffectType.Permanent) {
    base.SetState(GetIndex(state), value, setAsDirty, effect);
  }

  public void SetState(TState state, bool value, bool setAsDirty = true,
                       EffectType effect = EffectType.Permanent) {
    SetState(state, value ? (byte)1 : (byte)0, setAsDirty, effect);
  }

  private static int GetIndex(TState state) {
    if (!_indices.TryGetValue(state, out int index)) {
      throw new ArgumentOutOfRangeException(nameof(state));
    }
    return index;
  }

  private static Dictionary<TState, int> CreateIndices() {
    Dictionary<TState, int> result = new();
    foreach (TState state in Enum.GetValues<TState>()) {
      if (!result.TryAdd(state, result.Count)) {
        throw new InvalidOperationException("State enums must not contain aliases.");
      }
    }
    return result;
  }
}
