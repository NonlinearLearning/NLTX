// Based on CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
namespace Terraria.Ai.Goap.Core;

public interface IKeyResolver {
  string GetKey(ICondition condition);
  string GetKey(IEffect effect);
  bool AreConflicting(IEffect effect, ICondition condition);
}
