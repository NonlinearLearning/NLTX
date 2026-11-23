// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using Terraria.Ai.Goap.Core;

namespace Terraria.Ai.Goap.Runtime
{
    public abstract class ActionRunState : IActionRunState
    {
        public abstract void Update(IAgent agent, IActionContext context);
        public abstract bool ShouldStop(IAgent agent);
        public abstract bool ShouldPerform(IAgent agent);
        public abstract bool IsCompleted(IAgent agent);
        public abstract bool MayResolve(IAgent agent);
        public abstract bool IsRunning();

        public static readonly ActionRunState Continue = new ContinueActionRunState();
        public static readonly ActionRunState ContinueOrResolve = new ContinueOrResolveActionRunState();
        public static readonly ActionRunState Stop = new StopActionRunState();
        public static readonly ActionRunState Completed = new CompletedActionRunState();
        public static ActionRunState Wait(float time, bool mayResolve = false) => new WaitActionRunState(time, mayResolve);
        public static ActionRunState WaitThenComplete(float time, bool mayResolve = false) => new WaitThenCompleteActionRunState(time, mayResolve);
        public static ActionRunState WaitThenStop(float time, bool mayResolve = false) => new WaitThenStopActionRunState(time, mayResolve);
    }
}
