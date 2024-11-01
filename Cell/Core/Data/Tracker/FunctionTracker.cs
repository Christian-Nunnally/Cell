using Cell.Core.Execution.Functions;
using Cell.Model;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Cell.Core.Data.Tracker
{
    /// <summary>
    /// Tracks and sorts functions into namespaces, allowing for easy access to them. Also provides events for when functions are added or removed. Finally, it provides a way to create new functions.
    /// </summary>
    public class FunctionTracker
    {
        /// <summary>
        /// An observable collection of all the loaded cell functions.
        /// </summary>
        public ObservableCollection<CellFunction> CellFunctions { get; private set; } = [];

        private Dictionary<string, Dictionary<string, CellFunction>> Namespaces { get; set; } = [];

        /// <summary>
        /// Occurs when a function is added to this tracker and is therefore being tracked.
        /// </summary>
        public event Action<CellFunction>? FunctionAdded;

        /// <summary>
        /// Occurs when a function is removed from this tracker and is therefore no longer being tracked.
        /// </summary>
        public event Action<CellFunction>? FunctionRemoved;

        /// <summary>
        /// Starts tracking a function.
        /// </summary>
        /// <param name="space">The namespace of the function.</param>
        /// <param name="function">The function.</param>
        public void AddCellFunctionToNamespace(string space, CellFunction function)
        {
            if (Namespaces.TryGetValue(space, out var namespaceFunctions)) namespaceFunctions.Add(function.Model.Name, function);
            else Namespaces.Add(space, new Dictionary<string, CellFunction> { { function.Model.Name, function } });
            CellFunctions.Add(function);
            FunctionAdded?.Invoke(function);
        }

        /// <summary>
        /// Creates a cell function in the given namespace with the given name.
        /// </summary>
        /// <param name="space">The namespace to create the function in.</param>
        /// <param name="name">The name to give the function.</param>
        /// <param name="code">(optional) The initial code the function should have.</param>
        /// <returns>The newly created function.</returns>
        /// <exception cref="InvalidOperationException">If the name or space are not valid for a function.</exception>
        public CellFunction CreateCellFunction(string space, string name, string code = "")
        {
            if (space.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new InvalidOperationException("Invalid space name for function, can not contain characters that are invalid in a file name.");
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) throw new InvalidOperationException("Invalid space name for function, can not contain characters that are invalid in a file name.");
            var model = new CellFunctionModel(name, code, space);
            var function = new CellFunction(model);
            AddCellFunctionToNamespace(space, function);
            return function;
        }

        /// <summary>
        /// Deletes a cell function from the project, which means it will be deleted from disk.
        /// </summary>
        /// <param name="function">The function to delete.</param>
        public void DeleteCellFunction(CellFunction function)
        {
            if (Namespaces.TryGetValue(function.Model.ReturnType, out var namespaceFunctions))
            {
                namespaceFunctions.Remove(function.Model.Name);
                CellFunctions.Remove(function);
                FunctionRemoved?.Invoke(function);
            }
        }

        /// <summary>
        /// Gets a cell function in the given namespace with the given name if it exists, otherwise creates it.
        /// </summary>
        /// <param name="space">The namespace for the function.</param>
        /// <param name="name">The name of the function.</param>
        /// <returns>The existing or newly created function.</returns>
        public CellFunction GetOrCreateFunction(string space, string name)
        {
            if (TryGetCellFunction(space, name, out var function)) return function;
            return CreateCellFunction(space, name);
        }

        /// <summary>
        /// Attempts to get a cell function from the given namespace and name.
        /// </summary>
        /// <param name="space">The namespace to get the function from.</param>
        /// <param name="name">The name of the function to get.</param>
        /// <param name="function">The function, if it exists.</param>
        /// <returns>True if the function exists.</returns>
        public bool TryGetCellFunction(string space, string name, [MaybeNullWhen(false)] out CellFunction function)
        {
            function = null;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(space)) return false;
            if (Namespaces.TryGetValue(space, out var namespaceFunctions))
            {
                if (namespaceFunctions.TryGetValue(name, out var value))
                {
                    function = value;
                    return true;
                }
            }
            return false;
        }
    }
}
