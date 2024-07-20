

namespace Cell.Model
{
    internal static class Utilities
    {
        public static string GenerateUnqiueId(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                var randomNumberInRange = random.Next(chars.Length);
                stringChars[i] = chars[randomNumberInRange];
            }

            return new string(stringChars);
        }

        public static string GetUnqiueLocationString(string sheet, int row, int column) => $"{sheet}_{row}_{column}";

        public static string GetUnqiueLocationString(this CellModel model) => GetUnqiueLocationString(model.SheetName, model.Row, model.Column);
    }
}
