// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
// Changes: explicit per-agent ports, no Unity/proxy, context set before run-state update.
#nullable disable
using System;
using Terraria.Ai.Goap.Core;


namespace Terraria.Ai.Goap.Runtime
{
    public class ActionRunner
    {
        private readonly IAgent agent;
        private readonly ActionContext context = new();

        public ActionRunner(IAgent agent)
        {
            this.agent = agent;
        }

        public void Run(float deltaTime)
        {
            if (!this.RunIsValid())
                return;

            this.context.DeltaTime = deltaTime;
            this.context.IsInRange = this.agent.IsInRange();

            switch (this.agent.ActionState.Action.GetMoveMode(this.agent))
            {
                case ActionMoveMode.MoveBeforePerforming:
                    this.RunMoveBeforePerforming(deltaTime);
                    break;
                case ActionMoveMode.PerformWhileMoving:
                    this.RunPerformWhileMoving(deltaTime);
                    break;
            }
        }

        private bool RunIsValid()
        {
            if (this.agent.ActionState?.Action == null)
            {
                this.agent.Environment.LogWarning("No action to run!");
                return false;
            }

            var isValid = this.agent.ActionState.Action.IsValid(this.agent, this.agent.ActionState.Data);

            if (!isValid)
            {
                this.agent.Environment.LogWarning($"Action {this.agent.ActionState.Action.GetType().Name} is not valid!");
                this.agent.StopAction();
                return false;
            }

            return true;
        }

        private void RunPerformWhileMoving(float deltaTime)
        {
            if (this.agent.IsInRange())
            {
                this.agent.SetState(AgentState.PerformingAction);
                this.agent.SetMoveState(AgentMoveState.InRange);
                this.PerformAction(deltaTime);
                return;
            }

            this.agent.SetState(AgentState.MovingWhilePerformingAction);
            this.agent.SetMoveState(AgentMoveState.NotInRange);
            this.Move();
            this.PerformAction(deltaTime);
        }

        private void RunMoveBeforePerforming(float deltaTime)
        {
            if (this.agent.IsInRange() || this.agent.State == AgentState.PerformingAction)
            {
                this.agent.SetState(AgentState.PerformingAction);
                this.agent.SetMoveState(AgentMoveState.InRange);
                this.PerformAction(deltaTime);
                return;
            }

            this.agent.SetState(AgentState.MovingToTarget);
            this.agent.SetMoveState(AgentMoveState.NotInRange);
            this.Move();
        }

        private void Move()
        {
            if (this.agent.CurrentTarget == null)
                return;

            this.agent.Environment.Move(this.agent.CurrentTarget);
        }

        private void PerformAction(float deltaTime)
        {
            if (!this.ShouldContinue())
                return;

            this.context.DeltaTime = deltaTime;
            this.context.IsInRange = this.agent.IsInRange();

            if (!this.agent.ActionState.HasPerformed)
                this.agent.ActionState.Action.BeforePerform(this.agent, this.agent.ActionState.Data);

            var state = this.agent.ActionState.Action.Perform(this.agent, this.agent.ActionState.Data, this.context);

            if (state.IsCompleted(this.agent))
            {
                this.agent.CompleteAction();
                return;
            }

            if (state.ShouldStop(this.agent))
            {
                this.agent.StopAction();
                return;
            }

            this.agent.ActionState.RunState = state;
        }

        private bool ShouldContinue()
        {
            if (this.agent.ActionState.RunState == null)
                return true;

            this.agent.ActionState.RunState.Update(this.agent, this.context);

            if (this.agent.ActionState.RunState.IsCompleted(this.agent))
            {
                this.agent.CompleteAction();
                return false;
            }

            if (this.agent.ActionState.RunState.ShouldStop(this.agent))
            {
                this.agent.StopAction();
                return false;
            }

            return this.agent.ActionState.RunState.ShouldPerform(this.agent);
        }
    }

}
