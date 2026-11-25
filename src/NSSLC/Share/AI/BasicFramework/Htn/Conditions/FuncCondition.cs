// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

using System;

namespace Terraria.Share.Ai.Htn.Conditions
{
    public class FuncCondition<T> : ICondition where T : IContext
    {
        // ========================================================= FIELDS

        private readonly Func<T, bool> _func;

        // ========================================================= CONSTRUCTION

        public FuncCondition(string name, Func<T, bool> func)
        {
            Name = name;
            _func = func;
        }

        // ========================================================= PROPERTIES

        public string Name { get; }

        // ========================================================= VALIDITY

        public bool IsValid(IContext ctx)
        {
            if (ctx is T c)
            {
                var result = _func?.Invoke(c) ?? false;

                if (ctx.LogDecomposition)
                {
                    ctx.Log(Name, $"FuncCondition.IsValid:{result}", ctx.CurrentDecompositionDepth+1, this, result ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed);
                }

                return result;
            }

            throw new Exception("Unexpected context type!");
        }
    }
}
