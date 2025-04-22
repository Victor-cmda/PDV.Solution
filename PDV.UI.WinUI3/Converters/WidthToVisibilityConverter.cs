using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace PDV.UI.WinUI3.Converters
{
    /// <summary>
    /// Conversor que mostra ou esconde elementos com base na largura disponível
    /// </summary>
    public class WidthToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double width && parameter is string minWidthStr)
            {
                if (double.TryParse(minWidthStr, out double minWidth))
                {
                    return width >= minWidth ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 