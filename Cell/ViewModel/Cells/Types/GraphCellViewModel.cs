using Cell.Model;
using System.Windows;
using System.Windows.Media;

namespace Cell.ViewModel
{
    public class GraphCellViewModel(CellModel model, SheetViewModel sheetViewModel) : CellViewModel(model, sheetViewModel)
    {
        public PointCollection DataPoints { get; set; } =
            [
                new Point(0, 0),
                new Point(5, 5),
                new Point(10, 20),
                new Point(15, 1),
                new Point(20, 3),
                new Point(25, 44),
                new Point(30, 2),
                new Point(35, 3),
            ];
    }
}