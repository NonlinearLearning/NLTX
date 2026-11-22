using System;
using System.Collections.Generic;

namespace Terraria.Ai;

/// <summary>Metadata for a behavior declaration. Runtime operations belong to AiBinding.</summary>
public interface IAiBehavior {
  Type IntentType { get; }
  IReadOnlyList<Type> RequiredComponents { get; }
}
