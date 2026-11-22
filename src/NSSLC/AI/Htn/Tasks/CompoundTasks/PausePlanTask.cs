// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

using System;
using System.Collections.Generic;
using Terraria.Ai.Htn.Compounds;
using Terraria.Ai.Htn.Conditions;

namespace Terraria.Ai.Htn
{
    public class PausePlanTask : ITask
    {
        // ========================================================= PROPERTIES

        public string Name { get; set; }
        public ICompoundTask Parent { get; set; }
        public List<ICondition> Conditions { get; } = null;
        public List<IEffect> Effects { get; } = null;

        // ========================================================= VALIDITY

        public DecompositionStatus OnIsValidFailed(IContext ctx)
        {
            return DecompositionStatus.Failed;
        }

        // ========================================================= ADDERS

        public ITask AddCondition(ICondition condition)
        {
            throw new Exception("Pause Plan tasks does not support conditions.");
        }

        public ITask AddEffect(IEffect effect)
        {
            throw new Exception("Pause Plan tasks does not support effects.");
        }

        // ========================================================= FUNCTIONALITY

        public void ApplyEffects(IContext ctx)
        {
        }

        // ========================================================= VALIDITY

        public bool IsValid(IContext ctx)
        {
            if (ctx.LogDecomposition)
            {
                Log(ctx, $"PausePlanTask.IsValid:Success!");
            }

            return true;
        }

        // ========================================================= LOGGING

        protected virtual void Log(IContext ctx, string description)
        {
            ctx.Log(Name, description, ctx.CurrentDecompositionDepth, this, ConsoleColor.Green);
        }
    }
}
