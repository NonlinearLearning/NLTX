// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

using System;

namespace Terraria.Ai.Htn.Effects
{
    public class ActionEffect<T> : IEffect where T : IContext
    {
        // ========================================================= FIELDS

        private readonly Action<T, EffectType> _action;

        // ========================================================= CONSTRUCTION

        public ActionEffect(string name, EffectType type, Action<T, EffectType> action)
        {
            Name = name;
            Type = type;
            _action = action;
        }

        // ========================================================= PROPERTIES

        public string Name { get; }
        public EffectType Type { get; }

        // ========================================================= FUNCTIONALITY

        public void Apply(IContext ctx)
        {
            if (ctx is T c)
            {
                if (ctx.LogDecomposition)
                {
                    ctx.Log(Name, $"ActionEffect.Apply:{Type}", ctx.CurrentDecompositionDepth+1, this);
                }

                _action?.Invoke(c, Type);
            }
            else
            {
                throw new Exception("Unexpected context type!");
            }
        }
    }
}
