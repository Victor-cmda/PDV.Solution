using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PDV.UI.WinUI3.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PDV.UI.WinUI3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private Frame _navigationFrame;
        public MainWindow()
        {
            this.InitializeComponent();

            if (contentFrame == null)
            {
                contentFrame = new Frame();
            }

            try
            {
                contentFrame.Navigate(typeof(HomePage));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro na navegação inicial: {ex}");
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                if (contentFrame == null)
                {
                    contentFrame = new Frame();
                    System.Diagnostics.Debug.WriteLine("contentFrame foi inicializado");
                }

                if (args?.SelectedItem == null)
                {
                    System.Diagnostics.Debug.WriteLine("SelectedItem é nulo");
                    return;
                }

                if (args.SelectedItem is NavigationViewItem selectedItem)
                {
                    string tag = selectedItem.Tag?.ToString();

                    if (string.IsNullOrEmpty(tag))
                    {
                        System.Diagnostics.Debug.WriteLine("Tag é nula ou vazia");
                        return;
                    }

                    Type pageType = null;
                    switch (tag)
                    {
                        case "home":
                            pageType = typeof(HomePage);
                            break;
                        case "products":
                            pageType = typeof(ProductPage);
                            break;
                    }

                    if (pageType != null)
                    {
                        try
                        {
                            contentFrame.Navigate(pageType);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Erro ao navegar para {pageType.Name}: {ex}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro na navegação: {ex}");
            }
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (contentFrame.CanGoBack)
            {
                contentFrame.GoBack();
            }
        }
    }
}
