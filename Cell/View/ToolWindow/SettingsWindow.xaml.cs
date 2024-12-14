using Cell.View.Application;
using Cell.View.Cells;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace Cell.View.ToolWindow
{
    public partial class SettingsWindow : ResizableToolWindow
    {
        private readonly SettingsWindowViewModel _viewModel;
        /// <summary>
        /// Creates a new instance of <see cref="SettingsWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public SettingsWindow(SettingsWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private async void CreateBackupButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ApplicationViewModel.Instance.BackupManager is null)
            {
                ApplicationViewModel.Instance.DialogFactory?.Show("Unable to create backup", "The backup manager has not been initialized so backups can not be created at this time.");
                return;
            }
            await ApplicationViewModel.Instance.BackupManager.CreateBackupAsync();
            ApplicationViewModel.Instance.DialogFactory?.Show("Backup created", "Backup created successfully.");
        }

        private void DefaultCellFormatEditorButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel is null) return;
            _viewModel.OpenEditorForDefaultCellFormat();
        }

        private void DefaultRowColumnCellFormatEditorButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel is null) return;
            _viewModel.OpenEditorForDefaultRowAndColumnCellFormat();
        }

        private void OpenSaveLocationButtonClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer", ApplicationViewModel.Instance.PersistedProject?.GetRootPath() ?? "");
        }

        private void PrintCurrentSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ApplicationViewModel.Instance.SheetViewModel is null) return;

            var sheetView = new SheetView(ApplicationViewModel.Instance.SheetViewModel)
            {
                Background = System.Windows.Media.Brushes.White
            };
            if (File.Exists("printPreview.xps")) File.Delete("printPreview.xps");
            XpsDocument xpsDocument = new("printPreview.xps", FileAccess.ReadWrite);
            XpsDocumentWriter xpsDocumentWriter = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            SerializerWriterCollator serializerWriterCollator = xpsDocumentWriter.CreateVisualsCollator();
            serializerWriterCollator.BeginBatchWrite();
            serializerWriterCollator.Write(sheetView);
            serializerWriterCollator.EndBatchWrite();
            FixedDocumentSequence preview = xpsDocument.GetFixedDocumentSequence();
            xpsDocument.Close();

            var previewWindow = new Window
            {
                Content = new DocumentViewer { Document = preview },
            };
            previewWindow.ShowDialog();
        }

        private async void RestoreFromBackupButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel is null) return;
            await _viewModel.RestoreFromBackupAsync();
        }

        private void ShowLogWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var logWindowViewModel = new LogWindowViewModel(ApplicationViewModel.Instance.Logger);
            ApplicationViewModel.Instance.ShowToolWindow(logWindowViewModel);
        }

        private void ShowUndoRedoStackWindowButtonClick(object sender, RoutedEventArgs e)
        {
            var undoRedoStackWindowViewModel = new UndoRedoStackWindowViewModel();
            ApplicationViewModel.Instance.ShowToolWindow(undoRedoStackWindowViewModel);
        }
    }
}
