
namespace Cell.Persistence.Migration
{
    internal interface IMigrator
    {
        void Migrate();

        public string FromVersion { get; }

        public string ToVersion { get; }
    }
}
