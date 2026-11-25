// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

using Terraria.Share.Ai.Htn.Conditions;
using Terraria.Share.Ai.Htn.PrimitiveTasks;
using System;
using System.Collections.Generic;

namespace Terraria.Share.Ai.Htn
{
    public class DefaultPlannerState : IPlannerState
    {
        // ========================================================= PROPERTIES

        public ITask CurrentTask { get; set; }
        public Queue<ITask> Plan { get; set; } = new Queue<ITask>();
        public TaskStatus LastStatus { get; set; }

        // ========================================================= CALLBACKS

        public Action<Queue<ITask>> OnNewPlan { get; set; }
        public Action<Queue<ITask>, ITask, Queue<ITask>> OnReplacePlan { get; set; }
        public Action<ITask> OnNewTask { get; set; }
        public Action<ITask, ICondition> OnNewTaskConditionFailed { get; set; }
        public Action<IPrimitiveTask> OnStopCurrentTask { get; set; }
        public Action<IPrimitiveTask> OnCurrentTaskCompletedSuccessfully { get; set; }
        public Action<IEffect> OnApplyEffect { get; set; }
        public Action<IPrimitiveTask> OnCurrentTaskFailed { get; set; }
        public Action<IPrimitiveTask> OnCurrentTaskStarted { get; set; }
        public Action<IPrimitiveTask> OnCurrentTaskContinues { get; set; }
        public Action<IPrimitiveTask, ICondition> OnCurrentTaskExecutingConditionFailed { get; set; }
    }
}
