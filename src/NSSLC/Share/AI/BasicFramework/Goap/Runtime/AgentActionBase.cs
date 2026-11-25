// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: .NET namespaces/platform types; see SOURCES.md for file-specific changes.
#nullable disable
using Terraria.Share.Ai.Goap.Core;

namespace Terraria.Share.Ai.Goap.Runtime
{
    public abstract class AgentActionBase<TActionData, TActionProperties>
        where TActionData : IActionData, new()
        where TActionProperties : class, IActionProperties, new()
    {
        /// <summary>
        ///     Gets the action data.
        /// </summary>
        /// <returns>The action data.</returns>
        public IActionData GetData()
        {
            return this.CreateData();
        }

        /// <summary>
        ///     Creates a new instance of action data.
        /// </summary>
        /// <returns>The created action data.</returns>
        public virtual TActionData CreateData()
        {
            return new TActionData();
        }

        /// <summary>
        ///     Gets the action properties.
        /// </summary>
        public abstract TActionProperties Properties { get; }

        /// <summary>
        ///     Called when the action is created.
        /// </summary>
        public virtual void Created() { }

        /// <summary>
        ///     Called when the action is started (when it is assigned to an agent).
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="data">The action data.</param>
        public void Start(IAgent agent, IActionData data) => this.Start(agent, (TActionData) data);

        /// <summary>
        ///     Called when the action is started (when it is assigned to an agent).
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="data">The action data.</param>
        public virtual void Start(IAgent agent, TActionData data) { }

        /// <summary>
        ///     Called once before performing the action. Don't override this method, override the other BeforePerform method
        ///     instead.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="data">The action data.</param>
        public void BeforePerform(IAgent agent, IActionData data) => this.BeforePerform(agent, (TActionData) data);

        /// <summary>
        ///     Called once before performing the action. Override this method to implement custom logic.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="data">The action data.</param>
        public virtual void BeforePerform(IAgent agent, TActionData data) { }

        /// <summary>
        ///     Performs the action. Don't override this method, override the other Perform method instead.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="data">The action data.</param>
        /// <param name="context">The action context.</param>
        /// <returns>The action run state.</returns>
        public IActionRunState Perform(IAgent agent, IActionData data, IActionContext context) => this.Perform(agent, (TActionData) data, context);

        /// <summary>
        ///     Performs the action with the specified action data. Use this method to implement custom logic.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="data">The action data.</param>
        /// <param name="context">The action context.</param>
        /// <returns>The action run state.</returns>
        public abstract IActionRunState Perform(IAgent agent, TActionData data, IActionContext context);

        /// <summary>
        ///     Called when the action ends. This is called after the action is completed or stopped.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="data">The action data.</param>
        public virtual void End(IAgent agent, TActionData data) { }

        /// <summary>
        ///     Called when the action is stopped. Don't override this method, override the other Stop method instead.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="data">The action data.</param>
        public void Stop(IAgent agent, IActionData data)
        {
            try
            {
                this.Stop(agent, (TActionData) data);
            }
            finally
            {
                this.End(agent, (TActionData) data);
            }
        }

        /// <summary>
        ///     Called when the action is stopped. Use this method to implement custom logic.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="data">The action data.</param>
        public virtual void Stop(IAgent agent, TActionData data) { }

        /// <summary>
        ///     Called when the action is completed. Don't override this method, override the other Complete method instead.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="data">The action data.</param>
        public void Complete(IAgent agent, IActionData data)
        {
            try
            {
                this.Complete(agent, (TActionData) data);
            }
            finally
            {
                this.End(agent, (TActionData) data);
            }
        }

        /// <summary>
        ///     Called when the action is completed. Use this method to implement custom logic.
        /// </summary>
        /// <param name="agent">The agent.</param>
        /// <param name="data">The action data.</param>
        public virtual void Complete(IAgent agent, TActionData data) { }
    }
}
