using LifetimeUtility;

namespace LifetimeExample
{
    public class DataSource
    {
        public DataSource(OuterLifetime outerLifetime)
        {
            X = Property<float>.Create(outerLifetime);
            Y = Property<float>.Create(outerLifetime);
        }
        
        public Property<float> X { get; }
        public Property<float> Y { get; }
    }
}