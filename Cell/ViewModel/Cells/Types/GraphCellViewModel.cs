using Cell.Common;
using Cell.Model;
using System.Windows;
using System.Windows.Media;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// A line graph cell that displays a list of points.
    /// </summary>
    public class GraphCellViewModel : CollectionCellViewModel
    {
        private PointCollection _dataPoints = ScaleAndCenterPoints(
        [
            new Point(0, 1),
            new Point(1, 5),
            new Point(2, 3),
            new Point(3, 7),
            new Point(4, 8),
        ], 10, 10);
        /// <summary>
        /// Creates a new instance of <see cref="GraphCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public GraphCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            model.PropertyChanged += (sender, args) =>
            {
                if (sender is not CellModel cell) return;
                if (args.PropertyName == nameof(CellModel.Width))
                {
                    UpdatePointsScaling();
                }
                if (args.PropertyName == nameof(CellModel.Height))
                {
                    UpdatePointsScaling();
                }
            };
        }

        /// <summary>
        /// Gets the list of points to display the correctly scaled (based on the nodes width and height) graph in the UI.
        /// </summary>
        public PointCollection DataPoints
        {
            get => _dataPoints;
            set
            {
                _dataPoints = value;
                NotifyPropertyChanged(nameof(DataPoints));
            }
        }

        /// <summary>
        /// Updates the collection of this <see cref="CollectionCellViewModel"/> with the given items.
        /// </summary>
        /// <param name="items">The items to populate the collection with.</param>
        protected override void UpdateCollection(object? items)
        {
            base.UpdateCollection(items);
            UpdatePointsScaling();
        }

        private static PointCollection ScaleAndCenterPoints(List<Point> points, double targetWidth, double targetHeight)
        {
            const int Margin = 1;
            if (points.Count == 0) return [];
            var bounds = Utilities.GetBoundingRectangle(points);

            double scaleX = targetWidth / bounds.Width;
            double scaleY = targetHeight / bounds.Height;

            List<Point> scaledAndCenteredPoints = [];
            foreach (var point in points)
            {
                double scaledX = (point.X - bounds.X) * scaleX;
                double scaledY = (point.Y - bounds.Y) * scaleY;
                scaledAndCenteredPoints.Add(new Point(scaledX + Margin, scaledY + Margin));
            }

            return [.. scaledAndCenteredPoints];
        }

        private List<Point> CreatePointsFromData()
        {
            var points = new List<Point>();
            var index = 0;
            for (int i = 0; i < Collection.Count; i++)
            {
                if (double.TryParse(Collection[i].ToString(), out double value))
                {
                    points.Add(new Point(index++, -value));
                }
            }

            return points;
        }

        private void UpdatePointsScaling()
        {
            const int Margin = 5;
            var points = CreatePointsFromData();
            DataPoints = ScaleAndCenterPoints(points, Model.Width - Margin - Margin, Model.Height - Margin - Margin);
        }
    }
}
