using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using PDV.UI.WinUI3.Services;
using PDV.UI.WinUI3.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System;

namespace PDV.UI.WinUI3
{
    public sealed partial class MainWindow : Window
    {
        private Dictionary<string, Type> _pages;

        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(CustomHeader);

            // Initialize the global notification service
            NotificationService.Instance.Initialize(GlobalNotification);

            // Initialize page mapping
            InitializePages();

            // Setup events
            SetupEvents();

            // Update date display
            UpdateDateDisplay();
        }

        private void InitializePages()
        {
            _pages = new Dictionary<string, Type>
            {
                { "login", typeof(LoginPage) },
                { "home", typeof(HomePage) },
                { "employees", typeof(EmployeesPage) },
                { "products", typeof(ProductsPage) },
                { "reports", typeof(ReportsPage) },
                { "pos", typeof(POSPage) }
                // Add more pages here as needed
            };
        }

        private void SetupEvents()
        {
            // Subscribe to session events
            SessionService.Instance.UserLoggedIn += SessionService_UserLoggedIn;
            SessionService.Instance.UserLoggedOut += SessionService_UserLoggedOut;
        }

        private void SessionService_UserLoggedIn(object sender, EventArgs e)
        {
            // Update UI for logged in user
            UpdateUserInfo();
        }

        private void SessionService_UserLoggedOut(object sender, EventArgs e)
        {
            // Navigate to login page
            ContentFrame.Navigate(typeof(LoginPage), null, new DrillInNavigationTransitionInfo());

            // Hide navigation elements
            NavView.IsPaneVisible = false;
        }

        private void UpdateUserInfo()
        {
            var user = SessionService.Instance.CurrentUser;
            if (user != null)
            {
                // Update user info in the UI
                UserNameTextBlock.Text = user.Name;
                UserInitialsControl.Initials = GetInitials(user.Name);

                // Show navigation elements
                NavView.IsPaneVisible = true;

                // Update permissions based on user role
                UpdateNavViewBasedOnPermissions();
            }
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "?";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpper();
            else
                return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
        }

        private void UpdateNavViewBasedOnPermissions()
        {
            // Hide/show navigation items based on user permissions
            // This would be implemented based on the permissions system
        }

        private void UpdateDateDisplay()
        {
            CurrentDateText.Text = DateTime.Now.ToString("dd MMMM yyyy");
        }

        public void NavigateToHomePage()
        {
            // Show the NavigationView
            NavView.IsPaneVisible = true;

            // Update page title
            PageTitle.Text = "Dashboard";

            // Navigate to homepage
            ContentFrame.Navigate(typeof(HomePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

            // Select the home item in navigation
            foreach (NavigationViewItem item in NavView.MenuItems)
            {
                if (item.Tag?.ToString() == "home")
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }

            // Update user info
            UpdateUserInfo();
        }

        public void NavigateToLoginPage()
        {
            // Hide the NavigationView
            NavView.IsPaneVisible = false;

            // Navigate to login page
            ContentFrame.Navigate(typeof(LoginPage), null, new DrillInNavigationTransitionInfo());
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer is NavigationViewItem item && item.Tag != null)
            {
                string tag = item.Tag.ToString();

                // Update page title
                PageTitle.Text = item.Content.ToString();

                // Handle special cases
                if (tag == "sync")
                {
                    // Handle synchronization directly
                    SyncButton_Tapped(null, null);
                    return;
                }

                // Navigate to the selected page
                if (_pages.TryGetValue(tag, out Type pageType))
                {
                    ContentFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Encerra a sessão do usuário
            SessionService.Instance.Logout();

            // Notifica o usuário
            NotificationService.Instance.ShowInformation("Logout realizado com sucesso.");

            // Navega para a página de login
            NavigateToLoginPage();
        }

        private void NavView_PaneOpening(NavigationView sender, object args)
        {
            // Any logic needed when navigation pane opens
        }

        private void SyncButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Show synchronization in progress notification
            NotificationService.Instance.ShowInformation("Sincronização iniciada. Aguarde...");

            // In a real app, you would call the sync service here
            // For demo purposes, we'll simulate a successful sync
            DispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(2000); // Simulate processing time
                NotificationService.Instance.ShowSuccess("Sincronização concluída com sucesso!");
            });
        }
    }
}