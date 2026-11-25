// Adapted from CrashKonijn/GOAP 7fb29cfe; see LICENSE.txt, NOTICE.txt and SOURCES.md.
namespace Terraria.Share.Ai.Goap.Core;

public interface IGoapAction : IAction, IConnectable {
  float GetCost(IAgent agent, ITarget? target);
  bool IsEnabled(IAgent agent);
}
