using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace PDV.UI.WinUI3.Views
{
    public sealed partial class HomePage : Page
    {
        private DispatcherTimer _clockTimer;

        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Start the entrance animations
            PageLoadAnimation.Begin();

            // Start the continuous background animation
            ContinuousAnimation.Begin();

            // Initialize and start the clock
            InitializeClock();

            // Start the clock animation
            ClockAnimation.Begin();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Stop timers when navigating away
            if (_clockTimer != null)
            {
                _clockTimer.Stop();
            }

            // Stop animations
            ContinuousAnimation.Stop();
            ClockAnimation.Stop();
        }

        private void InitializeClock()
        {
            // Update date and time initially
            UpdateDateTime();

            // Create timer to update the clock
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, e) => UpdateDateTime();
            _clockTimer.Start();
        }

        private void UpdateDateTime()
        {
            var now = DateTime.Now;

            // Update the date display with day of week and date
            CurrentDate.Text = now.ToString("dddd, d 'de' MMMM");

            // Update the time display
            CurrentTime.Text = now.ToString("HH:mm:ss");
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
            // Uncomment when the page is available
            // Frame.Navigate(typeof(CustomersPage), null, new DrillInNavigationTransitionInfo());
        }

        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ReportsPage), null, new DrillInNavigationTransitionInfo());
        }
    }
}