// Adapted from CrashKonijn/GOAP 7fb29cfe; see LICENSE.txt, NOTICE.txt and SOURCES.md.
namespace Terraria.Share.Ai.Goap.Core;

public interface IAction {
  ActionMoveMode GetMoveMode(IAgent agent);
  float GetStoppingDistance();
  bool IsInRange(IAgent agent, float distance, IActionData data);
  IActionData GetData();
  void Created();
  bool IsValid(IAgent agent, IActionData data);
  void Start(IAgent agent, IActionData data);
  void BeforePerform(IAgent agent, IActionData data);
  IActionRunState Perform(IAgent agent, IActionData data, IActionContext context);
  void Stop(IAgent agent, IActionData data);
  void Complete(IAgent agent, IActionData data);
  bool IsExecutable(IAgent agent, bool conditionsMet);
}
