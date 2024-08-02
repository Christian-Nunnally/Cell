using Cell.Model;
using Cell.Persistence;
using System.Collections;
using System.Windows;
using System.Windows.Media;

namespace Cell.ViewModel
{
    public class GraphCellViewModel : CellViewModel
    {
        private bool _addingPoint = false;
        private readonly List<Point> _rawDataPoints = [];

        public GraphCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
            model.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(Text))
                {
                    UpdatePointsFromCellText();
                }

                if (args.PropertyName == nameof(Width))
                {
                    UpdatePointsScaling();
                }

                if (args.PropertyName == nameof(Height))
                {
                    UpdatePointsScaling();
                }
            };
            UpdatePointsFromCellText();
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
            ], 10, 10);

        public override string Text
        {
            get => base.Text;
            set
            {
                if (_addingPoint) return;
                _addingPoint = true;
                base.Text = value;
                UpdatePointsFromCellText();
                _addingPoint = false;
            }
        }

        public string MaxPointsString
        {
            get => MaxPoints.ToString();
            set
            {
                if (int.TryParse(value, out var intValue))
                {
                    MaxPoints = intValue;
                    NotifyPropertyChanged(nameof(MaxPoints));
                }
            }
        }

        public int MaxPoints
        {
            get => (int)Model.GetNumericProperty(nameof(MaxPoints));
            set
            {
                Model.SetNumericProperty(nameof(MaxPoints), value);
                NotifyPropertyChanged(nameof(MaxPoints));
                NotifyPropertyChanged(nameof(MaxPointsString));
            }
        }

        private void UpdatePointsFromCellText()
        {
            if (!Text.Contains(',') && double.TryParse(Text, out double result))
            {
                _rawDataPoints.Add(new Point(_rawDataPoints.Count, -result));
            }
            else
            {
                var split = Text.Split(',');
                _rawDataPoints.Clear();
                foreach (var s in split)
                {
                    if (double.TryParse(s, out double r))
                    {
                        _rawDataPoints.Add(new Point(_rawDataPoints.Count, -r));
                    }
                }
            }
            UpdatePointsScaling();
        }

        public void UpdatePointsScaling()
        {
            while(_rawDataPoints.Count > MaxPoints) _rawDataPoints.RemoveAt(0);

            for (int i = 0; i < _rawDataPoints.Count; i++)
            {
                _rawDataPoints[i] = new Point(i, _rawDataPoints[i].Y);
            }
            DataPoints = ScaleAndCenterPoints(_rawDataPoints, (int)Model.Width - 10, (int)Model.Height - 10);
            NotifyPropertyChanged(nameof(DataPoints));
        }

        static PointCollection ScaleAndCenterPoints(List<Point> points, int targetWidth, int targetHeight)
        {
            if (points.Count == 0) return [];

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
                double scaledX = (point.X - minX) * scaleX;
                double scaledY = (point.Y - minY) * scaleY;
                scaledAndCenteredPoints.Add(new Point(scaledX + 1, scaledY + 1));
            }

            return [.. scaledAndCenteredPoints];
        }
    }
}