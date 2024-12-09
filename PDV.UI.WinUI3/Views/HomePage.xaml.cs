using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PDV.UI.WinUI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Configurar posições iniciais
            TitlePanel.Translation = new System.Numerics.Vector3(0, -50, 0);
            SalesButton.Translation = new System.Numerics.Vector3(0, 50, 0);
            ProductsButton.Translation = new System.Numerics.Vector3(0, 50, 0);
            CustomersButton.Translation = new System.Numerics.Vector3(0, 50, 0);
            ReportsButton.Translation = new System.Numerics.Vector3(0, 50, 0);

            // Iniciar animações
            PageLoadAnimation.Begin();

            // Animar posições
            AnimateElement(TitlePanel, 0);
            AnimateElement(SalesButton, 200);
            AnimateElement(ProductsButton, 300);
            AnimateElement(CustomersButton, 400);
            AnimateElement(ReportsButton, 500);
        }

        private void AnimateElement(UIElement element, int delay)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                var animation = element.Translation;
                animation.Y = 0;
                element.Translation = animation;
            });
        }

        private void NavigateToSales(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(POSPage), null, new DrillInNavigationTransitionInfo());
        }

        private void NavigateToProducts(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProductsPage), null, new DrillInNavigationTransitionInfo());
        }

        private void NavigateToCustomers(object sender, RoutedEventArgs e)
        {
            //Frame.Navigate(typeof(CustomersPage), null, new DrillInNavigationTransitionInfo());
        }

        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ReportsPage), null, new DrillInNavigationTransitionInfo());
        }

    }
}

