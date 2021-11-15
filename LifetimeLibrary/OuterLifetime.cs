namespace LifetimeUtility
{
    /// <summary>
    ///   <para>A subset of the <see cref="Lifetime" /> interface with which you cannot “bind” actions to a lifetime.</para>
    ///   <para>It's “outer” in the sense that it's not your lifetime, but of some parent object potentially more long-lived than yours. You cannot schedule anything to its termination because it will happen way after your object goes off scope.</para>
    ///   <para>The only thing you can know is that it's an “outer”, it's limiting the life of your object, and if it's terminated — so are you. Checking for <see cref="IsTerminated" /> is one of the allowed option.</para>
    ///   <para>Another option is to define a nested lifetime, in which case you MUST ensure it's terminated explicitly, without relying on the outer lifetime. The outer lifetime is only a safety catch to make sure it does not live too long.</para>
    /// </summary>
    public readonly struct OuterLifetime
    {
        private readonly Lifetime _lifetime;

        public bool IsTerminated => _lifetime.IsTerminated;

        internal LifetimeDefinition LifetimeDefinition => _lifetime.Definition;
        
        private OuterLifetime(Lifetime lifetime)
        {
            _lifetime = lifetime;
        }
        
        public static implicit operator OuterLifetime(Lifetime lifetime)
        {
            return new OuterLifetime(lifetime);
        }

        public static implicit operator Lifetime(OuterLifetime lifetime)
        {
            return lifetime._lifetime;
        }
        
        public static implicit operator OuterLifetime(LifetimeDefinition lifetime)
        {
            return new OuterLifetime(lifetime.Lifetime);
        }
    }
}