// Adapted from FluidHTN e67af264cfdf240053f392d4e0e6c620c454eb97.
// Copyright (c) 2019 Pål Trefall. MIT license: LICENSE.txt.
#nullable disable

using Terraria.Ai.Htn.Factory;

namespace Terraria.Ai.Htn
{
    /// <summary>
    ///     A simple domain builder for easy use when one just need the core functionality
    ///     of the BaseDomainBuilder. This class is sealed, so if you want to extend the
    ///     functionality of the domain builder, extend BaseDomainBuilder instead.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DomainBuilder<T> : BaseDomainBuilder<DomainBuilder<T>, T>
        where T : IContext
    {
        // ========================================================= CONSTRUCTION

        public DomainBuilder(string domainName) : base(domainName, new DefaultFactory())
        {
        }

        public DomainBuilder(string domainName, IFactory factory) : base(domainName, factory)
        {
        }
    }
}