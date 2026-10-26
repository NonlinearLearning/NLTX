using System;
using System.Collections.Generic;

namespace Terraria.Movement;

/// <summary>Chooses one request without applying it to an entity.</summary>
public static class MovementIntentResolver
{
 public static bool TrySelect(
   IEnumerable<MovementIntent> intents,
   long tick,
   out MovementIntent selected)
 {
  ArgumentNullException.ThrowIfNull(intents);
  selected = default;
  bool found = false;
  foreach (MovementIntent intent in intents)
  {
   if (intent.Tick != tick)
   {
    continue;
   }

   if (!found || intent.Priority > selected.Priority)
   {
    selected = intent;
    found = true;
   }
  }

  return found;
 }
}
