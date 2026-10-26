using System;
using System.Collections.Generic;

namespace Terraria.Ai;

/// <summary>
/// An owned, immutable result. Consume intents in order, including cleanup intents.
/// </summary>
public sealed class AiResult<TIntent> where TIntent : unmanaged {
  public AiStatus Status { get; }
  public IReadOnlyList<TIntent> Intents { get; }
  public IReadOnlyList<string> Warnings { get; }
  public IReadOnlyList<Type> MissingComponents { get; }

  internal AiResult(AiStatus status, TIntent[] intents, string[] warnings, Type[] missing) {
    Status = status;
    Intents = Array.AsReadOnly(intents);
    Warnings = Array.AsReadOnly(warnings);
    MissingComponents = Array.AsReadOnly(missing);
  }
}
