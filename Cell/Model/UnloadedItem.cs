using Cell.Core.Common;
using Cell.Core.Persistence;
using System.Text.Json;

namespace Cell.Model
{
    internal class UnloadedItem
    {
        private readonly PersistedDirectory _directory;
        private readonly string _id;

        public UnloadedItem(PersistedDirectory directory, string id)
        {
            _directory = directory;
            _id = id;
        }

        public UserItem Load()
        {
            var text = _directory.LoadFile(_id) ?? throw new CellError($"Failed to load {_directory.GetFullPath(_id)} because it is not a valid {nameof(UserItem)}");
            return JsonSerializer.Deserialize<UserItem>(text) ?? throw new CellError($"Failed to load {_directory.GetFullPath(_id)} because it is not a valid {nameof(UserItem)}. File contents = {text}");
        }
    }
}
