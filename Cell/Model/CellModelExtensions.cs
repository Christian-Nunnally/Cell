﻿using Cell.Data;

namespace Cell.Model
{
    internal static class CellModelExtensions
    {
        public static int CellsMergedToRight(this CellModel model)
        {
            var count = 0;
            while (model.IsMerged() && (Cells.GetCell(model.SheetName, model.Row, model.Column + 1 + count)?.IsMergedWith(model) ?? false)) count++;
            return count;
        }

        public static int CellsMergedBelow(this CellModel model)
        {
            var count = 0;
            while (model.IsMerged() && (Cells.GetCell(model.SheetName, model.Row + 1 + count, model.Column)?.IsMergedWith(model) ?? false)) count++;
            return count;
        }

        public static bool IsMerged(this CellModel model) => !string.IsNullOrWhiteSpace(model.MergedWith);

        public static bool IsMergedWith(this CellModel model, CellModel other) => model.MergedWith == other.MergedWith;

        public static bool IsMergedParent(this CellModel model) => model.MergedWith == model.ID;
    }
}
