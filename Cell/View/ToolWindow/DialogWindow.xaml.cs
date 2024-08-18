﻿using Cell.ViewModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class DialogWindow : UserControl, IToolWindow
    {
        private readonly string _title;
        public DialogWindow(string title, string message, List<CommandViewModel> actions)
        {
            DataContext = this;
            InitializeComponent();
            _messageLabel.Text = message;
            _title = title;
            foreach (var action in actions)
            {
                DialogOptions.Add(action);
            }
        }

        public ObservableCollection<CommandViewModel> DialogOptions { get; set; } = [];

        public Action? RequestClose { get; set; }

        public static void ShowDialog(string title, string message)
        {
            var actions = new List<CommandViewModel>
            {
                new("Ok", new RelayCommand(x => true, x => { }))
            };
            var dialogWindow = new DialogWindow(title, message, actions);
            ApplicationViewModel.Instance.MainWindow.ShowToolWindow(dialogWindow);
        }

        public static void ShowYesNoConfirmationDialog(string title, string message, Action action)
        {
            var actions = new List<CommandViewModel>
            {
                new("Yes", new RelayCommand(x => true, x => action())),
                new("No", new RelayCommand(x => true, x => { }))
            };
            var dialogWindow = new DialogWindow(title, message, actions);
            ApplicationViewModel.Instance.MainWindow.ShowToolWindow(dialogWindow);
        }

        public string GetTitle() => _title;

        public List<CommandViewModel> GetToolBarCommands() => [];

        public void HandleBeingClosed()
        {
        }

        private void ButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            RequestClose?.Invoke();
        }
    }
}
