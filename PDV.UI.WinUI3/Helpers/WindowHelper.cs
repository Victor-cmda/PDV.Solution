using Microsoft.UI.Xaml;

namespace PDV.UI.WinUI3.Helpers
{
    public static class WindowHelper
    {
        private static Window _mainWindow;

        public static Window MainWindow
        {
            get => _mainWindow;
            set => _mainWindow = value;
        }
    }
}
