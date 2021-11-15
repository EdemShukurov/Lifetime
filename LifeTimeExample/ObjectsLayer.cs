using System;
using LifetimeUtility;

namespace LifetimeExample
{
    public class ObjectsLayer : IDisposable
    {
        private readonly LifetimeDefinition _lifetimeDefinition;

        /// <summary>
        /// Objects layer depends on the outer lifetime
        /// </summary>
        /// <param name="outerLifetime"></param>
        public ObjectsLayer(OuterLifetime outerLifetime)
        {
            _lifetimeDefinition = Lifetime.DefineDependant(outerLifetime);
        }

        public void Dispose()
        {
            _lifetimeDefinition?.Dispose();
        }
    }
}