using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace AdRev.Desktop
{
    /// <summary>
    /// Converter to determine if window width should trigger compact mode
    /// </summary>
    public class WidthToCompactConverter : IValueConverter
    {
        private const double CompactModeThreshold = 1280;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                return width < CompactModeThreshold;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for responsive font sizing based on DPI
    /// </summary>
    public class DpiToFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double baseFontSize && parameter is string multiplierStr)
            {
                if (double.TryParse(multiplierStr, out double multiplier))
                {
                    // Get DPI scaling factor
                    var mainWindow = Application.Current?.MainWindow;
                    if (mainWindow != null)
                    {
                        var dpiScale = VisualTreeHelper.GetDpi(mainWindow).DpiScaleX;
                        return baseFontSize * multiplier * dpiScale;
                    }
                }
            }
            return 14.0; // Default font size
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for responsive spacing based on available space
    /// </summary>
    public class ResponsiveSpacingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                // Scale spacing based on width
                if (width < 1024) return new Thickness(8);
                if (width < 1366) return new Thickness(16);
                return new Thickness(24);
            }
            return new Thickness(16);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
