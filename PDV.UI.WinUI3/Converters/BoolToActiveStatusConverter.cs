using Microsoft.UI.Xaml.Data;
using System;

namespace PDV.UI.WinUI3.Converters
{
    /// <summary>
    /// Conversor que transforma um valor booleano em um texto descritivo do status (Ativo ou Inativo)
    /// </summary>
    public class BoolToActiveStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isActive)
            {
                return isActive ? "Ativo" : "Inativo";
            }

            return "Desconhecido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string status)
            {
                return status == "Ativo";
            }

            return false;
        }
    }
} 