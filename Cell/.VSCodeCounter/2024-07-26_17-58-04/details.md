# Details

Date : 2024-07-26 17:58:04

Directory c:\\Users\\chris\\source\\repos\\Cell\\Cell

Total : 90 files,  6856 codes, 85 comments, 825 blanks, all 7766 lines

[Summary](results.md) / Details / [Diff Summary](diff.md) / [Diff Details](diff-details.md)

## Files
| filename | language | code | comment | blank | total |
| :--- | :--- | ---: | ---: | ---: | ---: |
| [App.xaml](/App.xaml) | XML | 19 | 0 | 1 | 20 |
| [App.xaml.cs](/App.xaml.cs) | C# | 7 | 3 | 2 | 12 |
| [AssemblyInfo.cs](/AssemblyInfo.cs) | C# | 5 | 4 | 2 | 11 |
| [Cell.csproj](/Cell.csproj) | XML | 33 | 0 | 8 | 41 |
| [Common/Utilities.cs](/Common/Utilities.cs) | C# | 53 | 0 | 8 | 61 |
| [Data/Cells.cs](/Data/Cells.cs) | C# | 105 | 5 | 16 | 126 |
| [Exceptions/PluginFunctionSyntaxTransformException.cs](/Exceptions/PluginFunctionSyntaxTransformException.cs) | C# | 16 | 0 | 2 | 18 |
| [Exceptions/ProjectLoadException.cs](/Exceptions/ProjectLoadException.cs) | C# | 16 | 0 | 2 | 18 |
| [Model/ApplicationSettings.cs](/Model/ApplicationSettings.cs) | C# | 60 | 0 | 11 | 71 |
| [Model/CellLocation.cs](/Model/CellLocation.cs) | C# | 13 | 0 | 8 | 21 |
| [Model/CellModel.cs](/Model/CellModel.cs) | C# | 262 | 2 | 42 | 306 |
| [Model/CellModelExtensions.cs](/Model/CellModelExtensions.cs) | C# | 22 | 0 | 6 | 28 |
| [Model/CellModelFactory.cs](/Model/CellModelFactory.cs) | C# | 34 | 0 | 4 | 38 |
| [Model/CellType.cs](/Model/CellType.cs) | C# | 26 | 0 | 3 | 29 |
| [Model/ColorFor.cs](/Model/ColorFor.cs) | C# | 12 | 0 | 2 | 14 |
| [Model/PluginFunction.cs](/Model/PluginFunction.cs) | C# | 131 | 0 | 27 | 158 |
| [Model/Plugin/PluginModel.cs](/Model/Plugin/PluginModel.cs) | C# | 23 | 0 | 7 | 30 |
| [Model/Plugin/TodoItem.cs](/Model/Plugin/TodoItem.cs) | C# | 59 | 0 | 11 | 70 |
| [Model/PropertyChangedBase.cs](/Model/PropertyChangedBase.cs) | C# | 17 | 0 | 4 | 21 |
| [Persistence/CellLoader.cs](/Persistence/CellLoader.cs) | C# | 59 | 0 | 10 | 69 |
| [Persistence/PersistenceManager.cs](/Persistence/PersistenceManager.cs) | C# | 59 | 0 | 10 | 69 |
| [Persistence/PluginFunctionLoader.cs](/Persistence/PluginFunctionLoader.cs) | C# | 87 | 0 | 11 | 98 |
| [Persistence/UserCollectionLoader.cs](/Persistence/UserCollectionLoader.cs) | C# | 114 | 0 | 20 | 134 |
| [Plugin/CellPopulateManager.cs](/Plugin/CellPopulateManager.cs) | C# | 178 | 3 | 24 | 205 |
| [Plugin/CellTriggerManager.cs](/Plugin/CellTriggerManager.cs) | C# | 28 | 3 | 5 | 36 |
| [Plugin/CellValueProvider.cs](/Plugin/CellValueProvider.cs) | C# | 7 | 0 | 2 | 9 |
| [Plugin/CompileResult.cs](/Plugin/CompileResult.cs) | C# | 8 | 0 | 3 | 11 |
| [Plugin/DynamicCellPluginExecutor.cs](/Plugin/DynamicCellPluginExecutor.cs) | C# | 42 | 0 | 5 | 47 |
| [Plugin/EditContext.cs](/Plugin/EditContext.cs) | C# | 10 | 0 | 5 | 15 |
| [Plugin/PluginContext.cs](/Plugin/PluginContext.cs) | C# | 32 | 0 | 9 | 41 |
| [Plugin/RoslynCompiler.cs](/Plugin/RoslynCompiler.cs) | C# | 68 | 0 | 10 | 78 |
| [Plugin/SyntaxRewriters/CellReferenceSyntaxWalker.cs](/Plugin/SyntaxRewriters/CellReferenceSyntaxWalker.cs) | C# | 69 | 0 | 12 | 81 |
| [Plugin/SyntaxRewriters/CellReferenceToCodeSyntaxWalker.cs](/Plugin/SyntaxRewriters/CellReferenceToCodeSyntaxWalker.cs) | C# | 89 | 0 | 13 | 102 |
| [Plugin/SyntaxRewriters/CodeToCellReferenceSyntaxRewriter.cs](/Plugin/SyntaxRewriters/CodeToCellReferenceSyntaxRewriter.cs) | C# | 48 | 0 | 8 | 56 |
| [Plugin/SyntaxRewriters/FindAndReplaceCollectionReferencesSyntaxWalker.cs](/Plugin/SyntaxRewriters/FindAndReplaceCollectionReferencesSyntaxWalker.cs) | C# | 32 | 0 | 7 | 39 |
| [Plugin/UserCollection.cs](/Plugin/UserCollection.cs) | C# | 49 | 1 | 12 | 62 |
| [Plugin/UserList.cs](/Plugin/UserList.cs) | C# | 57 | 0 | 14 | 71 |
| [T2.md](/T2.md) | Markdown | 4 | 0 | 2 | 6 |
| [ViewModel/ApplicationViewModel.cs](/ViewModel/ApplicationViewModel.cs) | C# | 182 | 0 | 26 | 208 |
| [ViewModel/CellClipboard.cs](/ViewModel/CellClipboard.cs) | C# | 83 | 0 | 10 | 93 |
| [ViewModel/CellLayout.cs](/ViewModel/CellLayout.cs) | C# | 110 | 0 | 15 | 125 |
| [ViewModel/Cells/CellViewModel.cs](/ViewModel/Cells/CellViewModel.cs) | C# | 353 | 19 | 56 | 428 |
| [ViewModel/Cells/CellViewModelFactory.cs](/ViewModel/Cells/CellViewModelFactory.cs) | C# | 30 | 0 | 3 | 33 |
| [ViewModel/Cells/Types/ButtonCellViewModel.cs](/ViewModel/Cells/Types/ButtonCellViewModel.cs) | C# | 22 | 0 | 3 | 25 |
| [ViewModel/Cells/Types/CheckboxCellViewModel.cs](/ViewModel/Cells/Types/CheckboxCellViewModel.cs) | C# | 57 | 0 | 11 | 68 |
| [ViewModel/Cells/Types/DropdownCellViewModel.cs](/ViewModel/Cells/Types/DropdownCellViewModel.cs) | C# | 39 | 0 | 5 | 44 |
| [ViewModel/Cells/Types/GraphCellViewModel.cs](/ViewModel/Cells/Types/GraphCellViewModel.cs) | C# | 84 | 3 | 13 | 100 |
| [ViewModel/Cells/Types/LabelCellViewModel.cs](/ViewModel/Cells/Types/LabelCellViewModel.cs) | C# | 7 | 0 | 1 | 8 |
| [ViewModel/Cells/Types/ListCellViewModel.cs](/ViewModel/Cells/Types/ListCellViewModel.cs) | C# | 72 | 0 | 7 | 79 |
| [ViewModel/Cells/Types/ProgressCellViewModel.cs](/ViewModel/Cells/Types/ProgressCellViewModel.cs) | C# | 32 | 0 | 5 | 37 |
| [ViewModel/Cells/Types/Special/ColumnCellViewModel.cs](/ViewModel/Cells/Types/Special/ColumnCellViewModel.cs) | C# | 102 | 0 | 16 | 118 |
| [ViewModel/Cells/Types/Special/CornerCellViewModel.cs](/ViewModel/Cells/Types/Special/CornerCellViewModel.cs) | C# | 15 | 0 | 5 | 20 |
| [ViewModel/Cells/Types/Special/RowCellViewModel.cs](/ViewModel/Cells/Types/Special/RowCellViewModel.cs) | C# | 87 | 0 | 15 | 102 |
| [ViewModel/Cells/Types/Special/SpecialCellViewModel.cs](/ViewModel/Cells/Types/Special/SpecialCellViewModel.cs) | C# | 7 | 0 | 3 | 10 |
| [ViewModel/Cells/Types/TextboxCellViewModel.cs](/ViewModel/Cells/Types/TextboxCellViewModel.cs) | C# | 30 | 0 | 3 | 33 |
| [ViewModel/CodeCompletionWindowFactory.cs](/ViewModel/CodeCompletionWindowFactory.cs) | C# | 125 | 3 | 13 | 141 |
| [ViewModel/CodeEditorViewModel.cs](/ViewModel/CodeEditorViewModel.cs) | C# | 16 | 0 | 4 | 20 |
| [ViewModel/RelayCommand.cs](/ViewModel/RelayCommand.cs) | C# | 22 | 0 | 5 | 27 |
| [ViewModel/SheetViewModel.cs](/ViewModel/SheetViewModel.cs) | C# | 249 | 0 | 33 | 282 |
| [ViewModel/SheetViewModelFactory.cs](/ViewModel/SheetViewModelFactory.cs) | C# | 20 | 0 | 2 | 22 |
| [View/ApplicationView.xaml](/View/ApplicationView.xaml) | XML | 119 | 0 | 4 | 123 |
| [View/ApplicationView.xaml.cs](/View/ApplicationView.xaml.cs) | C# | 151 | 0 | 17 | 168 |
| [View/CellTextEditBar.xaml](/View/CellTextEditBar.xaml) | XML | 36 | 0 | 1 | 37 |
| [View/CellTextEditBar.xaml.cs](/View/CellTextEditBar.xaml.cs) | C# | 20 | 3 | 3 | 26 |
| [View/CodeEditor.xaml](/View/CodeEditor.xaml) | XML | 119 | 0 | 1 | 120 |
| [View/CodeEditor.xaml.cs](/View/CodeEditor.xaml.cs) | C# | 228 | 7 | 37 | 272 |
| [View/Controls/BetterCheckBox.cs](/View/Controls/BetterCheckBox.cs) | C# | 58 | 0 | 12 | 70 |
| [View/Controls/BetterComboBox.cs](/View/Controls/BetterComboBox.cs) | C# | 16 | 0 | 3 | 19 |
| [View/Controls/generic.xaml](/View/Controls/generic.xaml) | XML | 47 | 0 | 0 | 47 |
| [View/Converters/BrushToColorConverter.cs](/View/Converters/BrushToColorConverter.cs) | C# | 22 | 0 | 3 | 25 |
| [View/Converters/Converters.xaml](/View/Converters/Converters.xaml) | XML | 14 | 0 | 0 | 14 |
| [View/Converters/EnumToStringConverter.cs](/View/Converters/EnumToStringConverter.cs) | C# | 24 | 1 | 3 | 28 |
| [View/Converters/RGBHexColorConverter.cs](/View/Converters/RGBHexColorConverter.cs) | C# | 32 | 0 | 4 | 36 |
| [View/EditCellPanel.xaml](/View/EditCellPanel.xaml) | XML | 387 | 0 | 1 | 388 |
| [View/EditCellPanel.xaml.cs](/View/EditCellPanel.xaml.cs) | C# | 279 | 3 | 38 | 320 |
| [View/PanAndZoomCanvas.xaml](/View/PanAndZoomCanvas.xaml) | XML | 11 | 0 | 1 | 12 |
| [View/PanAndZoomCanvas.xaml.cs](/View/PanAndZoomCanvas.xaml.cs) | C# | 189 | 4 | 28 | 221 |
| [View/PluginContextCompletionData.cs](/View/PluginContextCompletionData.cs) | C# | 28 | 1 | 7 | 36 |
| [View/SheetView.xaml](/View/SheetView.xaml) | XML | 292 | 1 | 5 | 298 |
| [View/SheetView.xaml.cs](/View/SheetView.xaml.cs) | C# | 155 | 3 | 14 | 172 |
| [View/Skin/ColorPicker.xaml](/View/Skin/ColorPicker.xaml) | XML | 332 | 10 | 9 | 351 |
| [View/Skin/Controls.xaml](/View/Skin/Controls.xaml) | XML | 43 | 0 | 3 | 46 |
| [View/Skin/Dark.xaml](/View/Skin/Dark.xaml) | XML | 8 | 0 | 2 | 10 |
| [View/Skin/Light.xaml](/View/Skin/Light.xaml) | XML | 8 | 0 | 0 | 8 |
| [View/Skin/Styles.xaml](/View/Skin/Styles.xaml) | XML | 348 | 0 | 14 | 362 |
| [View/TitleBarSheetNavigation.xaml](/View/TitleBarSheetNavigation.xaml) | XML | 51 | 0 | 1 | 52 |
| [View/TitleBarSheetNavigation.xaml.cs](/View/TitleBarSheetNavigation.xaml.cs) | C# | 57 | 3 | 5 | 65 |
| [View/TypeSpecificEditCellPanel.xaml](/View/TypeSpecificEditCellPanel.xaml) | XML | 48 | 0 | 1 | 49 |
| [View/TypeSpecificEditCellPanel.xaml.cs](/View/TypeSpecificEditCellPanel.xaml.cs) | C# | 16 | 3 | 3 | 22 |
| [View/ViewUtilities.cs](/View/ViewUtilities.cs) | C# | 21 | 0 | 3 | 24 |

[Summary](results.md) / Details / [Diff Summary](diff.md) / [Diff Details](diff-details.md)