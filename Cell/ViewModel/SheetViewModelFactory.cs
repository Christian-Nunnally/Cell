namespace Cell.ViewModel
{
    public static class SheetViewModelFactory
    {
        public static Dictionary<string, SheetViewModel> Sheets { get; set; } = [];

        public static SheetViewModel GetOrCreate(string sheetName)
        {
            if (Sheets.TryGetValue(sheetName, out SheetViewModel? value))
            {
                return value;
            }
            else
            {
                var sheetViewModel = new SheetViewModel(sheetName);
                Sheets[sheetName] = sheetViewModel;
                return sheetViewModel;
            }
        }
    }
}
