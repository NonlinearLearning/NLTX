// Adapted from CrashKonijn/GOAP 7fb29cfe; see LICENSE.txt, NOTICE.txt and SOURCES.md.
namespace Terraria.Share.Ai.Goap.Core;

public interface IActionState {
  bool HasPerformed { get; }
  IAction? Action { get; }
  IAction? PreviousAction { get; }
  IActionRunState? RunState { get; set; }
  IActionData? Data { get; }
  void SetAction(IAction action, IActionData data);
  void Reset();
}
