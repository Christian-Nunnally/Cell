using Cell.Core.Persistence;
using Cell.Core.Persistence.Migration;
using Cell.Model;
using System.IO;
using System.Text.Json;

namespace Cell
{
    internal class FasterLoadMigrator : IMigrator
    {
        public bool Migrate(PersistedDirectory persistedDirectory)
        {
            foreach (var sheet in persistedDirectory.GetDirectories("Sheets"))
            {
                var sheetsDirectory = persistedDirectory.FromDirectory(sheet);
                foreach (var cell in sheetsDirectory.GetFiles())
                {
                    var cellModel = JsonSerializer.Deserialize<CellModel>(sheetsDirectory.LoadFile(cell));
                    SaveCell(cellModel, sheetsDirectory);
                }
            }
            return true;
        }

        public void SaveCell(CellModel cell, PersistedDirectory sheetsDirectory)
        {
            var fileName = cell.ID;
            var serialized = JsonSerializer.Serialize(cell);
            var path = fileName;
            if (cell.CellType == CellType.Corner)
            {
                fileName = "corner";
                // TODO: removed after migration
                if (!string.IsNullOrEmpty(sheetsDirectory.LoadFile(fileName))) sheetsDirectory.DeleteFile(fileName);

                path = Path.Combine("Corner", fileName);
            }
            if (cell.CellType == CellType.Row)
            {
                // TODO: removed after migration
                if (!string.IsNullOrEmpty(sheetsDirectory.LoadFile(fileName))) sheetsDirectory.DeleteFile(fileName);

                path = Path.Combine("Rows", fileName);
            }
            if (cell.CellType == CellType.Column)
            {
                // TODO: removed after migration
                if (!string.IsNullOrEmpty(sheetsDirectory.LoadFile(fileName))) sheetsDirectory.DeleteFile(fileName);

                path = Path.Combine("Columns", fileName);
            }
            sheetsDirectory.SaveFile(path, serialized);
        }
    }
}