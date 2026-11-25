// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

namespace Terraria.Share.Ai.Htn
{
    public interface IEffect
    {
        string Name { get; }
        EffectType Type { get; }
        void Apply(IContext ctx);
    }
}
