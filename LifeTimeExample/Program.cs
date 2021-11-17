using LifetimeUtility;

namespace LifetimeExample // Note: actual namespace depends on the project name.
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DataSource dataSource;
            using (var appLifeTime = LifetimeDefinition.Eternal)
            {
                dataSource = new DataSource(appLifeTime.Lifetime);
                dataSource.X.OnPropertyChanged += ShowValue;
                dataSource.Y.OnPropertyChanged += ShowValue;

                dataSource.X.Value = 44;
                dataSource.X.Value = 33;
            }
            
            dataSource.X.Value = 14;
            dataSource.X.Value = 16;
            
            Console.ReadKey();
        }

        private static void ShowValue(float value)
        {
            Console.WriteLine(value);
        }
    }
}