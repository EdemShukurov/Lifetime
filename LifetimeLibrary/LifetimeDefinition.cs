namespace LifetimeUtility
{
    /// <summary>
    /// Controller for <see cref="Lifetime"/> like <see cref="CancellationTokenSource"/> is a controller fot <see cref="CancellationToken"/>.
    /// You can terminate this definition by <see cref="Terminate"/> method (or <see cref="Dispose"/> which is the same). 
    /// </summary>
    /// <remarks>
    /// When this group is terminated, dependent groups (lifetimes) is terminated too.
    /// It is used for termination. 
    /// </remarks>
    public class LifetimeDefinition : IDisposable
    {
        //internal static readonly LifetimeDefinition Terminated = new LifetimeDefinition(new Lifetime();
        internal static readonly LifetimeDefinition Eternal = new LifetimeDefinition();
        
        /// <summary>
        /// Underlying lifetime for this definition.
        /// <remarks> There are no implicit cast from <see cref="LifetimeDefinition"/> to <see cref="Lifetime"/> intentionally.
        /// When method receives <see cref="LifetimeDefinition"/> as a parameter it means (in a philosophic sense) that this method either responsible for definition's termination
        /// or it must pass definition to some other method.
        /// You can easily make a mistake and forget to terminate definition when you implicitly convert into lifetime and pass to some other method.
        /// </remarks>
        /// </summary>
        public Lifetime Lifetime => new Lifetime(this);
        
        public bool IsTerminated { get; private set; }
        
        /// <summary>
        /// Means that this definition corresponds to <see cref="Lifetime.Eternal"/> and can't be terminated. 
        /// </summary>
        public bool IsEternal => ReferenceEquals(this, Eternal);

        private readonly List<Action> _endLifeActions = new List<Action>();

        /// <summary>
        /// Creates toplevel lifetime definition with no parent. LifeTime will always be alive.
        /// </summary>
        private LifetimeDefinition()
        {
        }

        /// <summary>
        /// Created definition nested into <paramref name="parent"/>, i.e. this definition is attached to parent as termination resource.  
        ///
        /// <para>
        /// <see cref="parent"/>'s termination (via <see cref="Terminate"/> method) will instantly propagate <c>Canceling</c> signal
        /// to all descendants, i.e all statuses of parent's children, children's children, ... will become <see cref="LifetimeStatus.Canceling"/>
        /// instantly. And then resources destructure will begin from the most recently connected children to the last (stack's like LIFO way).
        /// </para>
        /// </summary>
        ///
        /// <param name="parent"></param>
        public LifetimeDefinition(OuterLifetime parent) : this()
        {
            parent.LifetimeDefinition.Attach(this);
        }

        internal void Attach(LifetimeDefinition child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));

            //can't attach eternal lifetime
            if (child.IsEternal || child.IsTerminated)
            {
                return;
            }

            Add(child.Dispose);
        }


        internal void Add(Action action)
        {
            //will never be terminated; need to be revised for debugging
            if (IsEternal)
            {
                return;
            }
            
            _endLifeActions.Add(action);
        }

        public void Terminate()
        {
            if (IsEternal || IsTerminated)
            {
                return;
            }

            for (int i = _endLifeActions.Count - 1 ; i >= 0; i--)
            {
                _endLifeActions[i]();
            }
            
            _endLifeActions.Clear();
            IsTerminated = true;
        }
        
        // /// <summary>
        // /// Creates new instance of Lifetime which terminates only when last
        // /// dependent lifetime is terminated
        // /// </summary>
        // public static Lifetime WhenAll(params OuterLifetime[] lifetimes)
        // {
        //     var definition = new LifetimeDefinition();
        //     Action subscription = null;
        //
        //     var act = new Action(() =>
        //     {
        //         if (!definition.Lifetime.IsTerminated && lifetimes.All(x => x.IsTerminated))
        //         {
        //             definition.Terminate(); 
        //
        //             for (int i = 0; i < lifetimes.Length; i++)
        //             {
        //                 lifetimes[i].LifetimeDefinition._endLifeActions.Remove(subscription);
        //             }
        //         }
        //     });
        //
        //     subscription = act;
        //     
        //     for (int i = 0; i < lifetimes.Length; i++)
        //     {
        //         lifetimes[i].LifetimeDefinition._endLifeActions.Insert(0, subscription);
        //     }
        //
        //     return definition.Lifetime;
        // }

        public void Dispose()
        {
            Terminate();
        }
    }
}