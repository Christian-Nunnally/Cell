﻿using Cell.Core.Execution.References;
using Cell.Model;

namespace Cell.Execution.References
{
    public interface ICollectionReference : IReferenceFromCell
    {
        event Action? LocationsThatWillInvalidateCollectionNameForCellHaveChanged;

        string GetCollectionName(CellModel cell, PluginContext pluginFunctionRunContext);

        IEnumerable<string> GetLocationsThatWillInvalidateCollectionNameForCell(CellModel cell);
    }
}
