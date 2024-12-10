using Cell.Core.Common;
using Cell.ViewModel.Cells;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Cell.Core.Execution
{
    public class CellViewModelFlasher
    {
        private Dictionary<CellViewModel, DateTime> _flashingCells = new Dictionary<CellViewModel, DateTime>();

        private DispatcherTimer? _timer;

        private Color _startColor = Colors.White;  // Starting color
        private double _brightnessFactor = 0;    // To control brightness
        private bool _increasingBrightness = true; // To toggle brightness increase/decrease
        private const double _brightnessSpeed = 0.01;

        public void Flash(CellViewModel cellViewModel)
        {
            if (!_flashingCells.ContainsKey(cellViewModel))
            {
                var shouldStartAnimationTimer = _flashingCells.Count == 0;
                
                _flashingCells.Add(cellViewModel, DateTime.Now);

                if (shouldStartAnimationTimer)
                {
                    _timer?.Stop();
                    _timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
                    };
                    _timer.Tick += TimerTick;
                    _timer.Start();
                }
            }
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            foreach (var (cellViewModel, startTime) in _flashingCells.ToDictionary())
            {
                if (startTime.AddMilliseconds(500) < DateTime.Now)
                {
                    cellViewModel.BackgroundColor = ColorAdjuster.ConvertHexStringToBrush(cellViewModel.Model.Style.BackgroundColor);
                    _flashingCells.Remove(cellViewModel);
                    continue;
                }

                // Determine the new brightness factor using sine wave-like behavior
                if (_increasingBrightness)
                {
                    _brightnessFactor += _brightnessSpeed; // Increase brightness
                    if (_brightnessFactor >= 1) _increasingBrightness = false; // Switch to decrease
                }
                else
                {
                    _brightnessFactor -= _brightnessSpeed; // Decrease brightness
                    if (_brightnessFactor <= 0) _increasingBrightness = true; // Switch to increase
                }

                // Calculate the new color based on the brightness factor
                Color newColor = AdjustBrightness(_startColor, _brightnessFactor);

                cellViewModel.BackgroundColor = new SolidColorBrush(newColor);
            }
            if (_flashingCells.Count == 0)
            {
                _timer?.Stop();
            }
        }

        // Function to adjust the brightness of a color based on a factor (0.0 to 1.0)
        private Color AdjustBrightness(Color baseColor, double brightnessFactor)
        {
            // Ensure that brightnessFactor is between 0 and 1
            brightnessFactor = Math.Max(0, Math.Min(1, brightnessFactor));

            // Apply the brightness adjustment by modifying each RGB component
            byte r = (byte)(baseColor.R * brightnessFactor);
            byte g = (byte)(baseColor.G * brightnessFactor);
            byte b = (byte)(baseColor.B * brightnessFactor);

            return Color.FromArgb(baseColor.A, r, g, b); // Return the new color with adjusted brightness
        }
    }
}
