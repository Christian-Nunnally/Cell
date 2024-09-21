using Cell.Common;
using Cell.Model;
using System.Text.Json;

namespace Cell.Persistence.Migration
{
    public class V1ToV2Migrator : IMigrator
    {
        public bool Migrate(PersistedDirectory persistenceManager)
        {
            foreach (var sheet in persistenceManager.GetDirectories("Sheets"))
            {
                foreach (var cell in persistenceManager.GetFiles(sheet))
                {
                    var oldCellModelSerialized = persistenceManager.LoadFile(cell) ?? throw new CellError("Unable to load cell for migration");
                    var oldCellModel = JsonSerializer.Deserialize<OldCellModel>(oldCellModelSerialized) ?? throw new CellError("Deserialization failed for " + oldCellModelSerialized);
                    var newCellModel = new CellModel
                    {
                        ID = oldCellModel.ID,
                        SheetName = oldCellModel.SheetName,
                        Text = oldCellModel.Text,
                        CellType = oldCellModel.CellType,
                        Column = oldCellModel.Column,
                        Row = oldCellModel.Row,
                        Height = oldCellModel.Height,
                        Width = oldCellModel.Width,
                        Index = oldCellModel.Index,
                        PopulateFunctionName = oldCellModel.PopulateFunctionName,
                        TriggerFunctionName = oldCellModel.TriggerFunctionName,
                        BooleanProperties = oldCellModel.BooleanProperties,
                        NumericProperties = oldCellModel.NumericProperties,
                        StringProperties = oldCellModel.StringProperties,
                        MergedWith = oldCellModel.MergedWith,
                        Style = new CellStyleModel
                        {
                            BackgroundColor = oldCellModel.ColorHexes[(int)ColorFor.Background],
                            BorderColor = oldCellModel.ColorHexes[(int)ColorFor.Border],
                            ContentBackgroundColor = oldCellModel.ColorHexes[(int)ColorFor.ContentBackground],
                            ContentBorderColor = oldCellModel.ColorHexes[(int)ColorFor.ContentBorder],
                            ForegroundColor = oldCellModel.ColorHexes[(int)ColorFor.Foreground],
                            HighlightColor = oldCellModel.ColorHexes[(int)ColorFor.ContentHighlight],
                            Border = oldCellModel.BorderThicknessString,
                            ContentBorder = oldCellModel.ContentBorderThicknessString,
                            ContentMargin = oldCellModel.MarginString,
                            Font = oldCellModel.FontFamily,
                            FontSize = oldCellModel.FontSize,
                            Bold = oldCellModel.IsFontBold,
                            Italic = oldCellModel.IsFontItalic,
                            Strikethrough = oldCellModel.IsFontStrikethrough,
                            TextAlignment = oldCellModel.TextAlignmentForView,
                            HorizontalAlignment = oldCellModel.HorizontalAlignment,
                            VerticalAlignment = oldCellModel.VerticalAlignment,
                        }
                    };
                    var newCellSerialized = JsonSerializer.Serialize(newCellModel);
                    persistenceManager.SaveFile(cell, newCellSerialized);

                }
            }
            return true;
        }
    }
}
