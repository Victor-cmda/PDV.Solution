using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace PDV.UI.WinUI3.Services
{
    /// <summary>
    /// Serviço para exibição de diálogos e mensagens
    /// </summary>
    public static class DialogService
    {
        /// <summary>
        /// Exibe um diálogo de erro
        /// </summary>
        public static async Task ShowErrorAsync(string title, string message, XamlRoot xamlRoot = null)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot ?? App.MainWindow?.Content?.XamlRoot
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Exibe um diálogo de informação
        /// </summary>
        public static async Task ShowInfoAsync(string title, string message, XamlRoot xamlRoot = null)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot ?? App.MainWindow?.Content?.XamlRoot
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Exibe um diálogo de confirmação
        /// </summary>
        public static async Task<bool> ShowConfirmationAsync(string title, string message, string primaryButtonText = "Sim", string closeButtonText = "Não", XamlRoot xamlRoot = null)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText,
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot ?? App.MainWindow?.Content?.XamlRoot
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }
    }

    /// <summary>
    /// Classe para gerenciar configurações do aplicativo
    /// </summary>
    public static class AppSettings
    {
        private static Windows.Storage.ApplicationDataContainer LocalSettings =>
            Windows.Storage.ApplicationData.Current.LocalSettings;

        /// <summary>
        /// Obtém um valor da configuração
        /// </summary>
        public static T GetValue<T>(string key, T defaultValue = default)
        {
            if (LocalSettings.Values.TryGetValue(key, out object value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Define um valor na configuração
        /// </summary>
        public static void SetValue<T>(string key, T value)
        {
            LocalSettings.Values[key] = value;
        }

        /// <summary>
        /// Remove uma configuração
        /// </summary>
        public static void RemoveValue(string key)
        {
            if (LocalSettings.Values.ContainsKey(key))
            {
                LocalSettings.Values.Remove(key);
            }
        }
    }
}