using System;
using System.Collections.Generic;

namespace Terraria.Share.Ai;

/// <summary>Calculation-local output. Emitting outside Calculate/Stop/Dispose is invalid.</summary>
public sealed class AiOutput<TIntent> where TIntent : unmanaged {
  private readonly List<TIntent> _intents = [];
  private readonly List<string> _warnings = [];
  private bool _active;

  internal AiOutput() {
  }

  public void Emit(TIntent intent) {
    EnsureActive();
    _intents.Add(intent);
  }

  public void Warn(string message) {
    EnsureActive();
    ArgumentNullException.ThrowIfNull(message);
    _warnings.Add(message);
  }

  internal void Begin() {
    _intents.Clear();
    _warnings.Clear();
    _active = true;
  }

  internal void End() {
    _active = false;
  }

  internal AiResult<TIntent> ToResult(AiStatus status, Type[]? missing = null) {
    return new AiResult<TIntent>(status, _intents.ToArray(), _warnings.ToArray(), missing ?? []);
  }

  private void EnsureActive() {
    if (!_active) {
      throw new InvalidOperationException("AI output is only valid during a binding operation.");
    }
  }
}
