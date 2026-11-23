// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Runtime
{
    public class ActionState : IActionState
    {
        public bool HasPerformed => this.RunState != null;
        public IAction Action { get; private set; }
        public IAction PreviousAction { get; private set; }
        public IActionRunState RunState { get; set; }
        public IActionData Data { get; private set; }

        public void SetAction(IAction action, IActionData data)
        {
            this.Action = action;
            this.RunState = null;
            this.Data = data;
        }

        public void Reset()
        {
            this.PreviousAction = this.Action;
            this.Action = null;
            this.RunState = null;
            this.Data = null;
        }
    }
}
