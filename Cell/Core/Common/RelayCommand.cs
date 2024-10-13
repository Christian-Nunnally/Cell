using System.Windows.Input;

namespace Cell.Core.Common
{
    /// <summary>
    /// A simple command that can be disabled remotely with a CanExecute predicate.
    /// </summary>
    /// <param name="canExecute">Determines whether the command can execute given the parameter.</param>
    /// <param name="execute">Executes the command with the parameter.</param>
    public class RelayCommand(Predicate<object?> canExecute, Action<object?> execute) : ICommand
    {
        private readonly Predicate<object?> _canExecute = canExecute;
        private readonly Action<object?> _execute = execute;

        /// <summary>
        /// Creates a basic command that can always executes.
        /// </summary>
        /// <param name="execute">The action to perform when it executes.</param>
        public RelayCommand(Action<object?> execute) : this(_ => true, execute)
        {
        }

        /// <summary>
        /// Occurs when changes occur that affect whether the command should be allowed to execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Determines whether the command can execute with the given parameter.
        /// </summary>
        /// <param name="parameter">The parameter to test with.</param>
        /// <returns>True if the command can execute.</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute(parameter);
        }

        /// <summary>
        /// Executes the command with the given parameter.
        /// </summary>
        /// <param name="parameter">The parameter to run the command with.</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
