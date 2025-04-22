using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace PDV.UI.WinUI3.Converters
{
    /// <summary>
    /// Conversor que transforma um valor booleano em uma cor de status (verde para ativo, vermelho para inativo)
    /// </summary>
    public class BoolToStatusBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isActive)
            {
                if (isActive)
                {
                    // Verde para ativo
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 187, 23));
                }
                else
                {
                    // Vermelho para inativo
                    return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 232, 17, 35));
                }
            }

            // Cinza para indefinido
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 128, 128, 128));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
} 