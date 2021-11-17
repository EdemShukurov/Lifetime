using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LifetimeUtility
{
    /// <summary>
    /// Has two main functions:<br/>
    /// 1. High performance analogue of <see cref="CancellationToken"/>. <see cref="LifetimeDefinition"/> is analogue of <see cref="CancellationTokenSource"/> <br/>
    /// 2. Inversion of <see cref="IDisposable"/> pattern (with thread-safety):
    /// user can add termination resources into Lifetime with bunch of <c>Add</c> (e.g. <see cref="Add(Action)"/>) methods.
    /// When lifetime is being terminated (i.e. it's <see cref="LifetimeDefinition"/> was called <see cref="LifetimeDefinition.Terminate"/>) all
    /// previously added termination resources are being terminated in stack-way LIFO order. Lifetimes forms a hierarchy with parent-child relations so in single-threaded world child always
    /// becomes <see cref="IsTerminated"/> <b>BEFORE</b> parent. Usually this hierarchy is a tree but in some cases (like <see cref="Intersect(Lifetime[])"/> it can be
    /// a directed acyclic graph.
    /// </summary>
    ///
    /// <remarks>
    /// See https://github.com/JetBrains/rd/tree/master/rd-net
    /// </remarks>
    public readonly struct Lifetime : IEquatable<Lifetime>
    {
        public bool IsTerminated => Definition.IsTerminated;

        private readonly LifetimeDefinition _myDefinition;
        
        internal LifetimeDefinition Definition => _myDefinition ?? LifetimeDefinition.Eternal;

        /// <summary>
        /// The whole lifetime, it lives forever.
        /// </summary>
        public static Lifetime Eternal = LifetimeDefinition.Eternal.Lifetime;

        //ctor
        internal Lifetime(LifetimeDefinition definition)
        {
            _myDefinition = definition;
        }

        /// <summary>
        /// Add termination resource with <c>kind == Action</c> that will be invoked when lifetime termination start
        /// Resources invocation order: LIFO
        /// </summary>
        /// <param name="action">Action to invoke on termination</param>
        public void Add(Action action)
        {
            CheckNotNull(action);
            
            // synchronization version
            // lock (_actions)
            // {
            //     _actions.Add(action);
            // }
            Definition.Add(action);
        }

        public void AddDisposable(IDisposable disposable)
        {
            CheckNotNull(disposable);
            Definition.Add(disposable.Dispose);
        }

        public void AddBracket(Action subscribe, Action unsubscribe)
        {
            CheckNotNull(subscribe, unsubscribe);
            
            subscribe();
            Add(unsubscribe);
        }
        
        /// <summary>
        /// Keeps object from being garbage collected until this lifetime is terminated.
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public void KeepAlive(object @object)
        {
            if (@object == null) throw new ArgumentNullException(nameof(@object));      

            Add(() => GC.KeepAlive(@object));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckNotNull(object opening, object closing)
        {
            CheckNotNull(opening);      
            CheckNotNull(closing);      
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckNotNull(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));      
        }

        #region Equality
    
        public static bool operator ==(Lifetime left, Lifetime right) 
        {
            return ReferenceEquals(left.Definition, right.Definition); 
        }

        public static bool operator !=(Lifetime left, Lifetime right)
        {
            return !(left == right);
        }
    
        public bool Equals(Lifetime other)
        {
            return ReferenceEquals(Definition, other.Definition);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Lifetime other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Definition.GetHashCode();
        }

        #endregion
        
        /// <summary>
        /// Create lifetime definition, that depends on the another lifetime.
        /// </summary>
        /// <param name="parent">Outer lifetime.</param>
        /// <returns></returns>
        public static LifetimeDefinition DefineDependant(OuterLifetime parent)
        {
            var lifetimeDefinition = new LifetimeDefinition(parent);
            return lifetimeDefinition;
        }

        // /// <summary>
        // /// Creates new instance of Lifetime which terminates only when last
        // /// dependent lifetime is terminated
        // /// </summary>
        // public static Lifetime WhenAny(params OuterLifetime[] lifetimes)
        // {
        //     var definition = Define();
        //     var lifetimesCopy = (OuterLifetime[]) lifetimes.Clone();
        //     
        //     Action subscription = null;
        //
        //     var act = new Action(() =>
        //     {
        //         if (!definition.Lifetime.IsTerminated)
        //         {
        //             definition.Terminate();
        //
        //             for (int i = 0; i < lifetimes.Length; i++)
        //             {
        //                 lifetimes[i].Lifetime.Actions.Remove(subscription);
        //             }
        //         }
        //     });
        //
        //     subscription = act;
        //     
        //     for (int i = 0; i < lifetimes.Length; i++)
        //     {
        //         lifetimes[i].Lifetime.Actions.Insert(0, subscription);
        //     }
        //
        //     return definition.Lifetime;
        // }

        // /// <summary>
        // /// Creates new instance of Lifetime which terminates only when last
        // /// dependent lifetime is terminated
        // /// </summary>
        // public static Lifetime WhenAny(params OuterLifetime[] lifetimes)
        // {
        //     var def = Define();
        //     var lifetimesCopy = (OuterLifetime[]) lifetimes.Clone();
        //     
        //     Action subscription = null;
        //
        //     var act = new Action(() =>
        //     {
        //         if (!def.Lifetime.IsTerminated))
        //         {
        //             def.Terminate();
        //
        //             for (int i = 0; i < lifetimes.Length; i++)
        //             {
        //                 lifetimes[i].Lifetime.Actions.Remove(subscription);
        //             }
        //         }
        //     });
        //
        //     subscription = act;
        //     
        //     for (int i = 0; i < lifetimesCopy.Length; i++)
        //     {
        //         lifetimesCopy[i].Lifetime.Actions.Insert(0, subscription);
        //     }
        //
        //     return def.Lifetime;
        // }
    }
}