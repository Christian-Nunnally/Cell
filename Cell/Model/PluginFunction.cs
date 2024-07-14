
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Text.RegularExpressions;
using Cell.Persistence;
using System.Text.Json.Serialization;

namespace Cell.Model
{
    public partial class PluginFunction
    {
        private const string classHeader = @"
            using System;
            using Cell.Model;
            using Cell.Plugin;                

            namespace Plugin
            {
                public class Program
                {";

        private const string classFooter = @"
                }
            }";

        private const string populateHeader = @"
                    public static object Populate(PluginContext c, CellModel cell)
                    {
                        ";
        private const string triggerHeader = @"
                    public static void Trigger(PluginContext c, CellModel cell)
                    {
                        ";
        private const string methodFooter = @"
                    }";

        private string code = string.Empty;
        private readonly List<CellModel> _cellsToNotify = [];

        public List<string> SheetDependencies { get; set; } = [];
        public List<int> RowDependencies { get; set; } = [];
        public List<int> ColumnDependencies { get; set; } = [];

        public string Name { get; set; } = string.Empty;

        public bool IsTrigger { get; set; } = false;

        public string Code
        {
            get => code; set
            {
                code = value;
                FindAndRefreshDependencies();
            }
        }

        [JsonIgnore]
        public SyntaxTree SyntaxTree { get; set; } = CSharpSyntaxTree.ParseText("");

        public void FindAndRefreshDependencies()
        {
            SheetDependencies.Clear();
            RowDependencies.Clear();
            ColumnDependencies.Clear();
            var methodHeader = IsTrigger ? triggerHeader : populateHeader;
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(classHeader + methodHeader + code + methodFooter + classFooter);
            SyntaxNode root = syntaxTree.GetRoot();
            var n = root.DescendantNodes();

            var identifierNames = root.DescendantNodes().OfType<IdentifierNameSyntax>();

            foreach (var idName in identifierNames)
            {
                var identifierName = root
                    .DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Where(x => x.Identifier.ToString() == idName.Identifier.ToString())
                    .FirstOrDefault();
                if (identifierName is null) continue;
                var variableName = identifierName.Identifier.ToString();
                if (variableName.Contains('_'))
                {
                    var splitVariableName = variableName.Split('_');
                    var sheetName = splitVariableName[0];
                    var locationString = splitVariableName[1];
                    if (IsCellLocation(locationString))
                    {
                        (var row, var column) = GetCellLocationFromVariable(locationString);
                        SheetDependencies.Add(sheetName);
                        RowDependencies.Add(row);
                        ColumnDependencies.Add(column);

                        var getCellValueSyntax = CSharpSyntaxTree.ParseText($"c.GetCellValue({sheetName}, {row}, {column})");
                        var nodes = getCellValueSyntax.GetRoot().DescendantNodes();
                        root = identifierName.ReplaceNode(identifierName, getCellValueSyntax.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First());
                    }
                }
                else
                {
                    if (IsCellLocation(variableName))
                    {
                        (var row, var column) = GetCellLocationFromVariable(variableName);
                        SheetDependencies.Add(string.Empty);
                        RowDependencies.Add(row);
                        ColumnDependencies.Add(column);

                        var getCellValueSyntax = CSharpSyntaxTree.ParseText($"c.GetCellValue(cell, {row}, {column})");
                        root = root.ReplaceNode(identifierName, getCellValueSyntax.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First());
                    }
                }
            }

            SyntaxTree = root.SyntaxTree;
            NotifyDependenciesHaveChanged();

            PluginFunctionLoader.SavePluginFunction(this, IsTrigger ? PluginFunctionLoader.TriggerFunctionsDirectoryName : PluginFunctionLoader.PopulateFunctionsDirectoryName);
        }

        private void NotifyDependenciesHaveChanged()
        {
            _cellsToNotify.ForEach(cell => cell.UpdateDependencySubscriptions(this));
        }

        private static (int Row, int Column) GetCellLocationFromVariable(string variableName)
        {
            string columnPart = GetColumnFromCellLocationString().Match(variableName).Value;
            string rowPart = GetRowFromCellLocationString().Match(variableName).Value;
            return (int.Parse(rowPart), ColumnToIndex(columnPart));
        }

        public static bool IsCellLocation(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return IsCellLocationString().IsMatch(input);
        }

        private static int ColumnToIndex(string column)
        {
            column = column.ToUpper();
            int index = 0;
            foreach (char c in column)
            {
                index = index * 26 + (c - 'A' + 1);
            }
            return index; //1 based
        }

        [GeneratedRegex(@"^[A-Za-z]+[0-9]+$")]
        private static partial Regex IsCellLocationString();

        [GeneratedRegex(@"^[A-Za-z]+")]
        private static partial Regex GetColumnFromCellLocationString();

        [GeneratedRegex(@"\d+$")]
        private static partial Regex GetRowFromCellLocationString();

        internal void StopListeningForDependencyChanges(CellModel cell)
        {
            _cellsToNotify.Remove(cell);
        }

        internal void StartListeningForDependencyChanges(CellModel cell)
        {
            _cellsToNotify.Add(cell);
        }
    }
}
