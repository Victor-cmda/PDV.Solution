using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using PDV.Domain.Enums;
using System;
using Windows.UI;

namespace PDV.UI.WinUI3.Converters
{
    /// <summary>
    /// Converte UserRole para uma cor representativa
    /// </summary>
    public class RoleToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is UserRole role)
            {
                return role switch
                {
                    UserRole.Admin => new SolidColorBrush(Color.FromArgb(255, 0, 120, 212)),      // Azul
                    UserRole.Manager => new SolidColorBrush(Color.FromArgb(255, 16, 124, 16)),    // Verde
                    UserRole.Salesperson => new SolidColorBrush(Color.FromArgb(255, 202, 80, 16)), // Laranja
                    UserRole.Cashier => new SolidColorBrush(Color.FromArgb(255, 130, 26, 201)),   // Roxo
                    UserRole.Stockist => new SolidColorBrush(Color.FromArgb(255, 77, 79, 84)),    // Cinza
                    _ => new SolidColorBrush(Color.FromArgb(255, 104, 118, 138)),                 // Cinza azulado
                };
            }

            return new SolidColorBrush(Color.FromArgb(255, 104, 118, 138));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte boolean para Visibility
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }

            return false;
        }
    }

    /// <summary>
    /// Converte boolean para Visibility (inverso)
    /// </summary>
    public class BoolToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }

            return true;
        }
    }

    /// <summary>
    /// Converte DateTime para string relativa (ex: "Há 2 dias", "Há 5 meses")
    /// </summary>
    public class DateTimeToRelativeTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dateTime)
            {
                var now = DateTime.Now;
                var timeSpan = now - dateTime;

                if (timeSpan.TotalDays < 1)
                {
                    if (timeSpan.TotalHours < 1)
                    {
                        var minutes = (int)timeSpan.TotalMinutes;
                        return $"Há {minutes} {(minutes == 1 ? "minuto" : "minutos")}";
                    }
                    else
                    {
                        var hours = (int)timeSpan.TotalHours;
                        return $"Há {hours} {(hours == 1 ? "hora" : "horas")}";
                    }
                }
                else if (timeSpan.TotalDays < 30)
                {
                    var days = (int)timeSpan.TotalDays;
                    return $"Há {days} {(days == 1 ? "dia" : "dias")}";
                }
                else if (timeSpan.TotalDays < 365)
                {
                    var months = (int)(timeSpan.TotalDays / 30);
                    return $"Há {months} {(months == 1 ? "mês" : "meses")}";
                }
                else
                {
                    var years = (int)(timeSpan.TotalDays / 365);
                    return $"Há {years} {(years == 1 ? "ano" : "anos")}";
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte UserRole para um ícone correspondente
    /// </summary>
    public class UserRoleToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is UserRole role)
            {
                return role switch
                {
                    UserRole.Admin => "\uE7EE",     // Ícone de coroa
                    UserRole.Manager => "\uE7C9",   // Ícone de pessoas
                    UserRole.Salesperson => "\uE719", // Ícone de loja
                    UserRole.Cashier => "\uE8C0",   // Ícone de calculadora
                    UserRole.Stockist => "\uE7B8",  // Ícone de caixa
                    _ => "\uE77B",                  // Ícone de pessoa
                };
            }

            return "\uE77B"; // Ícone de pessoa como padrão
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte nome completo para iniciais (ex: "João Silva" para "JS")
    /// </summary>
    public class NameToInitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string name && !string.IsNullOrEmpty(name))
            {
                var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                    return "";

                if (parts.Length == 1)
                    return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

                return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte DateTime para string formatada (dd/MM/yyyy)
    /// </summary>
    public class DateToFormattedStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("dd/MM/yyyy");
            }

            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte contagem para texto descritivo
    /// </summary>
    public class CountToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                if (count == 0)
                    return "Nenhum funcionário";
                else if (count == 1)
                    return "1 funcionário";
                else
                    return $"{count} funcionários";
            }

            return "0 funcionários";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}