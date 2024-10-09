﻿using Cell.Common;
using Cell.Model;
using Cell.Persistence;
using Cell.Persistence.Migration;
using System.Text.Json;

namespace Cell.Core.Persistence.Migration
{
    internal class Migration : IMigrator
    {
        public bool Migrate(PersistedDirectory persistedDirectory)
        {
            var sheets = persistedDirectory.GetDirectories("Sheets");
            foreach (var sheet in sheets)
            {
                var cells = persistedDirectory.GetFiles(sheet);
                foreach (var cell in cells)
                {
                    var cellModel = JsonSerializer.Deserialize<CellModelOld>(persistedDirectory.LoadFile(cell) ?? throw new CellError());
                    if (cellModel is null) continue;
                    var migratedCell = MigrateCell(cellModel);
                    persistedDirectory.SaveFile(cell, JsonSerializer.Serialize(migratedCell));
                }
            }
            return true;
        }

        private CellModel MigrateCell(CellModelOld cellModel)
        {
            var migrated = new CellModel
            {
                Style = cellModel.Style,
                Text = cellModel.Text,
                CellType = cellModel.CellType,
                ID = cellModel.ID,
                Height = cellModel.Height,
                Width = cellModel.Width,
                Index = cellModel.Index,
                MergedWith = cellModel.MergedWith,
                TriggerFunctionName = cellModel.TriggerFunctionName,
                PopulateFunctionName = cellModel.PopulateFunctionName
            };
            migrated.Location.SheetName = cellModel.SheetName;
            migrated.Location.Row = cellModel.Row;
            migrated.Location.Column = cellModel.Column;
            
            foreach (var property in cellModel.StringProperties)
            {
                migrated.Properties[property.Key] = property.Value;
            }
            foreach (var property in cellModel.BooleanProperties)
            {
                migrated.Properties.SetBooleanProperty(property.Key, property.Value);
            }
            foreach (var property in cellModel.NumericProperties)
            {
                migrated.Properties.SetNumericProperty(property.Key, property.Value);
            }

            return migrated;
        }
    }
}