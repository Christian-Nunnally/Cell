
namespace Cell.Core.Data
{
    /// <summary>
    /// A provider for user collections.
    /// </summary>
    public interface IUserCollectionProvider
    {
        /// <summary>
        /// Gets a collection by name.
        /// </summary>
        /// <param name="name">The name of the collection to get.</param>
        /// <returns>The collection if it exists, or null.</returns>
        UserCollection? GetCollection(string name);
    }
}
