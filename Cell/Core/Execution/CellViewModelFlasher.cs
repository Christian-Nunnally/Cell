using Cell.Core.Common;
using Cell.Model;
using Cell.ViewModel.Cells;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cell.Core.Execution
{
    /// <summary>
    /// Class to handle flashing of cells.
    /// </summary>
    public class CellViewModelFlasher
    {
        private const int NumberOfFrames = 16;
        private const double _brightnessSpeed = 0.01;
        private readonly Dictionary<CellViewModel, int> _flashingCells = [];
        private readonly Dictionary<CellViewModel, CellStyleModel> _cellStartColors = [];
        private DispatcherTimer _timer;

        /// <summary>
        /// Creates a new instance of <see cref="CellViewModelFlasher"/>.
        /// </summary>
        public CellViewModelFlasher()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(16), // ~60 FPS
            };
            _timer.Tick += TimerTick;
        }

        /// <summary>
        /// Causes the given cell to flash.
        /// </summary>
        /// <param name="cellViewModel"></param>
        public void Flash(CellViewModel cellViewModel)
        {
            if (_flashingCells.TryAdd(cellViewModel, NumberOfFrames))
            {
                _cellStartColors.Add(cellViewModel, cellViewModel.Model.Style);
                var shouldStartAnimationTimer = _flashingCells.Count != 0;
                if (shouldStartAnimationTimer)
                {
                    _timer.Start();
                }
            }
        }

        /// <summary>
        /// Adjusts the brightness of a color in a way agnostic to the original color.
        /// </summary>
        /// <param name="originalColor">The starting color.</param>
        /// <param name="adjustmentFactor">The amount the adjust the brightness to flash the color.</param>
        /// <returns></returns>
        public static Color FlashColor(Color originalColor, double adjustmentFactor)
        {
            int r = originalColor.R;
            int g = originalColor.G;
            int b = originalColor.B;

            float brightness = (r + g + b) / 3f;

            if (r == 0 && g == 0 && b == 0)
            {
                r = (int)Math.Min(r + (255 * adjustmentFactor), 255);
                g = (int)Math.Min(g + (255 * adjustmentFactor), 255);
                b = (int)Math.Min(b + (255 * adjustmentFactor), 255);
            }
            else if (brightness < 128)
            {
                r = (int)Math.Min(r * (1 + adjustmentFactor), 255);
                g = (int)Math.Min(g * (1 + adjustmentFactor), 255);
                b = (int)Math.Min(b * (1 + adjustmentFactor), 255);
            }
            else if (brightness > 128)
            {
                r = (int)Math.Max(r * (1 - adjustmentFactor), 0);
                g = (int)Math.Max(g * (1 - adjustmentFactor), 0);
                b = (int)Math.Max(b * (1 - adjustmentFactor), 0);
            }
            else 
            {
                r = (int)Math.Min(Math.Max(r * (1 + adjustmentFactor), 0), 255);
                g = (int)Math.Min(Math.Max(g * (1 + adjustmentFactor), 0), 255);
                b = (int)Math.Min(Math.Max(b * (1 + adjustmentFactor), 0), 255);
            }

            return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            var cellsToRemove = new List<CellViewModel>();
            foreach (var (cellViewModel, framesLeft) in _flashingCells)
            {
                if (framesLeft <= 0)
                {
                    cellViewModel.BackgroundColor = ColorAdjuster.ConvertHexStringToBrush(cellViewModel.Model.Style.BackgroundColor);
                    cellViewModel.ContentBackgroundColor = ColorAdjuster.ConvertHexStringToBrush(cellViewModel.Model.Style.ContentBackgroundColor);
                    cellViewModel.ForegroundColor = ColorAdjuster.ConvertHexStringToBrush(cellViewModel.Model.Style.ForegroundColor);
                    cellsToRemove.Add(cellViewModel);
                    continue;
                }

                _flashingCells[cellViewModel]--;

                var colorBackground = ColorAdjuster.ConvertHexStringToColor(_cellStartColors[cellViewModel].BackgroundColor);
                Color newColorBackground = FlashColor(colorBackground, .5 * Math.Sin(framesLeft / (double)NumberOfFrames * Math.PI));
                var brushBackground = new SolidColorBrush(newColorBackground);
                cellViewModel.BackgroundColor = brushBackground;

                var colorContentBackgroundColor = ColorAdjuster.ConvertHexStringToColor(_cellStartColors[cellViewModel].ContentBackgroundColor);
                Color newColorContentBackgroundColor = FlashColor(colorContentBackgroundColor, .5 * Math.Sin(framesLeft / (double)NumberOfFrames * Math.PI));
                var brushContentBackgroundColor = new SolidColorBrush(newColorContentBackgroundColor);
                cellViewModel.ContentBackgroundColor = brushContentBackgroundColor;

                var colorForegroundColor = ColorAdjuster.ConvertHexStringToColor(_cellStartColors[cellViewModel].ForegroundColor);
                Color newColorForegroundColor = FlashColor(colorForegroundColor, .5 * Math.Sin(framesLeft / (double)NumberOfFrames * Math.PI));
                var brushForegroundColor = new SolidColorBrush(newColorForegroundColor);
                cellViewModel.ForegroundColor = brushForegroundColor;

            }
            foreach (var cell in cellsToRemove)
            {
                _flashingCells.Remove(cell);
                _cellStartColors.Remove(cell);
            }
            if (_flashingCells.Count == 0)
            {
                _timer?.Stop();
            }
        }
    }
}
