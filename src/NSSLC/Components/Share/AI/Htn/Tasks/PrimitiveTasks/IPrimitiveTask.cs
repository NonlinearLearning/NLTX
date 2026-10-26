// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

using System.Collections.Generic;
using Terraria.Ai.Htn.Conditions;
using Terraria.Ai.Htn.Operators;

namespace Terraria.Ai.Htn.PrimitiveTasks
{
    public interface IPrimitiveTask : ITask
    {
        /// <summary>
        ///     Executing conditions are validated before every call to Operator.Update(...)
        /// </summary>
        List<ICondition> ExecutingConditions { get; }

        /// <summary>
        ///     Add a new executing condition to the primitive task. This will be checked before
        ///		every call to Operator.Update(...)
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        ITask AddExecutingCondition(ICondition condition);

        IOperator Operator { get; }
        void SetOperator(IOperator action);

        List<IEffect> Effects { get; }
        ITask AddEffect(IEffect effect);
        void ApplyEffects(IContext ctx);

        /// <summary>
        /// Graceful end of task execution.
        /// </summary>
        /// <param name="ctx"></param>
        void Stop(IContext ctx);

        /// <summary>
        /// Forced termination of task execution.
        /// </summary>
        /// <param name="ctx"></param>
        void Abort(IContext ctx);
    }
}
