// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

namespace Terraria.Share.Ai.Htn.Operators
{
    public interface IOperator
    {
        TaskStatus Start(IContext ctx);
        TaskStatus Update(IContext ctx);

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
