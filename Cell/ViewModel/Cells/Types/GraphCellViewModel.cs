using Cell.Model;
using System.Windows;
using System.Windows.Media;

namespace Cell.ViewModel
{
    public class GraphCellViewModel : CellViewModel
    {
        private readonly List<Point> _rawDataPoints = [];

        public GraphCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            model.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(Text))
                {
                    Text = model.Text;
                }
            };
        }

        public PointCollection DataPoints { get; set; } = ScaleAndCenterPoints(
            [
                new Point(0, 0),
                new Point(5, 5),
                new Point(10, 20),
                new Point(15, 1),
                new Point(20, 3),
                new Point(25, 44),
                new Point(30, 2),
                new Point(35, 3),
            ], 115, 15);

        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                if (double.TryParse(value, out double result))
                {
                    _rawDataPoints.Add(new Point(_rawDataPoints.Count, result));
                    UpdatePoints();
                }
            }
        }

        public void UpdatePoints()
        {
            while(_rawDataPoints.Count > 8) _rawDataPoints.RemoveAt(0);

            for (int i = 0; i < _rawDataPoints.Count; i++)
            {
                _rawDataPoints[i] = new Point(i, _rawDataPoints[i].Y);
            }
            DataPoints = ScaleAndCenterPoints(_rawDataPoints, (int)Model.Width - 10, (int)Model.Height - 10);
            OnPropertyChanged(nameof(DataPoints));
        }

        static PointCollection ScaleAndCenterPoints(List<Point> points, int targetWidth, int targetHeight)
        {
            // Find the current bounding box of the points
            double minX = points.Min(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxX = points.Max(p => p.X);
            double maxY = points.Max(p => p.Y);

            // Calculate scaling factors
            double scaleX = targetWidth / (maxX - minX);
            double scaleY = targetHeight / (maxY - minY);

            // Scale and center the points
            List<Point> scaledAndCenteredPoints = [];
            foreach (var point in points)
            {
                double scaledX = (point.X - minX) * scaleX + 4;
                double scaledY = (point.Y - minY) * scaleY + 4;
                scaledAndCenteredPoints.Add(new Point(scaledX, scaledY));
            }

            return [.. scaledAndCenteredPoints];
        }
    }
}