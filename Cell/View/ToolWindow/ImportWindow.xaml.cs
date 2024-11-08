﻿using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class ImportWindow : ResizableToolWindow
    {
        private readonly ImportWindowViewModel _viewModel;
        /// <summary>
        /// Creates a new instance of the <see cref="ImportWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public ImportWindow(ImportWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void ImportSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.ImportingTemplateName))
            {
                ApplicationViewModel.Instance.DialogFactory?.Show("No template selected", "Please select a template to import.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_viewModel.NewSheetNameForImportedTemplates))
            {
                ApplicationViewModel.Instance.DialogFactory?.Show("No sheet name", "Please enter a name for the new sheet.");
                return;
            }
            //var templateName = _viewModel.ImportingTemplateName;
            //var sheetName = _viewModel.NewSheetNameForImportedTemplates;
            ApplicationViewModel.Instance.DialogFactory?.Show("Not finished :)", $"Not finished :)");
        }
    }
}
