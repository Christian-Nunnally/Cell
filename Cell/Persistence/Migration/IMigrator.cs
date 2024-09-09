
namespace Cell.Persistence.Migration
{
    public interface IMigrator
    {
        void Migrate(PersistenceManager persistenceManager);
    }
}
